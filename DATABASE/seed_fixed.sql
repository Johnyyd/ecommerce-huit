-- =====================================================
-- SEED DATA FOR ECOMMERCE HUIT
-- Database: HuitShopDB
-- Note: Chạy file này SAU khi đã chạy init.sql
-- =====================================================

USE HuitShopDB;
GO

-- =====================================================
-- 1. INSERT USERS (Fixed IDs)
-- =====================================================
SET IDENTITY_INSERT users ON;

INSERT INTO users (id, full_name, email, phone, password_hash, role, status, created_at) VALUES
(1, N'Nguyễn Minh Trí', 'admin@huit.edu.vn', '0909000001', 'hash_admin_123', 'ADMIN', 'ACTIVE', '2025-01-01'),
(2, N'Trần Văn Nhân Viên', 'staff@huit.edu.vn', '0909000002', 'hash_staff_123', 'STAFF', 'ACTIVE', '2025-01-01'),
(3, N'Lê Thủ Kho', 'warehouse@huit.edu.vn', '0909000003', 'hash_warehouse_123', 'WAREHOUSE', 'ACTIVE', '2025-01-01'),
(4, N'Phạm Khách Hàng A', 'customerA@gmail.com', '0909000004', 'hash_customer_123', 'CUSTOMER', 'ACTIVE', '2025-01-01'),
(5, N'Hoàng Khách Hàng B', 'customerB@gmail.com', '0909000005', 'hash_customer_123', 'CUSTOMER', 'BANNED', '2025-01-01');

SET IDENTITY_INSERT users OFF;
GO

-- =====================================================
-- 2. INSERT ADDRESSES (NEW)
-- =====================================================
SET IDENTITY_INSERT addresses ON;

INSERT INTO addresses (id, user_id, label, receiver_name, receiver_phone, province, district, ward, street_address, is_default) VALUES
(1, 4, N'Nhà riêng', N'Phạm Khách Hàng A', '0909000004', N'TP. Hồ Chí Minh', N'Quận Tân Phú', N'Phường Tân Thới Hòa', N'140 Lê Trọng Tấn', 1),
(2, 4, N'Cơ quan', N'Phạm Khách Hàng A', '0909001111', N'TP. Hồ Chí Minh', N'Quận 1', N'Phường Bến Nghé', N'2 Lê Duẩn', 0),
(3, 1, N'Nơi làm việc', N'Nguyễn Minh Trí', '0909000001', N'TP. Hồ Chí Minh', N'Quận 12', N'Phường Tân Thới Nhất', N'15 Âu Cơ', 1);

SET IDENTITY_INSERT addresses OFF;
GO

-- =====================================================
-- 3. INSERT CATEGORIES & BRANDS & WAREHOUSES
-- =====================================================
SET IDENTITY_INSERT categories ON;

INSERT INTO categories (id, name, slug, description, is_active, sort_order) VALUES
(1, N'Laptop Gaming', 'laptop-gaming', N'Máy tính xách tay cấu hình cao cho chơi game', 1, 1),
(2, N'Laptop Văn Phòng', 'laptop-office', N'Mỏng nhẹ, pin trâu, phù hợp làm việc', 1, 2),
(3, N'Điện Thoại', 'smart-phone', N'Smartphone đời mới nhất', 1, 3),
(4, N'Máy Tính Bảng', 'tablet', N'iPad, Samsung Tab, Android Tablet', 1, 4),
(5, N'Phụ Kiện', 'accessories', N'Tai nghe, Chuột, Bàn phím, Ốp lưng', 1, 5);

SET IDENTITY_INSERT categories OFF;

SET IDENTITY_INSERT brands ON;

INSERT INTO brands (id, name, origin, description) VALUES
(1, 'Apple', 'USA', N'Thiết bị chất lượng cao'),
(2, 'Samsung', 'Korea', N'Thương hiệu hàng đầu Hàn Quốc'),
(3, 'Dell', 'USA', N'Máy tính để bàn và laptop'),
(4, 'Asus', 'Taiwan', N'Laptop Gaming và Mainboard'),
(5, 'Sony', 'Japan', N'Âm thanh và điện tử'),
(6, 'Xiaomi', 'China', N'Điện thoại và thiết bị thông minh');

