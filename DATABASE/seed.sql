-- =====================================================
-- SEED DATA FOR ECOMMERCE HUIT
-- Database: HuitShopDB
-- Note: Sử dụng IDENTITY_INSERT cho các bảng cần ép ID.
-- Chú ý: Chạy file này SAU khi đã chạy init.sql
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
-- 2. INSERT PERMISSIONS (đã có trong init.sql, chỉ chạy nếu cần)
-- =====================================================
-- (Không cần chạy lại nếu đã có)
-- SELECT * FROM permissions; -- verify
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
-- 5. INSERT INVENTORIES & SERIALS
-- =====================================================

INSERT INTO inventories (warehouse_id, variant_id, quantity_on_hand, quantity_reserved, reorder_point) VALUES
(1, 1, 10, 0, 5),
(1, 2, 5, 1, 3),
(1, 3, 20, 0, 10),
(1, 4, 2, 0, 5),
(1, 6, 15, 0, 8),
(2, 1, 3, 0, 5),
(2, 7, 8, 0, 5);
GO

INSERT INTO product_serials (variant_id, serial_number, warehouse_id, status, inbound_date, warranty_expire_date) VALUES
-- iPhone 15 Pro Max 256GB (variant_id=1)
(1, 'IMEI-IP15-001', 1, 'AVAILABLE', GETDATE(), DATEADD(MONTH, 12, GETDATE())),
(1, 'IMEI-IP15-002', 1, 'AVAILABLE', GETDATE(), DATEADD(MONTH, 12, GETDATE())),
(1, 'IMEI-IP15-003', 1, 'SOLD', '2025-12-01', DATEADD(MONTH, 12, '2025-12-01')),
(1, 'IMEI-IP15-004', 1, 'AVAILABLE', GETDATE(), DATEADD(MONTH, 12, GETDATE())),
(1, 'IMEI-IP15-005', 1, 'AVAILABLE', GETDATE(), DATEADD(MONTH, 12, GETDATE())),
(1, 'IMEI-IP15-006', 2, 'AVAILABLE', GETDATE(), DATEADD(MONTH, 12, GETDATE())),
-- iPhone 512GB
(2, 'IMEI-IP15-512-001', 1, 'AVAILABLE', GETDATE(), DATEADD(MONTH, 12, GETDATE())),
(2, 'IMEI-IP15-512-002', 1, 'RESERVED', GETDATE(), DATEADD(MONTH, 12, GETDATE())),
-- Samsung S24 Ultra 256GB
(3, 'IMEI-SS24-001', 1, 'AVAILABLE', GETDATE(), DATEADD(MONTH, 12, GETDATE())),
(3, 'IMEI-SS24-002', 1, 'AVAILABLE', GETDATE(), DATEADD(MONTH, 12, GETDATE())),
-- Asus ROG G16
(6, 'SN-ASUS-001', 1, 'AVAILABLE', GETDATE(), DATEADD(MONTH, 12, GETDATE())),
(6, 'SN-ASUS-002', 1, 'AVAILABLE', GETDATE(), DATEADD(MONTH, 12, GETDATE())),
-- Sony XM5
(7, 'SN-SONY-XM5-001', 2, 'AVAILABLE', GETDATE(), DATEADD(MONTH, 12, GETDATE())),
(7, 'SN-SONY-XM5-002', 2, 'DEFECTIVE', GETDATE(), NULL);
GO

-- =====================================================
-- 6. INSERT STOCK MOVEMENTS (Initial and sales)
-- =====================================================

INSERT INTO stock_movements (warehouse_id, variant_id, quantity, movement_type, reference_id, note, created_at) VALUES
-- Initial stock
(1, 1, 5, 'INITIAL', NULL, N'Tồn kho ban đầu', '2025-01-01'),
(1, 1, 5, 'PURCHASE', NULL, N'Nhập lô 1', '2025-01-15'),
(1, 2, 5, 'PURCHASE', NULL, N'Nhập lô 1', '2025-01-15'),
(1, 3, 20, 'PURCHASE', NULL, N'Nhập lô 1', '2025-01-15'),
(1, 6, 15, 'PURCHASE', NULL, N'Nhập lô 1', '2025-01-15'),
(2, 1, 3, 'PURCHASE', NULL, N'Nhập vào showroom Q1', '2025-02-01'),
(2, 7, 8, 'PURCHASE', NULL, N'Nhập tai nghe', '2025-02-05'),
-- Sale reserved (from order)
(1, 1, -1, 'SALE_RESERVED', 1, N'Reserve cho đơn hàng ORD-2025-001', '2025-12-01'),
-- Sale ship (when order shipped)
(1, 1, -1, 'SALE_SHIP', 1, N'Xuất kho bán đơn ORD-2025-001', '2025-12-02');
GO

-- =====================================================
-- 7. INSERT CARTS (for users 4 and 5)
-- =====================================================

