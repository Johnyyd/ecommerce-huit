DECLARE @variant_id INT;
DECLARE @sku VARCHAR(100);

DECLARE variant_cursor CURSOR FOR 
SELECT id, sku FROM product_variants;

OPEN variant_cursor;
FETCH NEXT FROM variant_cursor INTO @variant_id, @sku;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Insert 3 AVAILABLE serials for each variant
    DECLARE @i INT = 1;
    WHILE @i <= 3
    BEGIN
        DECLARE @sn VARCHAR(100) = 'SN-' + ISNULL(@sku, 'VAR' + CAST(@variant_id AS VARCHAR)) + '-A' + CAST(@i AS VARCHAR);
        IF NOT EXISTS (SELECT 1 FROM product_serials WHERE serial_number = @sn)
        BEGIN
            INSERT INTO product_serials (variant_id, serial_number, warehouse_id, status, inbound_date, created_at, updated_at)
            VALUES (@variant_id, @sn, 1, 'AVAILABLE', GETDATE(), GETDATE(), GETDATE());
        END
        SET @i = @i + 1;
    END

    FETCH NEXT FROM variant_cursor INTO @variant_id, @sku;
END

CLOSE variant_cursor;
DEALLOCATE variant_cursor;

DECLARE @order_item_id INT;
DECLARE @o_variant_id INT;
DECLARE @o_quantity INT;
DECLARE @o_sku VARCHAR(100);

DECLARE oi_cursor CURSOR FOR 
SELECT oi.id, oi.variant_id, oi.quantity, pv.sku 
FROM order_items oi
JOIN orders o ON oi.order_id = o.id
JOIN product_variants pv ON oi.variant_id = pv.id
WHERE o.status = 'COMPLETED';

OPEN oi_cursor;
FETCH NEXT FROM oi_cursor INTO @order_item_id, @o_variant_id, @o_quantity, @o_sku;

WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @q INT = 1;
    WHILE @q <= @o_quantity
    BEGIN
        DECLARE @sn2 VARCHAR(100) = 'SN-' + ISNULL(@o_sku, 'VAR' + CAST(@o_variant_id AS VARCHAR)) + '-S' + CAST(@order_item_id AS VARCHAR) + CAST(@q AS VARCHAR);
        
        IF NOT EXISTS (SELECT 1 FROM product_serials WHERE serial_number = @sn2)
        BEGIN
            INSERT INTO product_serials (variant_id, serial_number, warehouse_id, status, inbound_date, outbound_date, warranty_expire_date, notes, created_at, updated_at)
            VALUES (@o_variant_id, @sn2, 1, 'SOLD', DATEADD(month, -1, GETDATE()), GETDATE(), DATEADD(year, 1, GETDATE()), N'Bán theo đơn hàng', GETDATE(), GETDATE());
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
