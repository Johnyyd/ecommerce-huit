-- =====================================================
-- 8. STORED PROCEDURES
-- =====================================================

-- -----------------------------------------------------
-- sp_CreateOrder
-- Tạo đơn hàng mới từ giỏ hàng của người dùng
-- Logic:
-- - Validate giỏ hàng (quantity > 0)
-- - Calculate subtotal, voucher discount, tax, shipping, total
-- - Kiểm tra tồn kho: reserve quantity
-- - Tạo Order, OrderItems, giảm inventory (reserved)
-- - Ghi log vào order_status_history với trạng thái PENDING
-- - Xóa cart items sau khi đặt hàng
-- =====================================================
CREATE OR ALTER PROCEDURE sp_CreateOrder
    @UserID INT,
    @ShippingAddress NVARCHAR(MAX), -- JSON string
    @PaymentMethod VARCHAR(20),
    @VoucherCode VARCHAR(50) = NULL,
    @OrderItemsJSON NVARCHAR(MAX) -- JSON array: [{variant_id, quantity, price_at_time}]
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @OrderCode VARCHAR(20);
    DECLARE @OrderID INT;
    DECLARE @ subtotal DECIMAL(15,2) = 0;
    DECLARE @TaxAmount DECIMAL(15,2) = 0;
    DECLARE @ShippingFee DECIMAL(10,2) = 0;
    DECLARE @DiscountAmount DECIMAL(15,2) = 0;
    DECLARE @TotalAmount DECIMAL(15,2) = 0;
    DECLARE @VoucherID INT = NULL;
    DECLARE @WarehouseID INT = 1; -- Mặc định warehouse 1 (có thể mở rộng cho multi-warehouse)

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Validate user exists
        IF NOT EXISTS (SELECT 1 FROM users WHERE id = @UserID AND status = 'ACTIVE')
        BEGIN
            RAISERROR('User không tồn tại hoặc không hoạt động', 16, 1);
            RETURN;
        END

        -- Validate payment method
        IF @PaymentMethod NOT IN ('CASH', 'COD', 'BANK_TRANSFER', 'MOMO', 'VNPAY')
        BEGIN
            RAISERROR('Phương thức thanh toán không hợp lệ', 16, 1);
            RETURN;
        END

        -- Validate voucher if provided
        IF @VoucherCode IS NOT NULL
        BEGIN
            SELECT @VoucherID = id FROM vouchers
            WHERE code = @VoucherCode
                AND is_active = 1
                AND (start_date IS NULL OR start_date <= GETDATE())
                AND (end_date IS NULL OR end_date >= GETDATE())
                AND (max_uses IS NULL OR times_used < max_uses)
                AND (min_order_amount IS NULL OR @ subtotal >= min_order_amount);

            IF @VoucherID IS NULL
            BEGIN
                RAISERROR('Voucher không hợp lệ hoặc đã hết hạn', 16, 1);
                RETURN;
            END
        END

        -- Parse order items
        -- Using OPENJSON requires @OrderItemsJSON to be a valid JSON array
        -- Example: [{"variant_id":1,"quantity":2,"price_at_time":1000000},{"variant_id":2,"quantity":1,"price_at_time":2000000}]

        -- Validate stock and calculate subtotal in one pass
        ;WITH OrderItems AS (
            SELECT
                value as item_json
            FROM OPENJSON(@OrderItemsJSON)
        )
        SELECT @Subtotal = SUM(JSON_VALUE(item_json, '$.quantity') * JSON_VALUE(item_json, '$.price_at_time'))
        FROM OrderItems;

        -- Apply voucher discount (percentage or fixed)
        IF @VoucherID IS NOT NULL
        BEGIN
            DECLARE @VoucherType VARCHAR(20), @VoucherValue DECIMAL(10,2), @MaxDiscount DECIMAL(15,2) = NULL;

            SELECT @VoucherType = discount_type, @VoucherValue = discount_value, @MaxDiscount = max_discount_amount
            FROM vouchers WHERE id = @VoucherID;

            IF @VoucherType = 'PERCENT'
            BEGIN
                SET @DiscountAmount = @Subtotal * (@VoucherValue / 100);
                IF @MaxDiscount IS NOT NULL AND @DiscountAmount > @MaxDiscount
                    SET @DiscountAmount = @MaxDiscount;
            END
            ELSE IF @VoucherType = 'FIXED'
            BEGIN
                SET @DiscountAmount = @VoucherValue;
            END

            -- Ensure discount not exceed subtotal
            IF @DiscountAmount > @Subtotal SET @DiscountAmount = @Subtotal;
        END

        -- Calculate shipping (simplified: free if > 500k)
        SET @ShippingFee = CASE WHEN @Subtotal >= 500000 THEN 0 ELSE 30000 END;
        -- TODO: Could use stored procedure parameter or lookup table

        SET @TaxAmount = @Subtotal * 0.1; -- 10% VAT
        SET @TotalAmount = @Subtotal - @DiscountAmount + @ShippingFee + @TaxAmount;

        -- Generate order code
        SET @OrderCode = 'ORD' + FORMAT(GETDATE(), 'yyyyMMdd') + RIGHT('00000' + CAST(
            (SELECT ISNULL(MAX(CAST(SUBSTRING(code, 13, 5) AS INT)), 0) + 1
             FROM orders
             WHERE code LIKE 'ORD' + FORMAT(GETDATE(), 'yyyyMMdd') + '%'
        ) AS VARCHAR(5)), 5);

        -- Insert Order
        INSERT INTO orders (
            user_id,
            code,
            subtotal,
            tax_amount,
            shipping_fee,
            discount_amount,
            total_amount,
            status,
            payment_method,
            payment_status,
            shipping_address,
            warehouse_id
        ) VALUES (
            @UserID,
            @OrderCode,
            @Subtotal,
            @TaxAmount,
            @ShippingFee,
            @DiscountAmount,
            @TotalAmount,
            'PENDING', -- initial status
            @PaymentMethod,
            CASE WHEN @PaymentMethod = 'COD' THEN 'PENDING' ELSE 'PAID' END,
            @ShippingAddress,
            @WarehouseID
        );

        SET @OrderID = SCOPE_IDENTITY();

        -- Insert Order Items with stock reservation
        ;WITH Items AS (
            SELECT
                JSON_VALUE(item_json, '$.variant_id') as variant_id,
                JSON_VALUE(item_json, '$.quantity') as quantity,
                JSON_VALUE(item_json, '$.price_at_time') as price_at_time,
                ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) as row_num
            FROM OPENJSON(@OrderItemsJSON)
        )
        INSERT INTO order_items (order_id, variant_id, quantity, price_at_time, subtotal)
        SELECT
            @OrderID as order_id,
            i.variant_id,
            i.quantity,
            i.price_at_time,
            i.quantity * i.price_at_time
        FROM Items i;

        -- Reserve inventory for each item (pessimistic locking)
        -- This ensures we don't oversell
        UPDATE i
        SET
            i.quantity_reserved = i.quantity_reserved + Items.quantity
        FROM inventories i
        INNER JOIN Items ON i.variant_id = Items.variant_id AND i.warehouse_id = @WarehouseID
        WHERE i.quantity_available >= Items.quantity; -- Check available

        -- Check if any item failed to reserve (should not happen if we validate before)
        IF @@ROWCOUNT < (SELECT COUNT(*) FROM Items)
        BEGIN
            RAISERROR('Không đủ tồn kho để đặt hàng cho một số sản phẩm', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Create stock movement records (reserve type)
        INSERT INTO stock_movement (warehouse_id, variant_id, movement_type, quantity, reference_type, reference_id, note)
        SELECT
            @WarehouseID,
            Items.variant_id,
            'SALE_RESERVED',
            Items.quantity,
            'ORDER',
            @OrderID,
            'Reserve for order ' + @OrderCode
        FROM Items;

        -- Update voucher usage if applied
        IF @VoucherID IS NOT NULL
        BEGIN
            INSERT INTO voucher_usages (voucher_id, user_id, order_id, discount_amount) VALUES
            (@VoucherID, @UserID, @OrderID, @DiscountAmount);

            UPDATE vouchers
            SET times_used = times_used + 1
            WHERE id = @VoucherID;
        END

        -- Insert order status history (PENDING)
        INSERT INTO order_status_history (order_id, status, note) VALUES
        (@OrderID, 'PENDING', 'Đơn hàng được tạo');

        -- Clear cart items (optional: could keep for history)
        DELETE ci FROM cart_items ci
        INNER JOIN carts c ON ci.cart_id = c.id
        WHERE c.user_id = @UserID;

        -- Commit
        COMMIT TRANSACTION;

        -- Return order info
        SELECT @OrderID as OrderID, @OrderCode as OrderCode;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @ErrorMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMsg, 16, 1);
        RETURN;
    END CATCH
