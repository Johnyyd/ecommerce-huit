-- =====================================================
-- PROJECT: ECOMMERCE HUIT - Complete Database Schema
-- Database: Microsoft SQL Server 2019+
-- Author: Tri Nguyen (Gemini AI assistance)
-- Created: 2025-02-14
-- Updated: 2025-03-03
-- =====================================================

USE master;
GO

-- Tạo Database nếu chưa có
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'HuitShopDB')
BEGIN
    CREATE DATABASE HuitShopDB;
END
GO

USE HuitShopDB;
GO

-- =====================================================
-- 1. CLEANUP (Xóa theo thứ tự để tránh lỗi FK)
-- =====================================================
SET FOREIGN_KEYS OFF; -- Không có trong SQL Server, dùng DROP theo thứ tự

DROP TABLE IF EXISTS audit_logs;
DROP TABLE IF EXISTS role_permissions;
DROP TABLE IF EXISTS permissions;
DROP TABLE IF EXISTS returns;
DROP TABLE IF EXISTS support_tickets;
DROP TABLE IF EXISTS reviews;
DROP TABLE IF EXISTS voucher_usages;
DROP TABLE IF EXISTS vouchers;
DROP TABLE IF EXISTS order_status_history;
DROP TABLE IF EXISTS order_item_serials;
DROP TABLE IF EXISTS order_items;
DROP TABLE IF EXISTS orders;
DROP TABLE IF EXISTS payments;
DROP TABLE IF EXISTS cart_items;
DROP TABLE IF EXISTS carts;
DROP TABLE IF EXISTS product_images;
DROP TABLE IF EXISTS product_serials;
DROP TABLE IF EXISTS inventories;
DROP TABLE IF EXISTS product_variants;
DROP TABLE IF EXISTS products;
DROP TABLE IF EXISTS brands;
DROP TABLE IF EXISTS categories;
DROP TABLE IF EXISTS warehouses;
DROP TABLE IF EXISTS suppliers;
DROP TABLE IF EXISTS users;
DROP TABLE IF EXISTS stock_movement_types; -- lookup table
GO

-- =====================================================
-- 2. LOOKUP TABLES (Bảng tra cứu)
-- =====================================================

CREATE TABLE stock_movement_types (
    code VARCHAR(50) PRIMARY KEY,
    name NVARCHAR(100) NOT NULL,
    description NVARCHAR(255) NULL,
    is_increase BIT NOT NULL -- 1 = tăng tồn, 0 = giảm tồn
);

INSERT INTO stock_movement_types (code, name, is_increase) VALUES
('PURCHASE', N'Nhập kho từ nhà cung cấp', 1),
('SALE_RESERVED', N'Đặt trước (Reserve)', 0),
('SALE_SHIP', N'Xuất kho bán (Ship)', 0),
('RETURN', N'Trả hàng', 1),
('TRANSFER_IN', N'Chuyển kho vào', 1),
('TRANSFER_OUT', N'Chuyển kho ra', 0),
('ADJUSTMENT_IN', N'Điều chỉnh tăng', 1),
('ADJUSTMENT_OUT', N'Điều chỉnh giảm', 0),
('INITIAL', N'Tồn kho ban đầu', 1);
GO

-- =====================================================
-- 3. MODULE: USERS & AUTH
-- =====================================================

CREATE TABLE users (
    id INT IDENTITY(1,1) PRIMARY KEY,
    full_name NVARCHAR(100) NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    phone VARCHAR(20) UNIQUE NULL,
    password_hash VARCHAR(255) NOT NULL,
    role VARCHAR(20) DEFAULT 'CUSTOMER' NOT NULL,
    CONSTRAINT CK_Users_Role CHECK (role IN ('ADMIN', 'STAFF', 'WAREHOUSE', 'CUSTOMER')),
    status VARCHAR(20) DEFAULT 'ACTIVE' NOT NULL,
    CONSTRAINT CK_Users_Status CHECK (status IN ('ACTIVE', 'BANNED')),
    avatar_url VARCHAR(500) NULL,
    last_login DATETIME2 NULL,
    created_at DATETIME2 DEFAULT GETDATE() NOT NULL
);

CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_role ON users(role);
CREATE INDEX idx_users_status ON users(status);
GO

CREATE TABLE addresses (
    id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL FOREIGN KEY REFERENCES users(id) ON DELETE CASCADE,
    label NVARCHAR(50) NOT NULL, -- 'Nhà', 'Văn phòng', 'Khác'
    receiver_name NVARCHAR(100) NOT NULL,
    receiver_phone VARCHAR(20) NOT NULL,
    province NVARCHAR(100) NOT NULL,
    district NVARCHAR(100) NOT NULL,
    ward NVARCHAR(100) NOT NULL,
    street_address NVARCHAR(255) NOT NULL,
    is_default BIT DEFAULT 0,
    created_at DATETIME2 DEFAULT GETDATE() NOT NULL
);

CREATE INDEX idx_addresses_user ON addresses(user_id);
GO

CREATE TABLE permissions (
    id INT IDENTITY(1,1) PRIMARY KEY,
    code VARCHAR(50) UNIQUE NOT NULL, -- ví dụ: 'products.read'
    name NVARCHAR(100) NOT NULL,
    module VARCHAR(50) NOT NULL -- 'PRODUCT', 'ORDER', 'INVENTORY', ...
);

CREATE INDEX idx_permissions_code ON permissions(code);
GO

CREATE TABLE role_permissions (
    role VARCHAR(20) NOT NULL,
    permission_id INT NOT NULL FOREIGN KEY REFERENCES permissions(id) ON DELETE CASCADE,
    PRIMARY KEY (role, permission_id)
);
-- Không tạo FK role → users.role vì users.role là string, nhưng logic kiểm tra trong app
GO