SET IDENTITY_INSERT brands OFF;

SET IDENTITY_INSERT warehouses ON;

INSERT INTO warehouses (id, code, name, address, type, manager) VALUES
(1, N'KHO-TONG', N'Kho Tổng HUIT', N'140 Lê Trọng Tấn, Tân Phú, TP.HCM', 'PHYSICAL', N'Lê Thủ Kho'),
(2, N'SHOWROOM-Q1', N'Showroom Quận 1', N'Nguyễn Huệ, Quận 1, TP.HCM', 'PHYSICAL', N'Trần Nhân Viên'),
(3, N'SHOWROOM-TD', N'Showroom Thủ Đức', N'Võ Văn Ngân, TP. Thủ Đức', 'PHYSICAL', N'Phạm Nhân Viên'),
(4, N'KHO-BAO-HANH', N'Kho Bảo Hành', N'Lý Thường Kiệt, Q10, TP.HCM', 'PHYSICAL', N'Lê Thủ Kho'),
(5, N'KHO-DANG-VE', N'Kho Hàng Đang Về', N'Cảng Cát Lái, TP.HCM', 'VIRTUAL', N'System');

SET IDENTITY_INSERT warehouses OFF;

SET IDENTITY_INSERT suppliers ON;

INSERT INTO suppliers (id, code, name, contact_person, phone, email, address, tax_code) VALUES
(1, 'SUP-APPLE', N'Apple Vietnam', N'Nguyễn Văn A', '0911111111', 'apple@vn.com', N'Quận 7, TP.HCM', '0301234567'),
(2, 'SUP-SAMSUNG', N'Samsung Vietnam', N'Trần Văn B', '0922222222', 'samsung@vn.com', N'Quận 12, TP.HCM', '0307654321'),
(3, 'SUP-DELL', N'Dell Vietnam', N'Lê Văn C', '0933333333', 'dell@vn.com', N'Quận Tân Bình', '0301112223');

SET IDENTITY_INSERT suppliers OFF;
GO

-- =====================================================
-- 4. INSERT PRODUCTS & VARIANTS
-- =====================================================
SET IDENTITY_INSERT products ON;

INSERT INTO products (id, name, slug, brand_id, category_id, description, specifications, status, created_by) VALUES
(1, N'iPhone 15 Pro Max', 'iphone-15-pro-max', 1, 3,
 N'<p>iPhone 15 Pro Max với chip A17 Pro, màn hình Super Retina XDR, hệ thống camera 48MP</p>',
 N'{"screen":"6.7 inch","chip":"A17 Pro","ram":"8GB","battery":"4422 mAh","camera":"48MP"}',
 'ACTIVE', 1),
(2, N'Samsung Galaxy S24 Ultra', 'samsung-s24-ultra', 2, 3,
 N'<p>S24 Ultra với S-Pen, camera 200MP, AI tích hợp</p>',
 N'{"screen":"6.8 inch","chip":"Snapdragon 8 Gen 3","ram":"12GB","battery":"5000 mAh","camera":"200MP"}',
 'ACTIVE', 1),
(3, N'MacBook Air M3', 'macbook-air-m3', 1, 2,
 N'<p>Laptop mỏng nhẹ, sang trọng, chip Apple M3 mạnh mẽ</p>',
 N'{"screen":"13.6 inch Liquid Retina","cpu":"Apple M3","ram":"8GB","storage":"256GB SSD","weight":"1.24 kg"}',
 'ACTIVE', 1),
(4, N'Asus ROG Strix G16', 'asus-rog-strix-g16', 4, 1,
 N'<p>Gaming laptop mạnh với card RTX 4060, màn hình 165Hz</p>',
 N'{"screen":"16 inch 165Hz","cpu":"Intel Core i9 13980HX","gpu":"RTX 4060","ram":"16GB","storage":"1TB SSD"}',
 'ACTIVE', 1),
(5, N'Sony WH-1000XM5', 'sony-wh-1000xm5', 5, 5,
 N'<p>Tai nghe chống ồn chủ động ANC, chất âm cao cấp</p>',
 N'{"type":"Over-ear","anc":"Yes","battery":"30 hours","weight":"250g"}',
 'ACTIVE', 1);