END
GO

-- -----------------------------------------------------
-- sp_ConfirmOrder
-- Xác nhận đơn hàng (chuyển PENDING → CONFIRMED)
-- =====================================================
CREATE OR ALTER PROCEDURE sp_ConfirmOrder
    @OrderID INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Check order exists and is PENDING
        IF NOT EXISTS (
            SELECT 1 FROM orders
            WHERE id = @OrderID AND status = 'PENDING'
        )
        BEGIN
            RAISERROR('Đơn hàng không tồn tại hoặc không ở trạng thái PENDING', 16, 1);
            RETURN;
        END

        -- Update order status
        UPDATE orders
        SET status = 'CONFIRMED',
            updated_at = GETDATE()
        WHERE id = @OrderID;

        -- Insert status history
        INSERT INTO order_status_history (order_id, status, note) VALUES
        (@OrderID, 'CONFIRMED', 'Đơn hàng đã được xác nhận');

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @ErrorMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMsg, 16, 1);
        RETURN;
    END CATCH
END
GO

-- -----------------------------------------------------
-- sp_ShipOrder
-- Vận chuyển đơn hàng (chuyển CONFIRMED → SHIPPING)
-- Logic: Gán serial numbers (nếu có) và tạo tracking number
-- =====================================================
CREATE OR ALTER PROCEDURE sp_ShipOrder
    @OrderID INT,
    @TrackingNumber VARCHAR(100) = NULL,
    @ShippingProvider NVARCHAR(100) = NULL,
    @UserID INT = NULL -- người thực hiện (nhân viên kho)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Validate order
        IF NOT EXISTS (
            SELECT 1 FROM orders
            WHERE id = @OrderID AND status = 'CONFIRMED'
        )
        BEGIN
            RAISERROR('Đơn hàng không tồn tại hoặc chưa được xác nhận', 16, 1);
            RETURN;
        END

        -- Get order items
        DECLARE @OrderCode VARCHAR(20);
        SELECT @OrderCode = code FROM orders WHERE id = @OrderID;

        -- Check if order items require serial numbers
        -- Get all order items with their variants and check if those variants have product_serials
        ;WITH OrderItems AS (
            SELECT oi.id as order_item_id, oi.variant_id, v.product_id
            FROM order_items oi
            INNER JOIN product_variants v ON oi.variant_id = v.id
            WHERE oi.order_id = @OrderID AND oi.quantity > 0
        )
        -- Find which order items need serial assignment
        DECLARE @NeedsSerial BIT = 0;
        SELECT @NeedsSerial = CASE WHEN COUNT(*) > 0 THEN 1 ELSE 0 END
        FROM OrderItems oi
        INNER JOIN products p ON oi.product_id = p.id
        WHERE EXISTS (
            SELECT 1 FROM product_serials ps
            WHERE ps.variant_id = oi.variant_id
        );

        IF @NeedsSerial = 1
        BEGIN
            -- Allocate serial numbers for order items
            -- We need to assign one serial per quantity in each order item
            -- Example: order_item quantity 2 → assign 2 serial numbers
            DECLARE @AssignedCount INT = 0;

            -- Cursor through order items needing serials
            DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
            SELECT oi.id, oi.variant_id, oi.quantity
            FROM OrderItems oi
            INNER JOIN products p ON oi.product_id = p.id
            WHERE EXISTS (
                SELECT 1 FROM product_serials ps
                WHERE ps.variant_id = oi.variant_id
            );

            DECLARE @OrderItemID INT, @VariantID INT, @Qty INT;
            OPEN cur;
            FETCH NEXT FROM cur INTO @OrderItemID, @VariantID, @Qty;

            WHILE @@FETCH_STATUS = 0
            BEGIN
                -- Get available serials for this variant with UPDLOCK
                DECLARE @SerialIDs TABLE (id INT);
                INSERT INTO @SerialIDs
                SELECT TOP (@Qty) id
                FROM product_serials WITH (UPDLOCK, HOLDLOCK)
                WHERE variant_id = @VariantID AND status = 'AVAILABLE'
                ORDER BY id;

                IF (SELECT COUNT(*) FROM @SerialIDs) < @Qty
                BEGIN
                    RAISERROR('Không đủ số serial/Imei tồn kho cho variant %d', 16, 1, @VariantID);
                    ROLLBACK TRANSACTION;
                    CLOSE cur;
                    DEALLOCATE cur;
                    RETURN;
                END

                -- Assign each serial to order item
                INSERT INTO order_item_serials (order_item_id, serial_id, assigned_at)
                SELECT @OrderItemID, id, GETDATE()
                FROM @SerialIDs;

                -- Update serial status to RESERVED
                UPDATE ps
                SET status = 'RESERVED'
                FROM product_serials ps
                INNER JOIN @SerialIDs s ON ps.id = s.id;

                SET @AssignedCount = @AssignedCount + @Qty;
                FETCH NEXT FROM cur INTO @OrderItemID, @VariantID, @Qty;
            END

            CLOSE cur;
            DEALLOCATE cur;
        END

        -- Update order status to SHIPPING
        UPDATE orders
        SET status = 'SHIPPING',
            tracking_number = @TrackingNumber,
            shipping_provider = @ShippingProvider,
            shipped_at = GETDATE(),
            updated_at = GETDATE()
        WHERE id = @OrderID;

        -- Insert status history
        INSERT INTO order_status_history (order_id, status, note, created_by) VALUES
        (@OrderID, 'SHIPPING', 'Đơn hàng đã được gửi vận chuyển', @UserID);

        COMMIT TRANSACTION;

        SELECT @OrderID as OrderID, @OrderCode as OrderCode, @AssignedCount as SerialsAssigned;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @ErrorMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMsg, 16, 1);
        RETURN;
    END CATCH
