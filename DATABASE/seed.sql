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
(3, N'Dell XPS 13 Plus', 'dell-xps-13-plus', 3, 2,
 N'<p>Laptop mỏng nhẹ, màn hình OLED, chip Intel Core i7 thế hệ mới</p>',
 N'{"screen":"13.4 inch OLED","cpu":"Intel Core i7 1360P","ram":"16GB","storage":"512GB SSD","weight":"1.23 kg"}',
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

INSERT INTO product_variants (id, product_id, sku, variant_name, price, original_price, cost_price, display_order) VALUES
(1, 1, 'IP15PM-256-TI', N'256GB - Titan Tự Nhiên', 28990000, 34990000, 26000000, 1),
(2, 1, 'IP15PM-512-BL', N'512GB - Titan Xanh', 34990000, 40990000, 31000000, 2),
(3, 2, 'SS-S24U-256-GR', N'256GB - Xám Titan', 26990000, 31990000, 24000000, 1),
(4, 2, 'SS-S24U-512-BK', N'512GB - Đen', 31990000, 36990000, 28000000, 2),
(5, 3, 'DELL-XPS13-16-512', N'i7/16GB/512GB', 45000000, 48000000, 40000000, 1),
(6, 4, 'ASUS-ROG-G16', N'i9/RTX4060/16GB/1TB', 32000000, 35000000, 29000000, 1),
(7, 5, 'SONY-XM5-BLK', N'Màu Đen', 6990000, 8490000, 5500000, 1),
(8, 5, 'SONY-XM5-SLV', N'Màu Bạc', 7190000, 8690000, 5600000, 2);

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
(1, 'ORD-2025-001', 4, 28990000, 0, 0, 28990000, 'MOMO', 'PAID', 'COMPLETED', N'{"street":"140 Lê Trọng Tấn"}', '2025-01-10'),
(2, 'ORD-2026-002', 4, 6990000, 200000, 0, 6790000, 'COD', 'PENDING', 'PROCESSING', N'{"street":"Quận 1"}', '2026-02-10');
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