SET IDENTITY_INSERT products OFF;
GO

SET IDENTITY_INSERT product_variants ON;

INSERT INTO product_variants (id, product_id, sku, variant_name, price, original_price, cost_price, thumbnail_url, display_order) VALUES
(1, 1, 'IP15PM-256-TI', N'256GB - Titan Tự Nhiên', 28990000, 34990000, 26000000, '/Content/Anh/iPhone_15_Pro_Max.png', 1),
(2, 1, 'IP15PM-512-BL', N'512GB - Titan Xanh', 34990000, 40990000, 31000000, '/Content/Anh/iPhone_15_Pro_Max.png', 2),
(3, 2, 'SS-S24U-256-GR', N'256GB - Xám Titan', 26990000, 31990000, 24000000, '/Content/Anh/Samsung_Galaxy_S24_Ultra.jpg', 1),
(4, 2, 'SS-S24U-512-BK', N'512GB - Đen', 31990000, 36990000, 28000000, '/Content/Anh/Samsung_Galaxy_S24_Ultra.jpg', 2),
(5, 3, 'MAC-AIR-M3-8-256', N'M3/8GB/256GB', 27990000, 29990000, 25000000, '/Content/Anh/MacBook Air M3.jpg', 1),
(6, 4, 'ASUS-ROG-G16', N'i9/RTX4060/16GB/1TB', 32000000, 35000000, 29000000, NULL, 1),
(7, 5, 'SONY-XM5-BLK', N'Màu Đen', 6990000, 8490000, 5500000, NULL, 1),
(8, 5, 'SONY-XM5-SLV', N'Màu Bạc', 7190000, 8690000, 5600000, NULL, 2);

SET IDENTITY_INSERT product_variants OFF;
GO

-- =====================================================
-- 5. INSERT INVENTORIES & WISHLISTS (NEW)
-- =====================================================

INSERT INTO inventories (warehouse_id, variant_id, quantity_on_hand, quantity_reserved, reorder_point) VALUES
(1, 1, 10, 0, 5),
(1, 2, 5, 1, 3),
(1, 3, 20, 0, 10),
(1, 4, 2, 0, 5),
(1, 6, 15, 0, 8),
(2, 1, 3, 0, 5),
(2, 7, 8, 0, 5);

INSERT INTO wishlists (user_id, product_id) VALUES
(4, 1),
(4, 3),
(5, 2);
GO

-- =====================================================
-- 6. INSERT ORDERS, PAYMENTS, VOUCHERS, REVIEWS
-- =====================================================

-- Vouchers
SET IDENTITY_INSERT vouchers ON;
INSERT INTO vouchers (id, code, name, description, discount_type, discount_value, max_discount_amount, min_order_value, start_date, end_date, usage_limit, usage_count, is_active) VALUES
(1, 'HUIT2026', N'Chào tân sinh viên 2026', N'Giảm 10% cho đơn hàng từ 2 triệu', 'PERCENT', 10, 500000, 2000000, '2026-01-01', '2026-12-31', 1000, 0, 1),
(2, 'TET2026', N'Lì xì Tết 2026', N'Giảm 200k cho đơn từ 1 triệu', 'FIXED', 200000, 200000, 1000000, '2026-01-20', '2026-02-20', 500, 0, 1),
(3, 'FREESHIP', N'Miễn phí vận chuyển', N'Miễn phí vận chuyển đơn từ 0đ', 'FIXED', 30000, 30000, 0, '2025-01-01', '2025-12-31', 5000, 0, 1);
SET IDENTITY_INSERT vouchers OFF;

-- Orders
SET IDENTITY_INSERT orders ON;
INSERT INTO orders (id, code, user_id, subtotal, discount, shipping_fee, total, payment_method, payment_status, status, shipping_address, created_at) VALUES
(1, 'ORD-2025-001', 4, 28990000, 0, 0, 28990000, 'MOMO', 'PAID', 'COMPLETED', N'{"street":"140 Lê Trọng Tấn"}', GETDATE()),
(2, 'ORD-2026-002', 4, 6990000, 200000, 0, 6790000, 'COD', 'PENDING', 'CANCELLED', N'{"street":"Quận 1"}', GETDATE());
SET IDENTITY_INSERT orders OFF;