END
GO

-- -----------------------------------------------------
-- sp_CompleteOrder
-- Hoàn thành đơn hàng (chuyển SHIPPING → COMPLETED)
-- =====================================================
CREATE OR ALTER PROCEDURE sp_CompleteOrder
    @OrderID INT,
    @UserID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Validate order
        IF NOT EXISTS (
            SELECT 1 FROM orders
            WHERE id = @OrderID AND status = 'SHIPPING'
        )
        BEGIN
            RAISERROR('Đơn hàng không tồn tại hoặc chưa được vận chuyển', 16, 1);
            RETURN;
        END

        -- Update order status
        UPDATE orders
        SET status = 'COMPLETED',
            completed_at = GETDATE(),
            updated_at = GETDATE()
        WHERE id = @OrderID;

        -- Move reserved → sold (deduct inventory reserved quantity)
        -- This is the final inventory deduction
        UPDATE i
        SET
            i.quantity_on_hand = i.quantity_on_hand - oi.quantity,
            i.quantity_reserved = i.quantity_reserved - oi.quantity
        FROM inventories i
        INNER JOIN order_items oi ON i.variant_id = oi.variant_id
        INNER JOIN orders o ON oi.order_id = o.id
        WHERE o.id = @OrderID AND o.warehouse_id = i.warehouse_id;

        -- Update serial status: RESERVED → SOLD
        UPDATE ps
        SET status = 'SOLD'
        FROM product_serials ps
        INNER JOIN order_item_serials ois ON ps.id = ois.serial_id
        INNER JOIN order_items oi ON ois.order_item_id = oi.id
        WHERE oi.order_id = @OrderID;

        -- Create stock movement for deduction
        INSERT INTO stock_movement (warehouse_id, variant_id, movement_type, quantity, reference_type, reference_id, note)
        SELECT
            o.warehouse_id,
            oi.variant_id,
            'SALE_SHIP',
            oi.quantity,
            'ORDER',
            @OrderID,
            'Deduct inventory for completed order'
        FROM order_items oi
        INNER JOIN orders o ON oi.order_id = o.id
        WHERE o.id = @OrderID;

        -- Insert status history
        INSERT INTO order_status_history (order_id, status, note, created_by) VALUES
        (@OrderID, 'COMPLETED', 'Đơn hàng đã hoàn thành', @UserID);

        COMMIT TRANSACTION;

        SELECT @OrderID as OrderID;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @ErrorMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMsg, 16, 1);
        RETURN;
    END CATCH