INSERT INTO carts (user_id, created_at, updated_at) VALUES
(4, GETDATE(), GETDATE()),
(5, GETDATE(), GETDATE());
GO

DECLARE @CartID4 INT = (SELECT id FROM carts WHERE user_id = 4);
DECLARE @CartID5 INT = (SELECT id FROM carts WHERE user_id = 5);

INSERT INTO cart_items (cart_id, variant_id, quantity, added_at) VALUES
(@CartID4, 1, 1, GETDATE()),
(@CartID4, 7, 2, GETDATE()),
(@CartID5, 3, 1, GETDATE());
GO

-- =====================================================
-- 8. INSERT ORDERS (Fixed IDs với IDENTITY_INSERT)
-- =====================================================
SET IDENTITY_INSERT orders ON;

INSERT INTO orders (id, code, user_id, subtotal, discount, shipping_fee, total, payment_method, payment_status, status, shipping_address, created_at) VALUES
(1, 'ORD-2025-001', 4, 28990000, 0, 0, 28990000, 'MOMO', 'PAID', 'COMPLETED',
 N'{"receiver_name":"Nguyễn Văn A","receiver_phone":"0909123456","province":"TP. Hồ Chí Minh","district":"Quận Tân Phú","ward":"Phường Tân Thới Hòa","street_address":"123 Lê Trọng Tấn","city":"TP. Hồ Chí Minh"}',
 '2025-12-01'),

(2, 'ORD-2026-002', 4, 13980000, 500000, 0, 13480000, 'COD', 'PENDING', 'PROCESSING',
 N'{"receiver_name":"Nguyễn Văn A","receiver_phone":"0909123456","province":"TP. Hồ Chí Minh","district":"Quận 1","ward":"Phường Bến Nghé","street_address":"45 Nguyễn Huệ","city":"TP. Hồ Chí Minh"}',
 '2026-02-10'),

(3, 'ORD-2026-003', 5, 45000000, 0, 0, 45000000, 'BANKING', 'PENDING', 'PENDING',
 N'{"receiver_name":"Hoàng Khách Hàng B","receiver_phone":"0909555666","province":"Hà Nội","district":"Cầu Giấy","ward":"Dịch Vọng","street_address":"123 Xuân Thủy","city":"Hà Nội"}',
 '2026-02-15'),

(4, 'ORD-2026-004', 4, 26990000, 0, 0, 26990000, 'COD', 'PENDING', 'CANCELLED',
 N'{"receiver_name":"Nguyễn Văn A","receiver_phone":"0909123456","province":"TP. Hồ Chí Minh","district":"Quận 1","ward":"Phường Nguyễn Thái Bình","street_address":"67 Nguyễn Huệ","city":"TP. Hồ Chí Minh"}',
 '2026-02-20'),

(5, 'ORD-2026-005', 2, 6990000, 0, 0, 6990000, 'CASH', 'PAID', 'COMPLETED',
 N'{"receiver_name":"Trần Văn Nhân Viên","receiver_phone":"0909333444","province":"TP. Hồ Chí Minh","district":"Quận Tân Phú","ward":"Phường Tân Thới Nhất","street_address":"456 Âu Cơ","city":"TP. Hồ Chí Minh"}',
 '2026-02-25');

SET IDENTITY_INSERT orders OFF;
GO

-- =====================================================
-- 9. INSERT ORDER ITEMS
-- =====================================================
SET IDENTITY_INSERT order_items ON;

INSERT INTO order_items (id, order_id, variant_id, product_name, sku, quantity, unit_price, total_price) VALUES
(1, 1, 1, N'iPhone 15 Pro Max 256GB', 'IP15PM-256-TI', 1, 28990000, 28990000),
(2, 2, 8, N'Sony WH-1000XM5 Màu Bạc', 'SONY-XM5-SLV', 2, 6990000, 13980000),
(3, 3, 5, N'Dell XPS 13 Plus i7/16GB/512GB', 'DELL-XPS13-16-512', 1, 45000000, 45000000),
(4, 4, 3, N'Samsung Galaxy S24 Ultra 256GB', 'SS-S24U-256-GR', 1, 26990000, 26990000),
(5, 5, 8, N'Sony WH-1000XM5 Màu Bạc', 'SONY-XM5-SLV', 1, 6990000, 6990000);

SET IDENTITY_INSERT order_items OFF;
GO

-- =====================================================
-- 10. LINK SERIALS TO ORDERS
-- =====================================================
INSERT INTO order_item_serials (order_item_id, serial_number) VALUES
(1, 'IMEI-IP15-003'); -- iPhone trong đơn ORD-2025-001
GO

-- =====================================================
-- 11. INSERT ORDER STATUS HISTORY
-- =====================================================
INSERT INTO order_status_history (order_id, status, changed_by, note) VALUES
(1, 'PENDING', NULL, N'Đặt hàng thành công'),
(1, 'CONFIRMED', 1, N'Đã xác nhận'),
(1, 'SHIPPING', 1, N'Đã xuất kho'),
(1, 'COMPLETED', 1, N'Khách nhận hàng'),