-- Order Items
SET IDENTITY_INSERT order_items ON;
INSERT INTO order_items (id, order_id, variant_id, product_name, sku, quantity, unit_price, total_price) VALUES
(1, 1, 1, N'iPhone 15 Pro Max 256GB', 'IP15PM-256-TI', 1, 28990000, 28990000),
(2, 2, 7, N'Sony WH-1000XM5 Màu Đen', 'SONY-XM5-BLK', 1, 6990000, 6990000);
SET IDENTITY_INSERT order_items OFF;

-- Reviews
INSERT INTO reviews (user_id, product_id, variant_id, rating, title, content, is_verified_purchase, is_approved) VALUES
(4, 1, 1, 5, N'Máy đẹp', N'Hàng chính hãng, đóng gói kĩ.', 1, 1),
(4, 5, 7, 4, N'Pin trâu', N'Chống ồn tốt, phù hợp đi du lịch.', 1, 1);

PRINT N'SEED DATA ENRICHED SUCCESSFULLY!';
GO


-- =====================================================

-- =====================================================
-- MORE ELECTRONICS: LAPTOP, WEARABLES, PC COMPONENTS
-- =====================================================
SET IDENTITY_INSERT products ON;
INSERT INTO products (id, name, slug, brand_id, category_id, description, specifications, status, created_by) VALUES 
(6, N'Asus ROG Zephyrus G14', 'asus-rog-zephyrus-g14', 4, 1, N'<p>Sản phẩm chính hãng Asus ROG Zephyrus G14</p>', N'{}', 'ACTIVE', 1),
(7, N'Dell XPS 15', 'dell-xps-15', 3, 2, N'<p>Sản phẩm chính hãng Dell XPS 15</p>', N'{}', 'ACTIVE', 1),
(8, N'MacBook Pro 16 M3 Max', 'macbook-pro-16-m3-max', 1, 2, N'<p>Sản phẩm chính hãng MacBook Pro 16 M3 Max</p>', N'{}', 'ACTIVE', 1),
(9, N'Lenovo Legion 5', 'lenovo-legion-5', 4, 1, N'<p>Sản phẩm chính hãng Lenovo Legion 5</p>', N'{}', 'ACTIVE', 1),
(10, N'HP Spectre x360', 'hp-spectre-x360', 3, 2, N'<p>Sản phẩm chính hãng HP Spectre x360</p>', N'{}', 'ACTIVE', 1),
(11, N'Apple Watch Series 9', 'apple-watch-series-9', 1, 5, N'<p>Sản phẩm chính hãng Apple Watch Series 9</p>', N'{}', 'ACTIVE', 1),
(12, N'Samsung Galaxy Watch 6 Classic', 'samsung-galaxy-watch-6-classic', 2, 5, N'<p>Sản phẩm chính hãng Samsung Galaxy Watch 6 Classic</p>', N'{}', 'ACTIVE', 1),
(13, N'Garmin Fenix 7', 'garmin-fenix-7', 5, 5, N'<p>Sản phẩm chính hãng Garmin Fenix 7</p>', N'{}', 'ACTIVE', 1),
(14, N'Xiaomi Mi Band 8', 'xiaomi-mi-band-8', 6, 5, N'<p>Sản phẩm chính hãng Xiaomi Mi Band 8</p>', N'{}', 'ACTIVE', 1),
(15, N'Huawei Watch GT 4', 'huawei-watch-gt-4', 6, 5, N'<p>Sản phẩm chính hãng Huawei Watch GT 4</p>', N'{}', 'ACTIVE', 1),
(16, N'CPU Intel Core i9-14900K', 'cpu-intel-core-i9-14900k', 3, 5, N'<p>Sản phẩm chính hãng CPU Intel Core i9-14900K</p>', N'{}', 'ACTIVE', 1),
(17, N'CPU AMD Ryzen 9 7950X3D', 'cpu-amd-ryzen-9-7950x3d', 3, 5, N'<p>Sản phẩm chính hãng CPU AMD Ryzen 9 7950X3D</p>', N'{}', 'ACTIVE', 1),
(18, N'VGA NVIDIA RTX 4090', 'vga-nvidia-rtx-4090', 4, 5, N'<p>Sản phẩm chính hãng VGA NVIDIA RTX 4090</p>', N'{}', 'ACTIVE', 1),
(19, N'VGA AMD Radeon RX 7900 XTX', 'vga-amd-radeon-rx-7900-xtx', 4, 5, N'<p>Sản phẩm chính hãng VGA AMD Radeon RX 7900 XTX</p>', N'{}', 'ACTIVE', 1),
(20, N'Mainboard Asus ROG Maximus Z790', 'mainboard-asus-rog-maximus-z790', 4, 5, N'<p>Sản phẩm chính hãng Mainboard Asus ROG Maximus Z790</p>', N'{}', 'ACTIVE', 1),
(21, N'Mainboard MSI MAG B650 Tomahawk', 'mainboard-msi-mag-b650-tomahawk', 4, 5, N'<p>Sản phẩm chính hãng Mainboard MSI MAG B650 Tomahawk</p>', N'{}', 'ACTIVE', 1),
(22, N'RAM Corsair Vengeance 32GB DDR5', 'ram-corsair-vengeance-32gb-ddr5', 5, 5, N'<p>Sản phẩm chính hãng RAM Corsair Vengeance 32GB DDR5</p>', N'{}', 'ACTIVE', 1),
(23, N'RAM G.Skill Trident Z5 64GB', 'ram-g-skill-trident-z5-64gb', 5, 5, N'<p>Sản phẩm chính hãng RAM G.Skill Trident Z5 64GB</p>', N'{}', 'ACTIVE', 1),
(24, N'SSD Samsung 990 Pro 2TB', 'ssd-samsung-990-pro-2tb', 2, 5, N'<p>Sản phẩm chính hãng SSD Samsung 990 Pro 2TB</p>', N'{}', 'ACTIVE', 1),
(25, N'SSD WD Black SN850X 1TB', 'ssd-wd-black-sn850x-1tb', 5, 5, N'<p>Sản phẩm chính hãng SSD WD Black SN850X 1TB</p>', N'{}', 'ACTIVE', 1);
SET IDENTITY_INSERT products OFF;
GO