-- Seed permissions
INSERT INTO permissions (code, name, module) VALUES
-- Products
('products.read', N'Xem sản phẩm', 'PRODUCT'),
('products.create', N'Tạo sản phẩm', 'PRODUCT'),
('products.update', N'Sửa sản phẩm', 'PRODUCT'),
('products.delete', N'Xóa sản phẩm', 'PRODUCT'),
-- Orders
('orders.read', N'Xem đơn hàng', 'ORDER'),
('orders.update', N'Cập nhật đơn hàng', 'ORDER'),
('orders.cancel', N'Hủy đơn hàng', 'ORDER'),
('orders.return', N'Xử lý trả hàng', 'ORDER'),
-- Inventory
('inventory.read', N'Xem tồn kho', 'INVENTORY'),
('inventory.import', N'Nhập kho', 'INVENTORY'),
('inventory.transfer', N'Chuyển kho', 'INVENTORY'),
('inventory.adjust', N'Điều chỉnh tồn', 'INVENTORY'),
-- Users
('users.read', N'Xem người dùng', 'USER'),
('users.create', N'Tạo người dùng', 'USER'),
('users.update', N'Sửa người dùng', 'USER'),
('users.delete', N'Xóa người dùng', 'USER'),
-- Vouchers
('vouchers.read', N'Xem voucher', 'MARKETING'),
('vouchers.create', N'Tạo voucher', 'MARKETING'),
('vouchers.update', N'Sửa voucher', 'MARKETING'),
('reports.read', N'Xem báo cáo', 'REPORT');
GO

-- Seed role_permissions (RBAC)
-- ADMIN - all permissions
INSERT INTO role_permissions SELECT 'ADMIN', id FROM permissions;

-- STAFF - limited
INSERT INTO role_permissions VALUES
('STAFF', (SELECT id FROM permissions WHERE code='products.read')),
('STAFF', (SELECT id FROM permissions WHERE code='orders.read')),
('STAFF', (SELECT id FROM permissions WHERE code='orders.update')),
('STAFF', (SELECT id FROM permissions WHERE code='inventory.read')),
('STAFF', (SELECT id FROM permissions WHERE code='vouchers.read')),
('STAFF', (SELECT id FROM permissions WHERE code='reports.read'));

-- WAREHOUSE
INSERT INTO role_permissions VALUES
('WAREHOUSE', (SELECT id FROM permissions WHERE code='products.read')),
('WAREHOUSE', (SELECT id FROM permissions WHERE code='inventory.read')),
('WAREHOUSE', (SELECT id FROM permissions WHERE code='inventory.import')),
('WAREHOUSE', (SELECT id FROM permissions WHERE code='inventory.transfer'));

-- CUSTOMER - no special permissions (không insert vào bảng này)
GO

-- =====================================================
-- 4. MODULE: CATALOG
-- =====================================================

CREATE TABLE categories (
    id INT IDENTITY(1,1) PRIMARY KEY,
    parent_id INT NULL FOREIGN KEY REFERENCES categories(id) ON DELETE NO ACTION,
    name NVARCHAR(100) NOT NULL,
    slug VARCHAR(100) UNIQUE NOT NULL,
    description NVARCHAR(MAX) NULL,
    is_active BIT DEFAULT 1 NOT NULL,
    sort_order INT DEFAULT 0 NOT NULL
);

CREATE INDEX idx_categories_parent ON categories(parent_id);
CREATE INDEX idx_categories_slug ON categories(slug);
CREATE INDEX idx_categories_active ON categories(is_active);
GO

CREATE TABLE brands (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(100) NOT NULL UNIQUE,
    logo_url VARCHAR(500) NULL,
    origin NVARCHAR(50) NULL,
    description NVARCHAR(MAX) NULL,
    website VARCHAR(200) NULL
);

CREATE INDEX idx_brands_name ON brands(name);
GO

CREATE TABLE products (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(255) NOT NULL,
    slug VARCHAR(255) UNIQUE NOT NULL,
    brand_id INT NULL FOREIGN KEY REFERENCES brands(id) ON DELETE SET NULL,
    category_id INT NOT NULL FOREIGN KEY REFERENCES categories(id) ON DELETE NO ACTION,
    short_description NVARCHAR(500) NULL,
    description NVARCHAR(MAX) NULL,
    specifications NVARCHAR(MAX) NULL, -- JSON string
    meta_title NVARCHAR(200) NULL,
    meta_description NVARCHAR(500) NULL,
    status VARCHAR(20) DEFAULT 'DRAFT' NOT NULL,
    CONSTRAINT CK_Products_Status CHECK (status IN ('DRAFT', 'ACTIVE', 'HIDDEN')),
    is_featured BIT DEFAULT 0 NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE() NOT NULL,
    updated_at DATETIME2 DEFAULT GETDATE() NOT NULL,
    created_by INT NOT NULL FOREIGN KEY REFERENCES users(id)
);

CREATE INDEX idx_products_slug ON products(slug);
CREATE INDEX idx_products_brand ON products(brand_id);
CREATE INDEX idx_products_category ON products(category_id);
CREATE INDEX idx_products_status ON products(status);
CREATE INDEX idx_products_featured ON products(is_featured);
GO

CREATE TRIGGER trg_products_updatetimestamp
ON products
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE p
    SET updated_at = GETDATE()
    FROM products p
    INNER JOIN inserted i ON p.id = i.id;
END;
GO