END
GO

-- -----------------------------------------------------
-- sp_CancelOrder
-- Hủy đơn hàng (chỉ hủy được khi ở PENDING/CONFIRMED)
-- Hoàn trả tồn kho (reserved quantity)
-- =====================================================
CREATE OR ALTER PROCEDURE sp_CancelOrder
    @OrderID INT,
    @Reason NVARCHAR(255) = NULL,
    @UserID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Validate order status
        IF NOT EXISTS (
            SELECT 1 FROM orders
            WHERE id = @OrderID AND status IN ('PENDING', 'CONFIRMED')
        )
        BEGIN
            RAISERROR('Không thể hủy đơn hàng ở trạng thái hiện tại', 16, 1);
            RETURN;
        END

        -- Update order status
        UPDATE orders
        SET status = 'CANCELLED',
            cancelled_at = GETDATE(),
            updated_at = GETDATE()
        WHERE id = @OrderID;

        -- Return reserved inventory
        UPDATE i
        SET
            i.quantity_reserved = i.quantity_reserved - oi.quantity
        FROM inventories i
        INNER JOIN order_items oi ON i.variant_id = oi.variant_id
        INNER JOIN orders o ON oi.order_id = o.id
        WHERE o.id = @OrderID AND o.warehouse_id = i.warehouse_id;

        -- Update serial status: if reserved, return to AVAILABLE
        UPDATE ps
        SET status = 'AVAILABLE'
        FROM product_serials ps
        INNER JOIN order_item_serials ois ON ps.id = ois.serial_id
        INNER JOIN order_items oi ON ois.order_item_id = oi.id
        WHERE oi.order_id = @OrderID;

        -- Insert stock movement (return reason)
        INSERT INTO stock_movement (warehouse_id, variant_id, movement_type, quantity, reference_type, reference_id, note)
        SELECT
            o.warehouse_id,
            oi.variant_id,
            'ADJUSTMENT_IN',
            oi.quantity,
            'ORDER',
            @OrderID,
            ISNULL(@Reason, 'Hủy đơn hàng - hoàn trả tồn kho')
        FROM order_items oi
        INNER JOIN orders o ON oi.order_id = o.id
        WHERE o.id = @OrderID;

        -- Insert status history
        INSERT INTO order_status_history (order_id, status, note, created_by) VALUES
        (@OrderID, 'CANCELLED', ISNULL(@Reason, 'Đơn hàng bị hủy'), @UserID);

        -- Refund if payment already made (optional complexity)
        -- For now, just mark payment as refunded if applicable
        UPDATE payments
        SET status = 'REFUNDED'
        WHERE order_id = @OrderID AND status = 'PAID';

        COMMIT TRANSACTION;

        SELECT @OrderID as OrderID;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @ErrorMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMsg, 16, 1);
        RETURN;
    END CATCH