SET IDENTITY_INSERT product_variants ON;
INSERT INTO product_variants (id, product_id, sku, variant_name, price, original_price, cost_price, thumbnail_url, display_order) VALUES 
(9, 6, 'SKU-6', N'Mặc định', 35000000, 42000000, 28000000, '/Content/Anh/Asus_ROG_Zephyrus_G14.jpg', 1),
(10, 7, 'SKU-7', N'Mặc định', 45000000, 54000000, 36000000, '/Content/Anh/Dell_XPS_15.jpg', 1),
(11, 8, 'SKU-8', N'Mặc định', 85000000, 102000000, 68000000, '/Content/Anh/MacBook_Pro_16_M3_Max.jpg', 1),
(12, 9, 'SKU-9', N'Mặc định', 30000000, 36000000, 24000000, '/Content/Anh/Lenovo_Legion_5.jpg', 1),
(13, 10, 'SKU-10', N'Mặc định', 40000000, 48000000, 32000000, '/Content/Anh/HP_Spectre_x360.jpg', 1),
(14, 11, 'SKU-11', N'Mặc định', 10000000, 12000000, 8000000, '/Content/Anh/Apple_Watch_Series_9.jpg', 1),
(15, 12, 'SKU-12', N'Mặc định', 8000000, 9600000, 6400000, '/Content/Anh/Samsung_Galaxy_Watch_6_Classic.jpg', 1),
(16, 13, 'SKU-13', N'Mặc định', 15000000, 18000000, 12000000, '/Content/Anh/Garmin_Fenix_7.jpg', 1),
(17, 14, 'SKU-14', N'Mặc định', 1000000, 1200000, 800000, '/Content/Anh/Xiaomi_Mi_Band_8.jpg', 1),
(18, 15, 'SKU-15', N'Mặc định', 6000000, 7200000, 4800000, '/Content/Anh/Huawei_Watch_GT_4.jpg', 1),
(19, 16, 'SKU-16', N'Mặc định', 16000000, 19200000, 12800000, '/Content/Anh/CPU_Intel_Core_i9_14900K.jpg', 1),
(20, 17, 'SKU-17', N'Mặc định', 18000000, 21600000, 14400000, '/Content/Anh/CPU_AMD_Ryzen_9_7950X3D.jpg', 1),
(21, 18, 'SKU-18', N'Mặc định', 45000000, 54000000, 36000000, '/Content/Anh/VGA_NVIDIA_RTX_4090.jpg', 1),
(22, 19, 'SKU-19', N'Mặc định', 30000000, 36000000, 24000000, '/Content/Anh/VGA_AMD_Radeon_RX_7900_XTX.jpg', 1),
(23, 20, 'SKU-20', N'Mặc định', 15000000, 18000000, 12000000, '/Content/Anh/Mainboard_Asus_ROG_Maximus_Z790.jpg', 1),
(24, 21, 'SKU-21', N'Mặc định', 6000000, 7200000, 4800000, '/Content/Anh/Mainboard_MSI_MAG_B650_Tomahawk.jpg', 1),
(25, 22, 'SKU-22', N'Mặc định', 3500000, 4200000, 2800000, '/Content/Anh/RAM_Corsair_Vengeance_32GB_DDR5.jpg', 1),
(26, 23, 'SKU-23', N'Mặc định', 7000000, 8400000, 5600000, '/Content/Anh/RAM_G_Skill_Trident_Z5_64GB.jpg', 1),
(27, 24, 'SKU-24', N'Mặc định', 4500000, 5400000, 3600000, '/Content/Anh/SSD_Samsung_990_Pro_2TB.jpg', 1),
(28, 25, 'SKU-25', N'Mặc định', 2500000, 3000000, 2000000, '/Content/Anh/SSD_WD_Black_SN850X_1TB.jpg', 1);
SET IDENTITY_INSERT product_variants OFF;
GO