CREATE TABLE product_variants (
    id INT IDENTITY(1,1) PRIMARY KEY,
    product_id INT NOT NULL FOREIGN KEY REFERENCES products(id) ON DELETE CASCADE,
    sku VARCHAR(50) UNIQUE NOT NULL,
    variant_name NVARCHAR(255) NULL,
    price DECIMAL(15,2) NOT NULL CHECK (price >= 0),
    original_price DECIMAL(15,2) NULL CHECK (original_price >= 0),
    cost_price DECIMAL(15,2) NULL CHECK (cost_price >= 0),
    thumbnail_url VARCHAR(500) NULL,
    display_order INT DEFAULT 0 NOT NULL,
    is_active BIT DEFAULT 1 NOT NULL,
    weight_grams INT NULL CHECK (weight_grams > 0),
    dimensions NVARCHAR(100) NULL, -- JSON: {"length":..., "width":..., "height":...}
    CONSTRAINT CK_Variants_Price CHECK (original_price >= price) --original >= sale
);

CREATE INDEX idx_variants_sku ON product_variants(sku);
CREATE INDEX idx_variants_product ON product_variants(product_id);
CREATE INDEX idx_variants_active ON product_variants(is_active);
GO

CREATE TABLE product_images (
    id INT IDENTITY(1,1) PRIMARY KEY,
    variant_id INT NOT NULL FOREIGN KEY REFERENCES product_variants(id) ON DELETE CASCADE,
    image_url VARCHAR(500) NOT NULL,
    alt_text NVARCHAR(200) NULL,
    sort_order INT DEFAULT 0 NOT NULL
);

CREATE INDEX idx_images_variant ON product_images(variant_id);
GO

-- =====================================================
-- 5. MODULE: WAREHOUSE & INVENTORY
-- =====================================================

CREATE TABLE warehouses (
    id INT IDENTITY(1,1) PRIMARY KEY,
    code VARCHAR(20) UNIQUE NOT NULL,
    name NVARCHAR(100) NOT NULL,
    address NVARCHAR(255) NULL,
    type VARCHAR(20) DEFAULT 'PHYSICAL' NOT NULL,
    CONSTRAINT CK_Warehouses_Type CHECK (type IN ('PHYSICAL','VIRTUAL')),
    phone VARCHAR(20) NULL,
    manager NVARCHAR(100) NULL,
    is_active BIT DEFAULT 1 NOT NULL
);

CREATE INDEX idx_warehouses_code ON warehouses(code);
CREATE INDEX idx_warehouses_active ON warehouses(is_active);
GO

CREATE TABLE suppliers (
    id INT IDENTITY(1,1) PRIMARY KEY,
    code VARCHAR(20) UNIQUE NOT NULL,
    name NVARCHAR(200) NOT NULL,
    contact_person NVARCHAR(100) NULL,
    phone VARCHAR(20) NULL,
    email VARCHAR(100) NULL,
    address NVARCHAR(MAX) NULL,
    tax_code VARCHAR(50) NULL,
    bank_account NVARCHAR(100) NULL,
    is_active BIT DEFAULT 1 NOT NULL
);

CREATE INDEX idx_suppliers_code ON suppliers(code);
GO

CREATE TABLE inventories (
    warehouse_id INT NOT NULL FOREIGN KEY REFERENCES warehouses(id) ON DELETE NO ACTION,
    variant_id INT NOT NULL FOREIGN KEY REFERENCES product_variants(id) ON DELETE NO ACTION,
    quantity_on_hand INT DEFAULT 0 NOT NULL CHECK (quantity_on_hand >= 0),
    quantity_reserved INT DEFAULT 0 NOT NULL CHECK (quantity_reserved >= 0),
    reorder_point INT DEFAULT 10 NOT NULL CHECK (reorder_point >= 0),
    last_updated DATETIME2 DEFAULT GETDATE() NOT NULL,
    PRIMARY KEY (warehouse_id, variant_id)
);

CREATE INDEX idx_inventories_variant ON inventories(variant_id);
CREATE INDEX idx_inventories_lowstock ON inventories(quantity_on_hand, reorder_point) WHERE quantity_on_hand <= reorder_point;
GO

CREATE TABLE product_serials (
    id INT IDENTITY(1,1) PRIMARY KEY,
    variant_id INT NOT NULL FOREIGN KEY REFERENCES product_variants(id) ON DELETE NO ACTION,
    serial_number VARCHAR(100) UNIQUE NOT NULL,
    warehouse_id INT NOT NULL FOREIGN KEY REFERENCES warehouses(id) ON DELETE NO ACTION,
    status VARCHAR(20) DEFAULT 'AVAILABLE' NOT NULL,
    CONSTRAINT CK_Serials_Status CHECK (status IN ('AVAILABLE','RESERVED','SOLD','DEFECTIVE','TRANSFERRING','RETURNED')),
    inbound_date DATETIME2 DEFAULT GETDATE() NOT NULL,
    outbound_date DATETIME2 NULL,
    warranty_expire_date DATE NULL,
    notes NVARCHAR(MAX) NULL
);

CREATE INDEX idx_serials_number ON product_serials(serial_number);
CREATE INDEX idx_serials_variant ON product_serials(variant_id);
CREATE INDEX idx_serials_warehouse ON product_serials(warehouse_id);
CREATE INDEX idx_serials_status ON product_serials(status);
GO

CREATE TABLE stock_movements (
    id INT IDENTITY(1,1) PRIMARY KEY,
    warehouse_id INT NOT NULL FOREIGN KEY REFERENCES warehouses(id),
    variant_id INT NOT NULL FOREIGN KEY REFERENCES product_variants(id),
    quantity INT NOT NULL CHECK (quantity != 0),
    movement_type VARCHAR(50) NOT NULL, -- Phải có trong bảng stock_movement_types
    reference_id INT NULL, -- order_id, purchase_id, ...
    reference_type VARCHAR(50) NULL, -- 'ORDER', 'PURCHASE', 'TRANSFER'
    supplier_id INT NULL FOREIGN KEY REFERENCES suppliers(id),
    note NVARCHAR(MAX) NULL,
    created_by INT NULL FOREIGN KEY REFERENCES users(id),
    created_at DATETIME2 DEFAULT GETDATE() NOT NULL
);