END
GO

-- -----------------------------------------------------
-- sp_ProcessReturn
-- Xử lý trả hàng (chuyển COMPLETED → RETURNED/REJECTED)
-- =====================================================
CREATE OR ALTER PROCEDURE sp_ProcessReturn
    @ReturnID INT,
    @Action VARCHAR(20), -- 'APPROVE' or 'REJECT'
    @StaffID INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Get return request
        DECLARE @OrderID INT, @Status VARCHAR(20), @Reason NVARCHAR(255);
        SELECT @OrderID = order_id, @Status = status, @Reason = reason
        FROM returns
        WHERE id = @ReturnID;

        IF @OrderID IS NULL
        BEGIN
            RAISERROR('Yêu cầu trả hàng không tồn tại', 16, 1);
            RETURN;
        END

        IF @Status <> 'REQUESTED'
        BEGIN
            RAISERROR('Yêu cầu trả hàng đã được xử lý', 16, 1);
            RETURN;
        END

        IF @Action NOT IN ('APPROVE', 'REJECT')
        BEGIN
            RAISERROR('Hành động không hợp lệ', 16, 1);
            RETURN;
        END

        -- Update return status
        UPDATE returns
        SET status = CASE WHEN @Action = 'APPROVE' THEN 'APPROVED' ELSE 'REJECTED' END,
            processed_at = GETDATE(),
            processed_by = @StaffID,
            admin_note = @Reason
        WHERE id = @ReturnID;

        -- If approve, create stock movement to return inventory
        IF @Action = 'APPROVE'
        BEGIN
            -- Get return items
            ;WITH ReturnItems AS (
                SELECT ri.variant_id, ri.quantity, ri.order_item_id
                FROM return_items ri
                WHERE ri.return_id = @ReturnID
            )
            -- Return inventory (increase on hand)
            UPDATE i
            SET
                i.quantity_on_hand = i.quantity_on_hand + ri.quantity,
                -- If serials exist, they will be handled separately
                i.quantity_reserved = i.quantity_reserved -- unchanged
            FROM inventories i
            INNER JOIN ReturnItems ri ON i.variant_id = ri.variant_id
            INNER JOIN orders o ON i.warehouse_id = o.warehouse_id
            WHERE o.id = @OrderID;

            -- Update serial status: SOLD → RETURNED
            UPDATE ps
            SET status = 'RETURNED'
            FROM product_serials ps
            INNER JOIN return_items ri ON ps.id = ri.serial_id
            WHERE ri.return_id = @ReturnID;

            -- Create stock movement for return
            INSERT INTO stock_movement (warehouse_id, variant_id, movement_type, quantity, reference_type, reference_id, note)
            SELECT
                o.warehouse_id,
                ri.variant_id,
                'RETURN',
                ri.quantity,
                'RETURN',
                @ReturnID,
                'Return approved for order ' + o.code
            FROM return_items ri
            INNER JOIN orders o ON ri.order_item_id IN (
                SELECT order_item_id FROM return_items WHERE return_id = @ReturnID
            ) -- rough mapping; better join with order_items

            -- Insert status history for order
            INSERT INTO order_status_history (order_id, status, note, created_by) VALUES
            (@OrderID, 'RETURNED', 'Trả hàng thành công', @StaffID);
        END
        ELSE
        BEGIN
            -- Reject: nothing to do with inventory
            INSERT INTO order_status_history (order_id, status, note, created_by) VALUES
            (@OrderID, 'RETURN_REJECTED', 'Yêu cầu trả hàng bị từ chối', @StaffID);
        END

        COMMIT TRANSACTION;

        SELECT @ReturnID as ReturnID;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @ErrorMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMsg, 16, 1);
        RETURN;
    END CATCH
