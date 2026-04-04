-- =====================================================
-- PROJECT: ECOMMERCE HUIT - Complete Database Schema
-- Database: Microsoft SQL Server 2022
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
-- 1. CLEANUP (Xóa theo thứ tự chuẩn xác từ con đến cha)
-- =====================================================
DROP TABLE IF EXISTS audit_logs;
DROP TABLE IF EXISTS returns;
DROP TABLE IF EXISTS support_tickets;
DROP TABLE IF EXISTS reviews;
DROP TABLE IF EXISTS voucher_usages;
DROP TABLE IF EXISTS order_status_history;
DROP TABLE IF EXISTS order_item_serials;
DROP TABLE IF EXISTS order_items;
DROP TABLE IF EXISTS payments;
DROP TABLE IF EXISTS stock_movements;
DROP TABLE IF EXISTS cart_items;
DROP TABLE IF EXISTS product_images;
DROP TABLE IF EXISTS product_serials;
DROP TABLE IF EXISTS inventories;
DROP TABLE IF EXISTS role_permissions;
DROP TABLE IF EXISTS addresses;
DROP TABLE IF EXISTS wishlists; 
DROP TABLE IF EXISTS orders;
DROP TABLE IF EXISTS carts;
DROP TABLE IF EXISTS vouchers;
DROP TABLE IF EXISTS product_variants;
DROP TABLE IF EXISTS products;
DROP TABLE IF EXISTS brands;
DROP TABLE IF EXISTS categories;
DROP TABLE IF EXISTS warehouses;
DROP TABLE IF EXISTS suppliers;
DROP TABLE IF EXISTS users;
DROP TABLE IF EXISTS permissions;
DROP TABLE IF EXISTS stock_movement_types;
GO

-- =====================================================
-- 2. LOOKUP TABLES (Bảng tra cứu)
-- =====================================================

CREATE TABLE stock_movement_types (
    code VARCHAR(50) PRIMARY KEY,
    name NVARCHAR(100) NOT NULL,
    description NVARCHAR(255) NULL,
    is_increase BIT NOT NULL
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
    label NVARCHAR(50) NOT NULL,
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
    code VARCHAR(50) UNIQUE NOT NULL,
    name NVARCHAR(100) NOT NULL,
    module VARCHAR(50) NOT NULL
);

CREATE INDEX idx_permissions_code ON permissions(code);
GO

CREATE TABLE role_permissions (
    role VARCHAR(20) NOT NULL,
    permission_id INT NOT NULL FOREIGN KEY REFERENCES permissions(id) ON DELETE CASCADE,
    PRIMARY KEY (role, permission_id)
);
GO

-- Seed permissions
INSERT INTO permissions (code, name, module) VALUES
('products.read', N'Xem sản phẩm', 'PRODUCT'),
('products.create', N'Tạo sản phẩm', 'PRODUCT'),
('products.update', N'Sửa sản phẩm', 'PRODUCT'),
('products.delete', N'Xóa sản phẩm', 'PRODUCT'),
('orders.read', N'Xem đơn hàng', 'ORDER'),
('orders.update', N'Cập nhật đơn hàng', 'ORDER'),
('orders.cancel', N'Hủy đơn hàng', 'ORDER'),
('orders.return', N'Xử lý trả hàng', 'ORDER'),
('inventory.read', N'Xem tồn kho', 'INVENTORY'),
('inventory.import', N'Nhập kho', 'INVENTORY'),
('inventory.transfer', N'Chuyển kho', 'INVENTORY'),
('inventory.adjust', N'Điều chỉnh tồn', 'INVENTORY'),
('users.read', N'Xem người dùng', 'USER'),
('users.create', N'Tạo người dùng', 'USER'),
('users.update', N'Sửa người dùng', 'USER'),
('users.delete', N'Xóa người dùng', 'USER'),
('vouchers.read', N'Xem voucher', 'MARKETING'),
('vouchers.create', N'Tạo voucher', 'MARKETING'),
('vouchers.update', N'Sửa voucher', 'MARKETING'),
('reports.read', N'Xem báo cáo', 'REPORT');
GO