CREATE INDEX idx_movements_warehouse ON stock_movements(warehouse_id);
CREATE INDEX idx_movements_variant ON stock_movements(variant_id);
CREATE INDEX idx_movements_created ON stock_movements(created_at);
CREATE INDEX idx_movements_ref ON stock_movements(reference_id, reference_type);
GO

-- =====================================================
-- 6. MODULE: CART
-- =====================================================

CREATE TABLE carts (
    id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT UNIQUE NOT NULL FOREIGN KEY REFERENCES users(id) ON DELETE CASCADE,
    voucher_code VARCHAR(20) NULL,
    created_at DATETIME2 DEFAULT GETDATE() NOT NULL,
    updated_at DATETIME2 DEFAULT GETDATE() NOT NULL
);

CREATE INDEX idx_carts_user ON carts(user_id);
GO

CREATE TABLE cart_items (
    id INT IDENTITY(1,1) PRIMARY KEY,
    cart_id INT NOT NULL FOREIGN KEY REFERENCES carts(id) ON DELETE CASCADE,
    variant_id INT NOT NULL FOREIGN KEY REFERENCES product_variants(id) ON DELETE NO ACTION,
    quantity INT NOT NULL CHECK (quantity > 0),
    added_at DATETIME2 DEFAULT GETDATE() NOT NULL,
    CONSTRAINT UQ_CartItems_CartVariant UNIQUE (cart_id, variant_id)
);

CREATE INDEX idx_cartitems_cart ON cart_items(cart_id);
CREATE INDEX idx_cartitems_variant ON cart_items(variant_id);
GO

-- =====================================================
-- 7. MODULE: ORDERS & PAYMENTS
-- =====================================================

CREATE TABLE orders (
    id INT IDENTITY(1,1) PRIMARY KEY,
    code VARCHAR(20) UNIQUE NOT NULL,
    user_id INT NOT NULL FOREIGN KEY REFERENCES users(id),
    order_type VARCHAR(20) DEFAULT 'ONLINE' NOT NULL,
    CONSTRAINT CK_Orders_Type CHECK (order_type IN ('ONLINE','POS','B2B')),
    subtotal DECIMAL(15,2) NOT NULL CHECK (subtotal >= 0),
    discount DECIMAL(15,2) DEFAULT 0 NOT NULL CHECK (discount >= 0),
    shipping_fee DECIMAL(15,2) DEFAULT 0 NOT NULL CHECK (shipping_fee >= 0),
    tax_amount DECIMAL(15,2) DEFAULT 0 NOT NULL CHECK (tax_amount >= 0),
    total DECIMAL(15,2) NOT NULL CHECK (total >= 0),
    payment_method VARCHAR(50) NOT NULL, -- 'CASH','MOMO','VNPAY','BANKING','COD'
    payment_status VARCHAR(20) DEFAULT 'PENDING' NOT NULL,
    CONSTRAINT CK_Orders_PayStatus CHECK (payment_status IN ('PENDING','PAID','FAILED','REFUNDED')),
    status VARCHAR(20) DEFAULT 'PENDING' NOT NULL,
    CONSTRAINT CK_Orders_Status CHECK (status IN ('PENDING','CONFIRMED','PROCESSING','SHIPPING','COMPLETED','CANCELLED','RETURNED')),
    shipping_address NVARCHAR(MAX) NOT NULL, -- JSON
    CONSTRAINT CK_Orders_Address_JSON CHECK (ISJSON(shipping_address)=1),
    note NVARCHAR(MAX) NULL,
    staff_note NVARCHAR(MAX) NULL,
    created_at DATETIME2 DEFAULT GETDATE() NOT NULL,
    updated_at DATETIME2 DEFAULT GETDATE() NOT NULL
);

CREATE INDEX idx_orders_code ON orders(code);
CREATE INDEX idx_orders_user ON orders(user_id);
CREATE INDEX idx_orders_created ON orders(created_at);
CREATE INDEX idx_orders_status ON orders(status);
CREATE INDEX idx_orders_payment_status ON orders(payment_status);
GO

CREATE TRIGGER trg_orders_updatetimestamp
ON orders
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE o
    SET updated_at = GETDATE()
    FROM orders o
    INNER JOIN inserted i ON o.id = i.id;
END;
GO

CREATE TABLE order_items (
    id INT IDENTITY(1,1) PRIMARY KEY,
    order_id INT NOT NULL FOREIGN KEY REFERENCES orders(id) ON DELETE CASCADE,
    variant_id INT NOT NULL FOREIGN KEY REFERENCES product_variants(id),
    product_name NVARCHAR(255) NOT NULL, -- snapshot
    sku VARCHAR(50) NOT NULL, -- snapshot
    quantity INT NOT NULL CHECK (quantity > 0),
    unit_price DECIMAL(15,2) NOT NULL CHECK (unit_price >= 0), -- snapshot
    cost_price DECIMAL(15,2) NULL, -- snapshot
    total_price DECIMAL(15,2) NOT NULL CHECK (total_price >= 0),
    discount_amount DECIMAL(15,2) DEFAULT 0 NOT NULL CHECK (discount_amount >= 0)
);

CREATE INDEX idx_orderitems_order ON order_items(order_id);
CREATE INDEX idx_orderitems_variant ON order_items(variant_id);
GO

CREATE TABLE order_item_serials (
    order_item_id INT NOT NULL FOREIGN KEY REFERENCES order_items(id) ON DELETE CASCADE,
    serial_number VARCHAR(100) NOT NULL,
    PRIMARY KEY (order_item_id, serial_number)
);

CREATE INDEX idx_ois_serial ON order_item_serials(serial_number);
GO

