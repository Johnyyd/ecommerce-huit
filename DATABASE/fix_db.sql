-- 1. Remove duplicate serial numbers that exceed the quantity
WITH CTE AS (
    SELECT ois.serial_number, 
           ROW_NUMBER() OVER(PARTITION BY ois.order_item_id ORDER BY ois.serial_number DESC) as rn,
           oi.quantity
    FROM order_item_serials ois
    JOIN order_items oi ON ois.order_item_id = oi.id
)
DELETE FROM order_item_serials WHERE serial_number IN (
    SELECT serial_number FROM CTE WHERE rn > quantity
);

-- 2. Update generic SKUs to more realistic SKUs
UPDATE pv
SET pv.sku = 
    CASE pv.id
        WHEN 9 THEN 'ASUS-ROG-G14'
        WHEN 10 THEN 'DELL-XPS-15'
        WHEN 11 THEN 'MAC-PRO16-M3'
        WHEN 12 THEN 'LENOVO-LEG5'
        WHEN 13 THEN 'HP-SPCTX360'
        WHEN 14 THEN 'AW-SERIES-9'
        WHEN 15 THEN 'SS-GW6-CLASSIC'
        WHEN 16 THEN 'GARMIN-FNX7'
        WHEN 17 THEN 'XIAOMI-MB8'
        WHEN 18 THEN 'HUAWEI-GT4'
        WHEN 19 THEN 'INTEL-I9-14900K'
        WHEN 20 THEN 'AMD-R9-7950X3D'
        WHEN 21 THEN 'NVIDIA-RTX4090'
        WHEN 22 THEN 'AMD-RX7900XTX'
        WHEN 23 THEN 'ASUS-ROG-Z790'
        WHEN 24 THEN 'MSI-MAG-B650'
        WHEN 25 THEN 'CORSAIR-V-32G'
        WHEN 26 THEN 'GSKILL-TZ5-64G'
        WHEN 27 THEN 'SS-990PRO-2T'
        WHEN 28 THEN 'WD-SN850X-1T'
        ELSE pv.sku
    END
FROM product_variants pv
WHERE pv.sku LIKE 'SKU-%';

-- 3. Ensure ALL order items have serial numbers up to their quantity
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
    DECLARE @current_count INT;
    SELECT @current_count = COUNT(*) FROM order_item_serials WHERE order_item_id = @order_item_id;

    DECLARE @q INT = @current_count + 1;
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