INSERT INTO inventories (warehouse_id, variant_id, quantity_on_hand, quantity_reserved, reorder_point) VALUES 
(1, 9, 100, 0, 10),
(1, 10, 100, 0, 10),
(1, 11, 100, 0, 10),
(1, 12, 100, 0, 10),
(1, 13, 100, 0, 10),
(1, 14, 100, 0, 10),
(1, 15, 100, 0, 10),
(1, 16, 100, 0, 10),
(1, 17, 100, 0, 10),
(1, 18, 100, 0, 10),
(1, 19, 100, 0, 10),
(1, 20, 100, 0, 10),
(1, 21, 100, 0, 10),
(1, 22, 100, 0, 10),
(1, 23, 100, 0, 10),
(1, 24, 100, 0, 10),
(1, 25, 100, 0, 10),
(1, 26, 100, 0, 10),
(1, 27, 100, 0, 10),
(1, 28, 100, 0, 10);
GO



-- =====================================================
-- ADDITIONAL DATA
-- =====================================================
GO
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
INSERT INTO product_variants (product_id, sku, variant_name, price, original_price, cost_price, thumbnail_url, display_order, is_active, created_at, updated_at)
VALUES 
(@ProductId1, 'MBP-M3M-16-36-1TB', '16-inch, M3 Max, 36GB, 1TB', 89990000, 95990000, 80000000, '/Content/Anh/MacBook_Pro_M3_Max.png', 1, 1, GETDATE(), GETDATE()),
(@ProductId2, 'IPAD-M4-11-256', '11-inch, M4, 256GB WiFi', 28990000, 30990000, 25000000, '/Content/Anh/iPad_Pro_M4.png', 1, 1, GETDATE(), GETDATE()),
(@ProductId3, 'AW-U2-49-O', '49mm Titanium, Ocean Band', 21990000, 22990000, 19000000, '/Content/Anh/Apple_Watch_Ultra_2.png', 1, 1, GETDATE(), GETDATE());

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


-- =====================================================
-- FIX DB DATA
-- =====================================================
GO
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


-- =====================================================
-- SEED SERIALS
-- =====================================================
GO
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


-- =====================================================
-- SEED MISSING SERIALS
-- =====================================================
GO
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

GO