CREATE TABLE order_status_history (
    id INT IDENTITY(1,1) PRIMARY KEY,
    order_id INT NOT NULL FOREIGN KEY REFERENCES orders(id) ON DELETE CASCADE,
    status VARCHAR(20) NOT NULL,
    changed_by INT NULL FOREIGN KEY REFERENCES users(id),
    note NVARCHAR(MAX) NULL,
    created_at DATETIME2 DEFAULT GETDATE() NOT NULL
);

CREATE INDEX idx_osh_order ON order_status_history(order_id);
CREATE INDEX idx_osh_created ON order_status_history(created_at);
GO

CREATE TABLE payments (
    id INT IDENTITY(1,1) PRIMARY KEY,
    order_id INT UNIQUE NOT NULL FOREIGN KEY REFERENCES orders(id),
    payment_gateway VARCHAR(50) NOT NULL,
    transaction_id VARCHAR(100) UNIQUE NULL, -- từ gateway
    amount DECIMAL(15,2) NOT NULL CHECK (amount >= 0),
    fee DECIMAL(15,2) DEFAULT 0 NOT NULL CHECK (fee >= 0),
    status VARCHAR(20) DEFAULT 'PENDING' NOT NULL,
    CONSTRAINT CK_Payments_Status CHECK (status IN ('PENDING','SUCCESS','FAILED','CANCELLED','REFUNDED')),
    paid_at DATETIME2 NULL,
    webhook_data NVARCHAR(MAX) NULL, -- JSON raw response từ gateway
    created_at DATETIME2 DEFAULT GETDATE() NOT NULL
);

CREATE INDEX idx_payments_order ON payments(order_id);
CREATE INDEX idx_payments_transaction ON payments(transaction_id) WHERE transaction_id IS NOT NULL;
CREATE INDEX idx_payments_status ON payments(status);
GO

-- =====================================================
-- 8. MODULE: MARKETING (VOUCHERS)
-- =====================================================

CREATE TABLE vouchers (
    id INT IDENTITY(1,1) PRIMARY KEY,
    code VARCHAR(20) UNIQUE NOT NULL,
    name NVARCHAR(255) NOT NULL,
    description NVARCHAR(500) NULL,
    discount_type VARCHAR(10) NOT NULL,
    CONSTRAINT CK_Vouchers_Type CHECK (discount_type IN ('PERCENT','FIXED')),
    discount_value DECIMAL(15,2) NOT NULL CHECK (discount_value > 0),
    max_discount_amount DECIMAL(15,2) NULL CHECK (max_discount_amount >= 0),
    min_order_value DECIMAL(15,2) DEFAULT 0 NOT NULL CHECK (min_order_value >= 0),
    applicable_product_ids NVARCHAR(MAX) NULL, -- JSON array
    applicable_category_ids NVARCHAR(MAX) NULL, -- JSON array
    start_date DATETIME2 NOT NULL,
    end_date DATETIME2 NOT NULL,
    usage_limit INT NULL CHECK (usage_limit > 0),
    usage_per_user INT DEFAULT 1 NOT NULL CHECK (usage_per_user > 0),
    usage_count INT DEFAULT 0 NOT NULL CHECK (usage_count >= 0),
    is_active BIT DEFAULT 1 NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE() NOT NULL,
    updated_at DATETIME2 DEFAULT GETDATE() NOT NULL
);

CREATE INDEX idx_vouchers_code ON vouchers(code);
CREATE INDEX idx_vouchers_dates ON vouchers(start_date, end_date, is_active);
GO

CREATE TRIGGER trg_vouchers_updatetimestamp
ON vouchers
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE v
    SET updated_at = GETDATE()
    FROM vouchers v
    INNER JOIN inserted i ON v.id = i.id;
END;
GO

CREATE TABLE voucher_usages (
    id INT IDENTITY(1,1) PRIMARY KEY,
    voucher_id INT NOT NULL FOREIGN KEY REFERENCES vouchers(id),
    user_id INT NOT NULL FOREIGN KEY REFERENCES users(id),
    order_id INT NOT NULL FOREIGN KEY REFERENCES orders(id),
    discount_amount DECIMAL(15,2) NOT NULL CHECK (discount_amount >= 0),
    used_at DATETIME2 DEFAULT GETDATE() NOT NULL,
    CONSTRAINT UQ_VoucherUsage_Order UNIQUE (voucher_id, order_id)
);

CREATE INDEX idx_vusage_voucher ON voucher_usages(voucher_id);
CREATE INDEX idx_vusage_user ON voucher_usages(user_id);
GO

-- =====================================================
-- 9. MODULE: SUPPORT & REVIEWS
-- =====================================================

CREATE TABLE reviews (
    id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL FOREIGN KEY REFERENCES users(id),
    product_id INT NOT NULL FOREIGN KEY REFERENCES products(id),
    variant_id INT NULL FOREIGN KEY REFERENCES product_variants(id),
    rating INT NOT NULL CHECK (rating BETWEEN 1 AND 5),
    title NVARCHAR(200) NULL,
    content NVARCHAR(MAX) NOT NULL,
    is_verified_purchase BIT DEFAULT 0 NOT NULL,
    is_approved BIT DEFAULT 0 NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE() NOT NULL
);

CREATE INDEX idx_reviews_product ON reviews(product_id);
CREATE INDEX idx_reviews_user ON reviews(user_id);
CREATE INDEX idx_reviews_rating ON reviews(rating);
GO