-- Seed role_permissions (RBAC)
INSERT INTO role_permissions SELECT 'ADMIN', id FROM permissions;
INSERT INTO role_permissions VALUES
('STAFF', (SELECT id FROM permissions WHERE code='products.read')),
('STAFF', (SELECT id FROM permissions WHERE code='orders.read')),
('STAFF', (SELECT id FROM permissions WHERE code='orders.update')),
('STAFF', (SELECT id FROM permissions WHERE code='inventory.read')),
('STAFF', (SELECT id FROM permissions WHERE code='vouchers.read')),
('STAFF', (SELECT id FROM permissions WHERE code='reports.read')),
('WAREHOUSE', (SELECT id FROM permissions WHERE code='products.read')),
('WAREHOUSE', (SELECT id FROM permissions WHERE code='inventory.read')),
('WAREHOUSE', (SELECT id FROM permissions WHERE code='inventory.import')),
('WAREHOUSE', (SELECT id FROM permissions WHERE code='inventory.transfer'));
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
    specifications NVARCHAR(MAX) NULL,
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
    dimensions NVARCHAR(100) NULL,
    CONSTRAINT CK_Variants_Price CHECK (original_price >= price)
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
-- BỔ SUNG: MODULE WISHLIST
-- =====================================================
CREATE TABLE wishlists (
    id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    product_id INT NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE() NOT NULL,
    CONSTRAINT FK_Wishlist_User FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT FK_Wishlist_Product FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
    CONSTRAINT UQ_Wishlist_User_Product UNIQUE (user_id, product_id)
);

CREATE INDEX idx_wishlists_user ON wishlists(user_id);
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
CREATE INDEX idx_inventories_lowstock ON inventories(quantity_on_hand, reorder_point);
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
    movement_type VARCHAR(50) NOT NULL,
    reference_id INT NULL,
    reference_type VARCHAR(50) NULL,
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
    payment_method VARCHAR(50) NOT NULL,
    payment_status VARCHAR(20) DEFAULT 'PENDING' NOT NULL,
    CONSTRAINT CK_Orders_PayStatus CHECK (payment_status IN ('PENDING','PAID','FAILED','REFUNDED')),
    status VARCHAR(20) DEFAULT 'PENDING' NOT NULL,
    CONSTRAINT CK_Orders_Status CHECK (status IN ('PENDING','CONFIRMED','PROCESSING','SHIPPING','COMPLETED','CANCELLED','RETURNED')),
    shipping_address NVARCHAR(MAX) NOT NULL,
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
    product_name NVARCHAR(255) NOT NULL,
    sku VARCHAR(50) NOT NULL,
    quantity INT NOT NULL CHECK (quantity > 0),
    unit_price DECIMAL(15,2) NOT NULL CHECK (unit_price >= 0),
    cost_price DECIMAL(15,2) NULL,
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
    transaction_id VARCHAR(100) UNIQUE NULL,
    amount DECIMAL(15,2) NOT NULL CHECK (amount >= 0),
    fee DECIMAL(15,2) DEFAULT 0 NOT NULL CHECK (fee >= 0),
    status VARCHAR(20) DEFAULT 'PENDING' NOT NULL,
    CONSTRAINT CK_Payments_Status CHECK (status IN ('PENDING','SUCCESS','FAILED','CANCELLED','REFUNDED')),
    paid_at DATETIME2 NULL,
    webhook_data NVARCHAR(MAX) NULL,
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
    applicable_product_ids NVARCHAR(MAX) NULL,
    applicable_category_ids NVARCHAR(MAX) NULL,
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
    old_values NVARCHAR(MAX) NULL,
    new_values NVARCHAR(MAX) NULL,
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

-- =====================================================
-- 12. VIEWS
-- =====================================================

CREATE OR ALTER VIEW vw_ProductDetails AS
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

CREATE OR ALTER VIEW vw_OrderDetails AS
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

CREATE OR ALTER VIEW vw_InventoryDashboard AS
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
WHERE w.is_active = 1 AND v.is_active = 1;
GO

PRINT N'DATABASE INITIALIZED SUCCESSFULLY!';
PRINT N'==========================================';
PRINT N'Next steps:';
PRINT N'1. Run DATABASE/seed.sql to populate sample data';
PRINT N'2. Update connection strings in your application';
GO