END
GO

-- -----------------------------------------------------
-- sp_ImportStock
-- Nhập kho hàng loạt từ supplier (dùng cho admin/warehouse)
-- =====================================================
CREATE OR ALTER PROCEDURE sp_ImportStock
    @SupplierID INT,
    @WarehouseID INT,
    @ItemsJSON NVARCHAR(MAX), -- JSON array: [{"variant_id":1,"quantity":100,"unit_cost":500000,"serial_numbers":["12345"]}]
    @Note NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Validate supplier
        IF NOT EXISTS (SELECT 1 FROM suppliers WHERE id = @SupplierID)
        BEGIN
            RAISERROR('Nhà cung cấp không tồn tại', 16, 1);
            RETURN;
        END

        -- Validate warehouse
        IF NOT EXISTS (SELECT 1 FROM warehouses WHERE id = @WarehouseID)
        BEGIN
            RAISERROR('Kho không tồn tại', 16, 1);
            RETURN;
        END

        -- Parse items
        ;WITH Items AS (
            SELECT
                JSON_VALUE(item_json, '$.variant_id') as variant_id,
                JSON_VALUE(item_json, '$.quantity') as quantity,
                JSON_VALUE(item_json, '$.unit_cost') as unit_cost
            FROM OPENJSON(@ItemsJSON)
        )
        -- Insert stock movements (PURCHASE)
        INSERT INTO stock_movement (warehouse_id, variant_id, supplier_id, movement_type, quantity, unit_cost, reference_type, reference_id, note)
        SELECT
            @WarehouseID,
            i.variant_id,
            @SupplierID,
            'PURCHASE',
            i.quantity,
            i.unit_cost,
            'SUPPLIER',
            @SupplierID,
            @Note
        FROM Items i;

        -- Update inventory (increase on-hand)
        UPDATE i
        SET
            i.quantity_on_hand = i.quantity_on_hand + Items.quantity,
            i.quantity_reserved = i.quantity_reserved -- unchanged
        FROM inventories i
        INNER JOIN Items ON i.variant_id = Items.variant_id AND i.warehouse_id = @WarehouseID;

        -- Insert inventory records if not exist (for variants without inventory record)
        -- Use MERGE to avoid duplicates
        ;WITH MissingInventories AS (
            SELECT i.variant_id
            FROM Items i
            LEFT JOIN inventories inv ON inv.variant_id = i.variant_id AND inv.warehouse_id = @WarehouseID
            WHERE inv.id IS NULL
        )
        INSERT INTO inventories (variant_id, warehouse_id, quantity_on_hand, quantity_reserved, last_stocked_at)
        SELECT
            mi.variant_id,
            @WarehouseID,
            0, -- quantity_on_hand = 0 initially, will be updated by next UPDATE
            0,
            GETDATE()
        FROM MissingInventories mi;

        -- Need to update again to set quantities for newly inserted rows
        UPDATE i
        SET
            i.quantity_on_hand = i.quantity_on_hand + Items.quantity
        FROM inventories i
        INNER JOIN Items ON i.variant_id = Items.variant_id AND i.warehouse_id = @WarehouseID;

        -- Handle serial numbers if provided
        -- This part requires JSON parsing of serial_numbers array per item
        -- Simplified: assume serial_numbers not in JSON for bulk import; use separate procedure
        -- Or extend JSON to include serial_numbers array and use OPENJSON with CROSS APPLY

        COMMIT TRANSACTION;

        SELECT @@ROWCOUNT as RowsAffected;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @ErrorMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMsg, 16, 1);
        RETURN;
    END CATCH