CREATE TABLE support_tickets (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ticket_number VARCHAR(20) UNIQUE NOT NULL,
    user_id INT NOT NULL FOREIGN KEY REFERENCES users(id),
    subject NVARCHAR(200) NOT NULL,
    priority VARCHAR(20) DEFAULT 'MEDIUM' NOT NULL,
    CONSTRAINT CK_Tickets_Priority CHECK (priority IN ('LOW','MEDIUM','HIGH','URGENT')),
    status VARCHAR(20) DEFAULT 'OPEN' NOT NULL,
    CONSTRAINT CK_Tickets_Status CHECK (status IN ('OPEN','IN_PROGRESS','WAITING_CUSTOMER','RESOLVED','CLOSED')),
    assigned_to INT NULL FOREIGN KEY REFERENCES users(id),
    order_id INT NULL FOREIGN KEY REFERENCES orders(id),
    product_id INT NULL FOREIGN KEY REFERENCES products(id),
    last_message_at DATETIME2 NULL,
    created_at DATETIME2 DEFAULT GETDATE() NOT NULL
);

CREATE INDEX idx_tickets_user ON support_tickets(user_id);
CREATE INDEX idx_tickets_status ON support_tickets(status);
CREATE INDEX idx_tickets_priority ON support_tickets(priority);
CREATE INDEX idx_tickets_assigned ON support_tickets(assigned_to) WHERE assigned_to IS NOT NULL;
GO

CREATE TABLE returns (
    id INT IDENTITY(1,1) PRIMARY KEY,
    return_number VARCHAR(20) UNIQUE NOT NULL,
    order_id INT NOT NULL FOREIGN KEY REFERENCES orders(id),
    order_item_id INT NOT NULL FOREIGN KEY REFERENCES order_items(id),
    user_id INT NOT NULL FOREIGN KEY REFERENCES users(id),
    reason NVARCHAR(500) NOT NULL,
    status VARCHAR(20) DEFAULT 'REQUESTED' NOT NULL,
    CONSTRAINT CK_Returns_Status CHECK (status IN ('REQUESTED','APPROVED','REJECTED','RECEIVED','REFUNDED','COMPLETED')),
    refund_amount DECIMAL(15,2) NULL CHECK (refund_amount >= 0),
    refund_method VARCHAR(50) NULL,
    created_at DATETIME2 DEFAULT GETDATE() NOT NULL,
    resolved_at DATETIME2 NULL
);

CREATE INDEX idx_returns_order ON returns(order_id);
CREATE INDEX idx_returns_user ON returns(user_id);
CREATE INDEX idx_returns_status ON returns(status);
GO

-- =====================================================
-- 10. MODULE: AUDIT LOGS
-- =====================================================

CREATE TABLE audit_logs (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    table_name VARCHAR(50) NOT NULL,
    record_id INT NOT NULL,
    operation VARCHAR(10) NOT NULL CHECK (operation IN ('INSERT','UPDATE','DELETE')),
    old_values NVARCHAR(MAX) NULL, -- JSON
    new_values NVARCHAR(MAX) NULL, -- JSON
    changed_by INT NULL FOREIGN KEY REFERENCES users(id),
    changed_at DATETIME2 DEFAULT GETDATE() NOT NULL,
    ip_address VARCHAR(45) NULL
);

CREATE INDEX idx_audit_table_record ON audit_logs(table_name, record_id);
CREATE INDEX idx_audit_changed ON audit_logs(changed_at);
CREATE INDEX idx_audit_user ON audit_logs(changed_by);
GO

-- =====================================================
-- 11. STORED PROCEDURES
-- =====================================================

-- sp_ImportStock
CREATE OR ALTER PROCEDURE sp_ImportStock
    @WarehouseID INT,
    @VariantID INT,
    @CostPrice DECIMAL(15,2),
    @SupplierID INT = NULL,
    @ListIMEI NVARCHAR(MAX), -- JSON: ["IMEI001","IMEI002"]
    @CreatedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Parse JSON để lấy số lượng
        DECLARE @Quantity INT;
        SELECT @Quantity = COUNT(*) FROM OPENJSON(@ListIMEI);

        -- 1. Insert serials
        INSERT INTO product_serials (variant_id, warehouse_id, serial_number, status, inbound_date, notes)
        SELECT @VariantID, @WarehouseID, value, 'AVAILABLE', GETDATE(), N'Nhập kho lô mới'
        FROM OPENJSON(@ListIMEI);

        -- 2. Upsert inventories
        MERGE inventories AS target
        USING (SELECT @WarehouseID AS warehouse_id, @VariantID AS variant_id) AS source
        ON (target.warehouse_id = source.warehouse_id AND target.variant_id = source.variant_id)
        WHEN MATCHED THEN
            UPDATE SET quantity_on_hand = quantity_on_hand + @Quantity,
                       last_updated = GETDATE()
        WHEN NOT MATCHED THEN
            INSERT (warehouse_id, variant_id, quantity_on_hand, quantity_reserved)
            VALUES (@WarehouseID, @VariantID, @Quantity, 0);

        -- 3. Insert stock movement
        INSERT INTO stock_movements (warehouse_id, variant_id, quantity, movement_type, supplier_id, note, created_by)
        VALUES (@WarehouseID, @VariantID, @Quantity, 'PURCHASE', @SupplierID, N'Nhập hàng từ supplier', @CreatedBy);

        -- 4. Update cost_price trên variant
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

