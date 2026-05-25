-- =====================================================
-- STORED PROCEDURES AND FUNCTIONS
-- =====================================================

USE HuitShopDB;
GO

-- =====================================================
-- 11. STORED PROCEDURES
-- =====================================================

CREATE OR ALTER PROCEDURE sp_ImportStock
    @WarehouseID INT,
    @VariantID INT,
    @CostPrice DECIMAL(15,2),
    @SupplierID INT = NULL,
    @ListIMEI NVARCHAR(MAX), 
    @CreatedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @Quantity INT;
        SELECT @Quantity = COUNT(*) FROM OPENJSON(@ListIMEI);

        INSERT INTO product_serials (variant_id, warehouse_id, serial_number, status, inbound_date, notes)
        SELECT @VariantID, @WarehouseID, value, 'AVAILABLE', GETDATE(), N'Nhập kho lô mới'
        FROM OPENJSON(@ListIMEI);

        MERGE inventories AS target
        USING (SELECT @WarehouseID AS warehouse_id, @VariantID AS variant_id) AS source
        ON (target.warehouse_id = source.warehouse_id AND target.variant_id = source.variant_id)
        WHEN MATCHED THEN
            UPDATE SET quantity_on_hand = quantity_on_hand + @Quantity,
                       last_updated = GETDATE()
        WHEN NOT MATCHED THEN
            INSERT (warehouse_id, variant_id, quantity_on_hand, quantity_reserved)
            VALUES (@WarehouseID, @VariantID, @Quantity, 0);

        INSERT INTO stock_movements (warehouse_id, variant_id, quantity, movement_type, supplier_id, note, created_by)
        VALUES (@WarehouseID, @VariantID, @Quantity, 'PURCHASE', @SupplierID, N'Nhập hàng từ supplier', @CreatedBy);

        UPDATE product_variants
        SET cost_price = @CostPrice
        WHERE id = @VariantID;

        COMMIT TRANSACTION;
        PRINT N'Nhập kho thành công!';
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE sp_CreateOrder
    @UserID INT,
    @ShippingAddress NVARCHAR(MAX),
    @PaymentMethod VARCHAR(50),
    @OrderItemsJSON NVARCHAR(MAX),
    @OrderID INT OUTPUT,
    @OrderCode VARCHAR(20) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        IF @PaymentMethod NOT IN ('CASH','MOMO','VNPAY','BANKING','COD')
        BEGIN
            RAISERROR('Invalid payment method', 16, 1);
            RETURN;
        END

        DECLARE @Subtotal DECIMAL(15,2);
        SELECT @Subtotal = SUM(quantity * price)
        FROM OPENJSON(@OrderItemsJSON)
        WITH (
            variant_id INT,
            quantity INT,
            price DECIMAL(15,2)
        );

        IF @Subtotal IS NULL SET @Subtotal = 0;

        SET @OrderCode = 'ORD-' + FORMAT(GETDATE(), 'yyyyMMddHHmmss');

        INSERT INTO orders (code, user_id, subtotal, total, payment_method, shipping_address, status)
        VALUES (@OrderCode, @UserID, @Subtotal, @Subtotal, @PaymentMethod, @ShippingAddress, 'PENDING');

        SET @OrderID = SCOPE_IDENTITY();

        DECLARE @Items TABLE (
            variant_id INT,
            quantity INT,
            price DECIMAL(15,2)
        );

        INSERT INTO @Items
        SELECT variant_id, quantity, price
        FROM OPENJSON(@OrderItemsJSON)
        WITH (
            variant_id INT,
            quantity INT,
            price DECIMAL(15,2)
        );

        DECLARE @InsufficientStock BIT = 0;
        DECLARE cur CURSOR LOCAL FOR
        SELECT i.variant_id, i.quantity, v.product_id, v.sku, p.name, v.variant_name
        FROM @Items i
        JOIN product_variants v ON v.id = i.variant_id
        JOIN products p ON v.product_id = p.id;

        DECLARE @VariantID INT, @Qty INT, @ProductID INT, @SKU VARCHAR(50), @ProductName NVARCHAR(255), @VariantName NVARCHAR(255);
        OPEN cur;
        FETCH NEXT FROM cur INTO @VariantID, @Qty, @ProductID, @SKU, @ProductName, @VariantName;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            DECLARE @Available INT;
            SELECT @Available = quantity_on_hand - quantity_reserved
            FROM inventories
            WHERE warehouse_id = 1
              AND variant_id = @VariantID;

            IF @Available < @Qty
            BEGIN
                SET @InsufficientStock = 1;
                BREAK;
            END

            FETCH NEXT FROM cur INTO @VariantID, @Qty, @ProductID, @SKU, @ProductName, @VariantName;
        END
        CLOSE cur;
        DEALLOCATE cur;

        IF @InsufficientStock = 1
        BEGIN
            RAISERROR('Insufficient stock for one or more items', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        INSERT INTO order_items (order_id, variant_id, product_name, sku, quantity, unit_price, total_price)
        SELECT @OrderID, i.variant_id, p.name + CASE WHEN v.variant_name IS NULL THEN '' ELSE ' ' + v.variant_name END, v.sku, i.quantity, i.price, (i.quantity * i.price)
        FROM @Items i
        JOIN product_variants v ON v.id = i.variant_id
        JOIN products p ON v.product_id = p.id;

        UPDATE inv
        SET quantity_reserved = quantity_reserved + i.quantity,
            last_updated = GETDATE()
        FROM inventories inv
        JOIN @Items i ON inv.variant_id = i.variant_id
        WHERE inv.warehouse_id = 1;

        INSERT INTO stock_movements (warehouse_id, variant_id, quantity, movement_type, reference_id, note)
        SELECT 1, i.variant_id, -i.quantity, 'SALE_RESERVED', @OrderID, N'Reserve for order ' + @OrderCode
        FROM @Items i;

        INSERT INTO order_status_history (order_id, status)
        VALUES (@OrderID, 'PENDING');

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE sp_ConfirmOrder
    @OrderID INT,
    @StaffID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE orders
    SET status = 'CONFIRMED'
    WHERE id = @OrderID AND status = 'PENDING';

    IF @@ROWCOUNT = 0
    BEGIN
        RAISERROR('Order not found or cannot be confirmed', 16, 1);
        RETURN;
    END

    INSERT INTO order_status_history (order_id, status, changed_by, note)
    VALUES (@OrderID, 'CONFIRMED', @StaffID, N'Đơn hàng đã được xác nhận');
END;
GO

CREATE OR ALTER PROCEDURE sp_ShipOrder
    @OrderID INT,
    @WarehouseID INT,
    @StaffID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @CurrentStatus VARCHAR(20);
        SELECT @CurrentStatus = status FROM orders WHERE id = @OrderID;

        IF @CurrentStatus NOT IN ('CONFIRMED','PROCESSING')
        BEGIN
            RAISERROR('Order cannot be shipped from current status', 16, 1);
            RETURN;
        END

        DECLARE @UnavailableSerialCount INT;
        SELECT @UnavailableSerialCount = COUNT(*)
        FROM order_item_serials ois
        JOIN product_serials ps ON ps.serial_number = ois.serial_number
        WHERE ois.order_item_id IN (SELECT id FROM order_items WHERE order_id = @OrderID)
          AND ps.status NOT IN ('AVAILABLE', 'RESERVED');

        IF @UnavailableSerialCount > 0
        BEGIN
            RAISERROR('One or more serials are not available for shipping', 16, 1);
            RETURN;
        END

        UPDATE ps
        SET status = 'SOLD',
            outbound_date = GETDATE(),
            warranty_expire_date = DATEADD(MONTH, 12, GETDATE())
        FROM product_serials ps
        JOIN order_item_serials ois ON ps.serial_number = ois.serial_number
        WHERE ois.order_item_id IN (SELECT id FROM order_items WHERE order_id = @OrderID);

        UPDATE inv
        SET quantity_reserved = quantity_reserved - oi.quantity,
            last_updated = GETDATE()
        FROM inventories inv
        JOIN order_items oi ON inv.variant_id = oi.variant_id
        WHERE oi.order_id = @OrderID AND inv.warehouse_id = @WarehouseID;

        INSERT INTO stock_movements (warehouse_id, variant_id, quantity, movement_type, reference_id, note)
        SELECT @WarehouseID, oi.variant_id, -oi.quantity, 'SALE_SHIP', @OrderID, N'Xuất kho bán hàng'
        FROM order_items oi
        WHERE oi.order_id = @OrderID;

        UPDATE orders
        SET status = 'SHIPPING'
        WHERE id = @OrderID;

        INSERT INTO order_status_history (order_id, status, changed_by, note)
        VALUES (@OrderID, 'SHIPPING', @StaffID, N'Đã xuất kho, vận chuyển');

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE sp_CompleteOrder
    @OrderID INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE orders
    SET status = 'COMPLETED'
    WHERE id = @OrderID AND status = 'SHIPPING';

    IF @@ROWCOUNT = 0
    BEGIN
        RAISERROR('Order not found or not in SHIPPING status', 16, 1);
        RETURN;
    END

    INSERT INTO order_status_history (order_id, status)
    VALUES (@OrderID, 'COMPLETED');
END;
GO

CREATE OR ALTER PROCEDURE sp_CancelOrder
    @OrderID INT,
    @Reason NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @Status VARCHAR(20);
        SELECT @Status = status FROM orders WHERE id = @OrderID;

        IF @Status NOT IN ('PENDING','CONFIRMED','PROCESSING')
        BEGIN
            RAISERROR('Order cannot be cancelled from current status', 16, 1);
            RETURN;
        END

        UPDATE inv
        SET quantity_reserved = quantity_reserved - oi.quantity,
            last_updated = GETDATE()
        FROM inventories inv
        JOIN order_items oi ON inv.variant_id = oi.variant_id
        WHERE oi.order_id = @OrderID;

        INSERT INTO stock_movements (warehouse_id, variant_id, quantity, movement_type, reference_id, note)
        SELECT 1, oi.variant_id, oi.quantity, 'ADJUSTMENT_IN', @OrderID, N'Hủy đơn, hoàn trả tồn kho'
        FROM order_items oi
        WHERE oi.order_id = @OrderID;

        UPDATE orders
        SET status = 'CANCELLED',
            note = ISNULL(@Reason, note)
        WHERE id = @OrderID;

        INSERT INTO order_status_history (order_id, status, note)
        VALUES (@OrderID, 'CANCELLED', @Reason);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE sp_ProcessReturn
    @ReturnID INT,
    @Action VARCHAR(20) 
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @OrderID INT, @OrderItemID INT, @VariantID INT, @WarehouseID INT, @SerialNumber VARCHAR(100);
        SELECT @OrderID = order_id, @OrderItemID = order_item_id
        FROM returns
        WHERE id = @ReturnID AND status = 'REQUESTED';

        IF @OrderID IS NULL
        BEGIN
            RAISERROR('Return request not found or not in REQUESTED status', 16, 1);
            RETURN;
        END

        SELECT TOP 1 @SerialNumber = serial_number
        FROM order_item_serials
        WHERE order_item_id = @OrderItemID;

        SELECT @VariantID = oi.variant_id
        FROM order_items oi
        WHERE oi.id = @OrderItemID;

        SET @WarehouseID = 1;

        IF @Action = 'APPROVE'
        BEGIN
            UPDATE product_serials
            SET status = 'AVAILABLE',
                warranty_expire_date = NULL,
                notes = ISNULL(notes, '') + ' | Returned and approved'
            WHERE serial_number = @SerialNumber;

            UPDATE inventories
            SET quantity_on_hand = quantity_on_hand + 1,
                last_updated = GETDATE()
            WHERE warehouse_id = @WarehouseID AND variant_id = @VariantID;

            INSERT INTO stock_movements (warehouse_id, variant_id, quantity, movement_type, reference_id, note)
            VALUES (@WarehouseID, @VariantID, 1, 'RETURN', @ReturnID, N'Khách hàng trả hàng, đã duyệt');

            UPDATE returns
            SET status = 'REFUNDED',
                resolved_at = GETDATE()
            WHERE id = @ReturnID;
        END
        ELSE IF @Action = 'REJECT'
        BEGIN
            UPDATE returns
            SET status = 'REJECTED',
                resolved_at = GETDATE()
            WHERE id = @ReturnID;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

CREATE OR ALTER FUNCTION ufn_CalculateDiscount
(
    @Subtotal DECIMAL(15,2),
    @VoucherID INT
)
RETURNS DECIMAL(15,2)
AS
BEGIN
    DECLARE @Discount DECIMAL(15,2) = 0;
    DECLARE @DiscountType VARCHAR(10);
    DECLARE @DiscountValue DECIMAL(15,2);
    DECLARE @MaxDiscount DECIMAL(15,2);
    DECLARE @MinOrder DECIMAL(15,2);

    SELECT @DiscountType = discount_type,
           @DiscountValue = discount_value,
           @MaxDiscount = max_discount_amount,
           @MinOrder = min_order_value
    FROM vouchers
    WHERE id = @VoucherID
      AND is_active = 1
      AND GETDATE() BETWEEN start_date AND end_date;

    IF @DiscountType IS NOT NULL AND @Subtotal >= @MinOrder
    BEGIN
        IF @DiscountType = 'PERCENT'
            SET @Discount = @Subtotal * (@DiscountValue / 100.0);
        ELSE IF @DiscountType = 'FIXED'
            SET @Discount = @DiscountValue;

        IF @MaxDiscount IS NOT NULL AND @Discount > @MaxDiscount
            SET @Discount = @MaxDiscount;
    END

    RETURN @Discount;
END;
GO

CREATE OR ALTER PROCEDURE sp_GetLowStockReport
    @WarehouseID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        w.id as warehouse_id,
        w.name as warehouse_name,
        w.code as warehouse_code,
        p.id as product_id,
        p.name as product_name,
        v.id as variant_id,
        v.sku,
        v.variant_name,
        i.quantity_on_hand,
        i.quantity_reserved,
        (i.quantity_on_hand - i.quantity_reserved) as available_quantity,
        i.reorder_point
    FROM inventories i
    JOIN warehouses w ON i.warehouse_id = w.id
    JOIN product_variants v ON i.variant_id = v.id
    JOIN products p ON v.product_id = p.id
    WHERE (@WarehouseID IS NULL OR i.warehouse_id = @WarehouseID)
      AND (i.quantity_on_hand - i.quantity_reserved) <= i.reorder_point
      AND w.is_active = 1
    ORDER BY w.id, p.id;
END;
GO

CREATE OR ALTER PROCEDURE sp_GetRevenueReport
    @FromDate DATE,
    @ToDate DATE,
    @GroupBy VARCHAR(20) = 'DAY' 
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Format VARCHAR(10);
    IF @GroupBy = 'DAY' SET @Format = 'yyyy-MM-dd';
    ELSE IF @GroupBy = 'MONTH' SET @Format = 'yyyy-MM';
    ELSE IF @GroupBy = 'YEAR' SET @Format = 'yyyy';

    SELECT
        FORMAT(created_at, @Format) as period,
        COUNT(id) as orders_count,
        SUM(total) as revenue,
        SUM(discount) as discount_total,
        SUM(shipping_fee) as shipping_fee_total,
        AVG(total) as average_order_value
    FROM orders
    WHERE created_at >= @FromDate
      AND created_at < DATEADD(DAY, 1, @ToDate)
      AND status = 'COMPLETED'
    GROUP BY FORMAT(created_at, @Format)
    ORDER BY period;
END;
GO
GO