END
GO

-- -----------------------------------------------------
-- sp_GetRevenueReport
-- Báo cáo doanh thu theo ngày/tháng/năm
-- =====================================================
CREATE OR ALTER PROCEDURE sp_GetRevenueReport
    @StartDate DATE,
    @EndDate DATE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        CAST(o.created_at AS DATE) as SaleDate,
        COUNT(DISTINCT o.id) as OrderCount,
        COUNT(DISTINCT o.user_id) as CustomerCount,
        SUM(o.total_amount) as TotalRevenue,
        SUM(o.tax_amount) as TotalTax,
        SUM(o.shipping_fee) as TotalShipping
    FROM orders o
    WHERE o.status IN ('COMPLETED', 'SHIPPING')
        AND CAST(o.created_at AS DATE) BETWEEN @StartDate AND @EndDate
    GROUP BY CAST(o.created_at AS DATE)
    ORDER BY SaleDate DESC;
END
GO

-- -----------------------------------------------------
-- sp_GetLowStockReport
-- Báo cáo sản phẩm tồn kho thấp
-- =====================================================
CREATE OR ALTER PROCEDURE sp_GetLowStockReport
    @Threshold INT = 10 -- Số lượng tồn thấp hơn threshold
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.id as ProductID,
        p.name as ProductName,
        pv.id as VariantID,
        pv.sku,
        w.name as Warehouse,
        i.quantity_on_hand,
        pv.thumbnail_url
    FROM inventories i
    INNER JOIN product_variants pv ON i.variant_id = pv.id
    INNER JOIN products p ON pv.product_id = p.id
    INNER JOIN warehouses w ON i.warehouse_id = w.id
    WHERE i.quantity_on_hand <= @Threshold
        AND pv.is_active = 1
        AND p.status = 'ACTIVE'
    ORDER BY i.quantity_on_hand ASC;
END
GO

-- =====================================================
-- 9. VIEWS
-- =====================================================

-- -----------------------------------------------------
-- vw_ProductDetails
-- View tổng hợp sản phẩm với thông tin biến thể, tồn kho, images
-- =====================================================
CREATE OR ALTER VIEW vw_ProductDetails AS
SELECT
    p.id as ProductID,
    p.name as ProductName,
    p.slug as ProductSlug,
    p.status as ProductStatus,
    b.name as BrandName,
    c.name as CategoryName,
    c.slug as CategorySlug,
    pv.id as VariantID,
    pv.sku,
    pv.variant_name,
    pv.price,
    pv.original_price,
    pv.thumbnail_url,
    ISNULL(i.quantity_on_hand, 0) as QuantityOnHand,
    ISNULL(i.quantity_reserved, 0) as QuantityReserved,
    (SELECT COUNT(*) FROM product_images pi WHERE pi.variant_id = pv.id) as ImageCount,
    p.is_featured,
    p.created_at as ProductCreatedAt