-- sp_CreateOrder
CREATE OR ALTER PROCEDURE sp_CreateOrder
    @UserID INT,
    @ShippingAddress NVARCHAR(MAX), -- JSON
    @PaymentMethod VARCHAR(50),
    @OrderItemsJSON NVARCHAR(MAX), -- [{"variant_id":1,"quantity":1,"price":20000000}]
    @OrderID INT OUTPUT,
    @OrderCode VARCHAR(20) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Validate payment method
        IF @PaymentMethod NOT IN ('CASH','MOMO','VNPAY','BANKING','COD')
        BEGIN
            RAISERROR('Invalid payment method', 16, 1);
            RETURN;
        END

        -- Tính tổng tiền
        DECLARE @Subtotal DECIMAL(15,2);
        SELECT @Subtotal = SUM(quantity * price)
        FROM OPENJSON(@OrderItemsJSON)
        WITH (
            variant_id INT,
            quantity INT,
            price DECIMAL(15,2)
        );

        IF @Subtotal IS NULL SET @Subtotal = 0;

        -- Tạo mã đơn hàng
        SET @OrderCode = 'ORD-' + FORMAT(GETDATE(), 'yyyyMMddHHmmss');

        -- 1. Insert order
        INSERT INTO orders (code, user_id, subtotal, total, payment_method, shipping_address, status)
        VALUES (@OrderCode, @UserID, @Subtotal, @Subtotal, @PaymentMethod, @ShippingAddress, 'PENDING');

        SET @OrderID = SCOPE_IDENTITY();

        -- 2. Insert order items & kiểm tra tồn kho + Reserve
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

        -- Kiểm tra tồn kho và lấy product_name, sku
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
            -- Check available quantity (on hand - reserved)
            DECLARE @Available INT;
            SELECT @Available = quantity_on_hand - quantity_reserved
            FROM inventories
            WHERE warehouse_id = 1 -- TODO: logic chọn warehouse (ví dụ: warehouse mặc định của user)
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

        -- Insert order items (with snapshot)
        INSERT INTO order_items (order_id, variant_id, product_name, sku, quantity, unit_price, total_price)
        SELECT @OrderID, i.variant_id, p.name + CASE WHEN v.variant_name IS NULL THEN '' ELSE ' ' + v.variant_name END, v.sku, i.quantity, i.price, (i.quantity * i.price)
        FROM @Items i
        JOIN product_variants v ON v.id = i.variant_id
        JOIN products p ON v.product_id = p.id;

        -- Reserve inventory: +quantity_reserved
        UPDATE inv
        SET quantity_reserved = quantity_reserved + i.quantity,
            last_updated = GETDATE()
        FROM inventories inv
        JOIN @Items i ON inv.variant_id = i.variant_id
        WHERE inv.warehouse_id = 1; -- same hardcoded warehouse

        -- Log stock movements (type: SALE_RESERVED)
        INSERT INTO stock_movements (warehouse_id, variant_id, quantity, movement_type, reference_id, note)
        SELECT 1, i.variant_id, -i.quantity, 'SALE_RESERVED', @OrderID, N'Reserve for order ' + @OrderCode
        FROM @Items i;

        -- Insert order status history
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

-- sp_ConfirmOrder
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

-- sp_ShipOrder (allocate serials and ship)
CREATE OR ALTER PROCEDURE sp_ShipOrder
    @OrderID INT,
    @WarehouseID INT,
    @StaffID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Kiểm tra trạng thái order phải là CONFIRMED hoặc PROCESSING
        DECLARE @CurrentStatus VARCHAR(20);
        SELECT @CurrentStatus = status FROM orders WHERE id = @OrderID;

        IF @CurrentStatus NOT IN ('CONFIRMED','PROCESSING')
        BEGIN
            RAISERROR('Order cannot be shipped from current status', 16, 1);
            RETURN;
        END

        -- Lấy danh sách serial numbers từ order items
        -- Giả định: trước khi ship, staff đã gắn serial vào order_item_serials
        -- Kiểm tra tất cả serial đều AVAILABLE (hoặc RESERVED)
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

        -- Cập nhật serial status thành SOLD và outbound_date
        UPDATE ps
        SET status = 'SOLD',
            outbound_date = GETDATE(),
            warranty_expire_date = DATEADD(MONTH, 12, GETDATE())
        FROM product_serials ps
        JOIN order_item_serials ois ON ps.serial_number = ois.serial_number
        WHERE ois.order_item_id IN (SELECT id FROM order_items WHERE order_id = @OrderID);

        -- Giảm quantity_reserved trong inventories (đã giữ từ lúc order)
        UPDATE inv
        SET quantity_reserved = quantity_reserved - oi.quantity,
            last_updated = GETDATE()
        FROM inventories inv
        JOIN order_items oi ON inv.variant_id = oi.variant_id
        WHERE oi.order_id = @OrderID AND inv.warehouse_id = @WarehouseID;

        -- Log stock movement (type: SALE_SHIP)
        INSERT INTO stock_movements (warehouse_id, variant_id, quantity, movement_type, reference_id, note)
        SELECT @WarehouseID, oi.variant_id, -oi.quantity, 'SALE_SHIP', @OrderID, N'Xuất kho bán hàng'
        FROM order_items oi
        WHERE oi.order_id = @OrderID;

        -- Update order status to SHIPPING
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

-- sp_CompleteOrder
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

-- sp_CancelOrder
CREATE OR ALTER PROCEDURE sp_CancelOrder
    @OrderID INT,
    @Reason NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Lấy thông tin order để hoàn trả tồn kho
        DECLARE @Status VARCHAR(20);
        SELECT @Status = status FROM orders WHERE id = @OrderID;

        IF @Status NOT IN ('PENDING','CONFIRMED','PROCESSING')
        BEGIN
            RAISERROR('Order cannot be cancelled from current status', 16, 1);
            RETURN;
        END

        -- Hoàn trả quantity_reserved
        UPDATE inv
        SET quantity_reserved = quantity_reserved - oi.quantity,
            last_updated = GETDATE()
        FROM inventories inv
        JOIN order_items oi ON inv.variant_id = oi.variant_id
        WHERE oi.order_id = @OrderID;

        -- Log return movement
        INSERT INTO stock_movements (warehouse_id, variant_id, quantity, movement_type, reference_id, note)
        SELECT 1, oi.variant_id, oi.quantity, 'ADJUSTMENT_IN', @OrderID, N'Hủy đơn, hoàn trả tồn kho'
        FROM order_items oi
        WHERE oi.order_id = @OrderID;

        -- Update order
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