(2, 'PENDING', NULL, N'Đặt hàng thành công'),
(2, 'CONFIRMED', 2, N'Staff xác nhận'),

(3, 'PENDING', NULL, N'Đặt hàng thành công'),

(4, 'PENDING', NULL, N'Đặt hàng thành công'),
(4, 'CANCELLED', 2, N'Khách hủy'),

(5, 'PENDING', NULL, N'Đặt hàng thành công'),
(5, 'CONFIRMED', 1, N'Xác nhận'),
(5, 'COMPLETED', 1, N'Hoàn tất');
GO

-- =====================================================
-- 12. INSERT PAYMENTS
-- =====================================================
SET IDENTITY_INSERT payments ON;

INSERT INTO payments (id, order_id, payment_gateway, transaction_id, amount, status, paid_at) VALUES
(1, 1, 'MOMO', 'MOMO-123456789', 28990000, 'SUCCESS', '2025-12-01 10:15:00'),
(2, 5, 'CASH', NULL, 6990000, 'SUCCESS', '2026-02-25 14:30:00');

SET IDENTITY_INSERT payments OFF;
GO

-- =====================================================
-- 13. INSERT VOUCHERS
-- =====================================================
SET IDENTITY_INSERT vouchers ON;

INSERT INTO vouchers (id, code, name, description, discount_type, discount_value, max_discount_amount, min_order_value, start_date, end_date, usage_limit, usage_count, is_active) VALUES
(1, 'HUIT2026', N'Chào tân sinh viên 2026', N'Giảm 10% cho đơn hàng từ 2 triệu', 'PERCENT', 10, 500000, 2000000, '2026-01-01', '2026-12-31', 1000, 450, 1),
(2, 'TET2026', N'Lì xì Tết 2026', N'Giảm 200k cho đơn từ 1 triệu', 'FIXED', 200000, 200000, 1000000, '2026-01-20', '2026-02-20', 500, 120, 0); -- hết hạn

SET IDENTITY_INSERT vouchers OFF;
GO

-- =====================================================
-- 14. INSERT VOUCHER USAGES
-- =====================================================
INSERT INTO voucher_usages (voucher_id, user_id, order_id, discount_amount, used_at) VALUES
(1, 4, 1, 2899000, '2025-12-01 10:20:00');
GO

-- =====================================================
-- 15. INSERT REVIEWS
-- =====================================================
INSERT INTO reviews (user_id, product_id, variant_id, rating, title, content, is_verified_purchase, is_approved, created_at) VALUES
(4, 1, 1, 5, N'Tuyệt vời!', N'Điện thoại chạy mượt, camera đẹp. Giao hàng nhanh.', 1, 1, '2025-12-05'),
(2, 5, 7, 4, N'Chất lượng tốt', N'Tai nghe chống ồn tốt, pin trâu. Tuy nhiên hơi nặng.', 1, 1, '2026-02-28');
GO

-- =====================================================
-- 16. INSERT SUPPORT TICKETS
-- =====================================================
INSERT INTO support_tickets (ticket_number, user_id, subject, priority, status, assigned_to, order_id, created_at) VALUES
('TICK-2026-001', 4, N'Đơn hàng chưa nhận được', N'HIGH', N'IN_PROGRESS', 2, 1, '2025-12-03'),
('TICK-2026-002', 4, N'Hỏi về bảo hành', N'MEDIUM', N'OPEN', NULL, 1, '2026-02-15');
GO

-- =====================================================
-- 17. INSERT RETURNS
-- =====================================================
-- (Giả sử khách trả lại một phần)
INSERT INTO returns (return_number, order_id, order_item_id, user_id, reason, status, refund_amount, refund_method, created_at) VALUES
('RET-2026-001', 1, 1, 4, N'Sản phẩm lỗi màn hình', N'REQUESTED', 28990000, N'MOMO', '2026-02-20');
GO

-- =====================================================
-- 18. INSERT AUDIT LOGS (sample)
-- =====================================================
INSERT INTO audit_logs (table_name, record_id, operation, changed_by, changed_at, ip_address) VALUES
('products', 1, 'INSERT', 1, '2025-01-02 10:00:00', '127.0.0.1'),
('orders', 1, 'INSERT', 4, '2025-12-01 09:30:00', '192.168.1.100');
GO

PRINT N'SEED DATA INSERTED SUCCESSFULLY!';
PRINT N'================================';
PRINT N'Users: 5 (admin, staff, warehouse, customers)';
PRINT N'Products: 5 with 8 variants';
PRINT N'Warehouses: 5';
PRINT N'Orders: 5 (various statuses)';
PRINT N'Sample serial numbers: 14';
PRINT N'================================';
PRINT N'You can now test the application.';
GO