FROM products p
LEFT JOIN categories c ON p.category_id = c.id
LEFT JOIN brands b ON p.brand_id = b.id
INNER JOIN product_variants pv ON p.id = pv.product_id AND pv.is_active = 1
LEFT JOIN inventories i ON pv.id = i.variant_id AND i.warehouse_id = 1 -- default warehouse
WHERE p.status = 'ACTIVE';
GO

-- -----------------------------------------------------
-- vw_OrderSummary
-- View tổng hợp đơn hàng với thông tin khách hàng, items
-- =====================================================
CREATE OR ALTER VIEW vw_OrderSummary AS
SELECT
    o.id as OrderID,
    o.code as OrderCode,
    o.user_id,
    u.full_name as CustomerName,
    u.email as CustomerEmail,
    u.phone as CustomerPhone,
    o.subtotal,
    o.tax_amount,
    o.shipping_fee,
    o.discount_amount,
    o.total_amount,
    o.status,
    o.payment_method,
    o.payment_status,
    o.shipping_address,
    o.tracking_number,
    o.created_at,
    COUNT(oi.id) as ItemCount,
    SUM(oi.quantity) as TotalQuantity
FROM orders o
INNER JOIN users u ON o.user_id = u.id
LEFT JOIN order_items oi ON o.id = oi.order_id
WHERE o.status NOT IN ('CANCELLED') -- optionally include cancelled
GROUP BY
    o.id, o.code, o.user_id, u.full_name, u.email, u.phone,
    o.subtotal, o.tax_amount, o.shipping_fee, o.discount_amount,
    o.total_amount, o.status, o.payment_method, o.payment_status,
    o.shipping_address, o.tracking_number, o.created_at;
GO

-- -----------------------------------------------------
-- vw_InventoryValuation
-- View đánh giá tồn kho theo giá vốn
-- =====================================================
CREATE OR ALTER VIEW vw_InventoryValuation AS
SELECT
    p.id as ProductID,
    p.name as ProductName,
    pv.id as VariantID,
    pv.sku,
    ISNULL(i.quantity_on_hand, 0) as QuantityOnHand,
    ISNULL(pv.cost_price, 0) as CostPrice,
    ISNULL(i.quantity_on_hand, 0) * ISNULL(pv.cost_price, 0) as Valuation
FROM products p
INNER JOIN product_variants pv ON p.id = pv.product_id AND pv.is_active = 1
LEFT JOIN inventories i ON pv.id = i.variant_id AND i.warehouse_id = 1
WHERE p.status = 'ACTIVE';
GO

-- =====================================================
-- 10. INDEXES ADDITIONAL
-- =====================================================
-- Index for orders (common queries)
CREATE INDEX idx_orders_user_created ON orders(user_id, created_at DESC);
CREATE INDEX idx_orders_status ON orders(status);
CREATE INDEX idx_orders_code ON orders(code);

-- Index for order_items
CREATE INDEX idx_order_items_order ON order_items(order_id);
CREATE INDEX idx_order_items_variant ON order_items(variant_id);

-- Index for inventory queries
CREATE INDEX idx_inventories_variant_warehouse ON inventories(variant_id, warehouse_id);

-- Index for stock movements
CREATE INDEX idx_stock_movement_variant ON stock_movement(variant_id);
CREATE INDEX idx_stock_movement_warehouse_date ON stock_movement(warehouse_id, movement_date DESC);

-- Index for vouchers
CREATE INDEX idx_vouchers_code ON vouchers(code);
CREATE INDEX idx_vouchers_active_dates ON vouchers(is_active, start_date, end_date);

-- Index for return requests
CREATE INDEX idx_returns_user ON returns(user_id);
CREATE INDEX idx_returns_status ON returns(status);

-- Index for reviews
CREATE INDEX idx_reviews_product ON reviews(product_id);
CREATE INDEX idx_reviews_user ON reviews(user_id);

PRINT 'Stored procedures and views created/updated successfully.';