-- sp_ProcessReturn
CREATE OR ALTER PROCEDURE sp_ProcessReturn
    @ReturnID INT,
    @Action VARCHAR(20) -- 'APPROVE' or 'REJECT'
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

        -- Lấy serial number (giả sử return là cho 1 item; nếu nhiều item cần bảng return_items riêng)
        SELECT TOP 1 @SerialNumber = serial_number
        FROM order_item_serials
        WHERE order_item_id = @OrderItemID;

        SELECT @VariantID = oi.variant_id
        FROM order_items oi
        WHERE oi.id = @OrderItemID;

        -- Giả định warehouse mặc định 1
        SET @WarehouseID = 1;

        IF @Action = 'APPROVE'
        BEGIN
            -- Trả về AVAILABLE
            UPDATE product_serials
            SET status = 'AVAILABLE',
                warranty_expire_date = NULL,
                notes = ISNULL(notes, '') + ' | Returned and approved'
            WHERE serial_number = @SerialNumber;

            -- Tăng tồn kho
            UPDATE inventories
            SET quantity_on_hand = quantity_on_hand + 1,
                last_updated = GETDATE()
            WHERE warehouse_id = @WarehouseID AND variant_id = @VariantID;

            -- Log movement
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

-- ufn_CalculateDiscount
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
    DECLARE @UsageCount INT;
    DECLARE @UsageLimit INT;

    SELECT @DiscountType = discount_type,
           @DiscountValue = discount_value,
           @MaxDiscount = max_discount_amount,
           @MinOrder = min_order_value,
           @UsageCount = usage_count,
           @UsageLimit = usage_limit
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

-- sp_GetLowStockReport
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
    WHERE @WarehouseID IS NULL OR i.warehouse_id = @WarehouseID
      AND (i.quantity_on_hand - i.quantity_reserved) <= i.reorder_point
      AND w.is_active = 1
    ORDER BY w.id, p.id;
END;
GO

-- sp_GetRevenueReport
CREATE OR ALTER PROCEDURE sp_GetRevenueReport
    @FromDate DATE,
    @ToDate DATE,
    @GroupBy VARCHAR(20) = 'DAY' -- 'DAY','MONTH','YEAR'
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

-- =====================================================
-- 12. VIEWS
-- =====================================================

CREATE VIEW vw_ProductDetails AS
SELECT
    p.id,
    p.name,
    p.slug,
    p.description,
    p.specifications,
    b.id as brand_id,
    b.name as brand_name,
    b.origin as brand_origin,
    b.logo_url as brand_logo,
    c.id as category_id,
    c.name as category_name,
    c.slug as category_slug,
    v.id as variant_id,
    v.sku,
    v.variant_name,
    v.price,
    v.original_price,
    v.thumbnail_url,
    v.is_active as variant_active,
    ISNULL(SUM(i.quantity_on_hand), 0) as total_stock
FROM products p
LEFT JOIN brands b ON p.brand_id = b.id
LEFT JOIN categories c ON p.category_id = c.id
LEFT JOIN product_variants v ON v.product_id = p.id
LEFT JOIN inventories i ON i.variant_id = v.id
WHERE p.status = 'ACTIVE'
GROUP BY p.id, p.name, p.slug, p.description, p.specifications,
         b.id, b.name, b.origin, b.logo_url,
         c.id, c.name, c.slug,
         v.id, v.sku, v.variant_name, v.price, v.original_price, v.thumbnail_url, v.is_active;
GO

CREATE VIEW vw_OrderDetails AS
SELECT
    o.id,
    o.code,
    o.subtotal,
    o.discount,
    o.shipping_fee,
    o.total,
    o.payment_method,
    o.payment_status,
    o.status as order_status,
    o.created_at,
    o.shipping_address,
    u.id as user_id,
    u.full_name as user_name,
    u.email as user_email,
    u.phone as user_phone,
    oi.id as item_id,
    oi.product_name,
    oi.sku,
    oi.quantity,
    oi.unit_price,
    oi.total_price,
    ois.serial_number
FROM orders o
JOIN users u ON o.user_id = u.id
LEFT JOIN order_items oi ON oi.order_id = o.id
LEFT JOIN order_item_serials ois ON ois.order_item_id = oi.id;
GO

CREATE VIEW vw_InventoryDashboard AS
SELECT
    w.id as warehouse_id,
    w.code as warehouse_code,
    w.name as warehouse_name,
    v.id as variant_id,
    v.sku,
    p.name as product_name,
    v.variant_name,
    ISNULL(i.quantity_on_hand, 0) as quantity_on_hand,
    ISNULL(i.quantity_reserved, 0) as quantity_reserved,
    ISNULL(i.quantity_on_hand - i.quantity_reserved, 0) as available_quantity,
    i.reorder_point
FROM warehouses w
CROSS JOIN product_variants v
JOIN products p ON v.product_id = p.id
LEFT JOIN inventories i ON i.warehouse_id = w.id AND i.variant_id = v.id
WHERE w.is_active = 1 AND v.is_active = 1
ORDER BY w.id, p.id;
GO

-- =====================================================
-- 13. SEED FIXED DATA (SAFE MODE)
-- =====================================================

-- Note: SEED data nên được tách riêng file seed.sql
-- Ở đây chỉ minh họa, không chạy auto-seed để tránh duplicate trong production
-- Xem file DATABASE/seed.sql để có bản seed đầy đủ với IDENTITY_INSERT

PRINT N'DATABASE INITIALIZED SUCCESSFULLY!';
PRINT N'==========================================';
PRINT N'Tables: 26';
PRINT N'Views: 3';
PRINT N'Stored Procedures: 8';
PRINT N'Functions: 1';
PRINT N'==========================================';
PRINT N'Next steps:';
PRINT N'1. Run DATABASE/seed.sql to populate sample data';
PRINT N'2. Update connection strings in your application';
PRINT N'3. Create admin user via application or manually';
GO
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
