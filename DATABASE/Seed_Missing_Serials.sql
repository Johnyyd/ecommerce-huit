-- Kịch bản: Bổ sung Serial Number cho các sản phẩm trong đơn hàng bị thiếu
-- Chạy script này mỗi khi có đơn hàng mới được tạo để tự động cấp mã Serial (chỉ dùng cho môi trường Test/Demo)

DECLARE @order_item_id INT;
DECLARE @o_variant_id INT;
DECLARE @o_quantity INT;
DECLARE @o_sku VARCHAR(100);

DECLARE oi_cursor CURSOR FOR 
SELECT oi.id, oi.variant_id, oi.quantity, pv.sku 
FROM order_items oi
JOIN product_variants pv ON oi.variant_id = pv.id;

OPEN oi_cursor;
FETCH NEXT FROM oi_cursor INTO @order_item_id, @o_variant_id, @o_quantity, @o_sku;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Đếm số lượng Serial Number hiện có của order_item này
    DECLARE @current_count INT;
    SELECT @current_count = COUNT(*) FROM order_item_serials WHERE order_item_id = @order_item_id;

    -- Nếu số lượng Serial Number ít hơn số lượng đặt mua, tiến hành sinh thêm
    DECLARE @q INT = @current_count + 1;
    WHILE @q <= @o_quantity
    BEGIN
        DECLARE @sn2 VARCHAR(100) = 'SN-' + ISNULL(@o_sku, 'VAR' + CAST(@o_variant_id AS VARCHAR)) + '-S' + CAST(@order_item_id AS VARCHAR) + CAST(@q AS VARCHAR);
        
        IF NOT EXISTS (SELECT 1 FROM product_serials WHERE serial_number = @sn2)
        BEGIN
            INSERT INTO product_serials (variant_id, serial_number, warehouse_id, status, inbound_date, outbound_date, warranty_expire_date, notes, created_at, updated_at)
            VALUES (@o_variant_id, @sn2, 1, 'SOLD', DATEADD(month, -1, GETDATE()), GETDATE(), DATEADD(year, 1, GETDATE()), N'Bán theo đơn hàng (Auto-generated)', GETDATE(), GETDATE());
        END
        
        IF NOT EXISTS (SELECT 1 FROM order_item_serials WHERE order_item_id = @order_item_id AND serial_number = @sn2)
        BEGIN
            INSERT INTO order_item_serials (order_item_id, serial_number)
            VALUES (@order_item_id, @sn2);
        END

        SET @q = @q + 1;
    END

    FETCH NEXT FROM oi_cursor INTO @order_item_id, @o_variant_id, @o_quantity, @o_sku;
END

CLOSE oi_cursor;
DEALLOCATE oi_cursor;
