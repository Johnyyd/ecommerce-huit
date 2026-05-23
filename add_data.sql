USE HuitShopDB;
GO

-- Add new products
INSERT INTO products (name, slug, brand_id, category_id, short_description, description, specifications, status, is_featured, created_at, updated_at, created_by)
VALUES 
('MacBook Pro M3 Max', 'macbook-pro-m3-max', 1, 2, 'MacBook Pro 16 inch v?i chip M3 Max', '<p>MacBook Pro M3 Max 16 inch v?i chip M3 Max</p>', '{"screen":"16 inch","chip":"M3 Max","ram":"36GB"}', 'ACTIVE', 0, GETDATE(), GETDATE(), 1),
('iPad Pro M4', 'ipad-pro-m4', 1, 4, 'iPad Pro M4 m?n h?nh OLED', '<p>iPad Pro M4 m?n h?nh OLED</p>', '{"screen":"11 inch","chip":"M4","ram":"8GB"}', 'ACTIVE', 0, GETDATE(), GETDATE(), 1),
('Apple Watch Ultra 2', 'apple-watch-ultra-2', 1, 5, 'Apple Watch Ultra 2 titan', '<p>Apple Watch Ultra 2 titan</p>', '{"screen":"1.92 inch","chip":"S9","ram":"1GB"}', 'ACTIVE', 0, GETDATE(), GETDATE(), 1);

DECLARE @ProductId1 INT = IDENT_CURRENT('products') - 2;
DECLARE @ProductId2 INT = IDENT_CURRENT('products') - 1;
DECLARE @ProductId3 INT = IDENT_CURRENT('products');

-- Add variants
INSERT INTO product_variants (product_id, sku, variant_name, price, original_price, cost_price, display_order, is_active, created_at, updated_at)
VALUES 
(@ProductId1, 'MBP-M3M-16-36-1TB', '16-inch, M3 Max, 36GB, 1TB', 89990000, 95990000, 80000000, 1, 1, GETDATE(), GETDATE()),
(@ProductId2, 'IPAD-M4-11-256', '11-inch, M4, 256GB WiFi', 28990000, 30990000, 25000000, 1, 1, GETDATE(), GETDATE()),
(@ProductId3, 'AW-U2-49-O', '49mm Titanium, Ocean Band', 21990000, 22990000, 19000000, 1, 1, GETDATE(), GETDATE());

DECLARE @VariantId1 INT = IDENT_CURRENT('product_variants') - 2;
DECLARE @VariantId2 INT = IDENT_CURRENT('product_variants') - 1;
DECLARE @VariantId3 INT = IDENT_CURRENT('product_variants');

-- Create cart for customerA (user_id = 4)
DECLARE @CartId INT;

SELECT @CartId = id FROM carts WHERE user_id = 4;

IF @CartId IS NULL
BEGIN
    INSERT INTO carts (user_id, created_at, updated_at) VALUES (4, GETDATE(), GETDATE());
    SET @CartId = SCOPE_IDENTITY();
END

-- Add items to cart
IF NOT EXISTS (SELECT 1 FROM cart_items WHERE cart_id = @CartId AND variant_id = @VariantId1)
    INSERT INTO cart_items (cart_id, variant_id, quantity, added_at, updated_at) VALUES (@CartId, @VariantId1, 1, GETDATE(), GETDATE());
ELSE
    UPDATE cart_items SET quantity = quantity + 1 WHERE cart_id = @CartId AND variant_id = @VariantId1;

IF NOT EXISTS (SELECT 1 FROM cart_items WHERE cart_id = @CartId AND variant_id = @VariantId2)
    INSERT INTO cart_items (cart_id, variant_id, quantity, added_at, updated_at) VALUES (@CartId, @VariantId2, 2, GETDATE(), GETDATE());
ELSE
    UPDATE cart_items SET quantity = quantity + 2 WHERE cart_id = @CartId AND variant_id = @VariantId2;

IF NOT EXISTS (SELECT 1 FROM cart_items WHERE cart_id = @CartId AND variant_id = @VariantId3)
    INSERT INTO cart_items (cart_id, variant_id, quantity, added_at, updated_at) VALUES (@CartId, @VariantId3, 1, GETDATE(), GETDATE());
ELSE
    UPDATE cart_items SET quantity = quantity + 1 WHERE cart_id = @CartId AND variant_id = @VariantId3;

-- Add an existing variant to the cart as well (e.g. variant 1 - iPhone 15 Pro Max)
IF NOT EXISTS (SELECT 1 FROM cart_items WHERE cart_id = @CartId AND variant_id = 1)
    INSERT INTO cart_items (cart_id, variant_id, quantity, added_at, updated_at) VALUES (@CartId, 1, 1, GETDATE(), GETDATE());
ELSE
    UPDATE cart_items SET quantity = quantity + 1 WHERE cart_id = @CartId AND variant_id = 1;

PRINT 'Data added successfully.';
GO
