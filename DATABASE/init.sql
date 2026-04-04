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
DROP TABLE IF EXISTS stock_movements;
DROP TABLE IF EXISTS product_variants;
DROP TABLE IF EXISTS products;
DROP TABLE IF EXISTS brands;
DROP TABLE IF EXISTS categories;
DROP TABLE IF EXISTS warehouses;
DROP TABLE IF EXISTS suppliers;
DROP TABLE IF EXISTS addresses;
DROP TABLE IF EXISTS wishlists;
DROP TABLE IF EXISTS users;
DROP TABLE IF EXISTS stock_movement_types;
GO

-- =====================================================
-- 2. LOOKUP TABLES
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
    created_at DATETIME2 DEFAULT GETDATE() NOT NULL,
    updated_at DATETIME2 DEFAULT GETDATE() NOT NULL
);

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
    created_at DATETIME2 DEFAULT GETDATE() NOT NULL,
    updated_at DATETIME2 DEFAULT GETDATE() NOT NULL
);

CREATE TABLE permissions (
    id INT IDENTITY(1,1) PRIMARY KEY,
    code VARCHAR(50) UNIQUE NOT NULL,
    name NVARCHAR(100) NOT NULL,
    module VARCHAR(50) NOT NULL
);

CREATE TABLE role_permissions (
    role VARCHAR(20) NOT NULL,
    permission_id INT NOT NULL FOREIGN KEY REFERENCES permissions(id) ON DELETE CASCADE,
    PRIMARY KEY (role, permission_id)
);

CREATE TABLE wishlists (
    id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL FOREIGN KEY REFERENCES users(id) ON DELETE CASCADE,
    product_id INT NOT NULL, -- Sẽ được cập nhật FK sau khi tạo bảng products
    created_at DATETIME2 DEFAULT GETDATE() NOT NULL
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

INSERT INTO role_permissions SELECT 'ADMIN', id FROM permissions;
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
    sort_order INT DEFAULT 0 NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE() NOT NULL,
    updated_at DATETIME2 DEFAULT GETDATE() NOT NULL
);

CREATE TABLE brands (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(100) NOT NULL UNIQUE,
    logo_url VARCHAR(500) NULL,
    origin NVARCHAR(50) NULL,
    description NVARCHAR(MAX) NULL,
    website VARCHAR(200) NULL,
    created_at DATETIME2 DEFAULT GETDATE() NOT NULL,
    updated_at DATETIME2 DEFAULT GETDATE() NOT NULL
);

CREATE TABLE products (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(255) NOT NULL,
    slug VARCHAR(255) UNIQUE NOT NULL,
    brand_id INT NULL FOREIGN KEY REFERENCES brands(id) ON DELETE SET NULL,
    category_id INT NOT NULL FOREIGN KEY REFERENCES categories(id) ON DELETE NO ACTION,
    short_description NVARCHAR(500) NULL,
    description NVARCHAR(MAX) NULL,
    specifications NVARCHAR(MAX) NULL, -- JSON
    status VARCHAR(20) DEFAULT 'DRAFT' NOT NULL,
    is_featured BIT DEFAULT 0 NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE() NOT NULL,
    updated_at DATETIME2 DEFAULT GETDATE() NOT NULL,
    created_by INT NULL FOREIGN KEY REFERENCES users(id)
);

-- Thêm FK cho wishlist sau khi products được tạo
ALTER TABLE wishlists ADD CONSTRAINT FK_Wishlist_Product FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE;

CREATE TABLE product_variants (
    id INT IDENTITY(1,1) PRIMARY KEY,
    product_id INT NOT NULL FOREIGN KEY REFERENCES products(id) ON DELETE CASCADE,
    sku VARCHAR(50) UNIQUE NOT NULL,
    variant_name NVARCHAR(255) NULL,
    price DECIMAL(15,2) NOT NULL DEFAULT 0,
    original_price DECIMAL(15,2) NULL,
    cost_price DECIMAL(15,2) NULL,
    thumbnail_url VARCHAR(500) NULL,
    display_order INT DEFAULT 0 NOT NULL,
    is_active BIT DEFAULT 1 NOT NULL,
    weight_grams INT NULL,
    dimensions NVARCHAR(100) NULL, -- JSON
    created_at DATETIME2 DEFAULT GETDATE() NOT NULL,
    updated_at DATETIME2 DEFAULT GETDATE() NOT NULL
);

CREATE TABLE product_images (
    id INT IDENTITY(1,1) PRIMARY KEY,
    variant_id INT NOT NULL FOREIGN KEY REFERENCES product_variants(id) ON DELETE CASCADE,
    image_url VARCHAR(500) NOT NULL,
    alt_text NVARCHAR(200) NULL,
    sort_order INT DEFAULT 0 NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE() NOT NULL,
    updated_at DATETIME2 DEFAULT GETDATE() NOT NULL
);
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
    phone VARCHAR(20) NULL,
    manager NVARCHAR(100) NULL,
    is_active BIT DEFAULT 1 NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE() NOT NULL,
    updated_at DATETIME2 DEFAULT GETDATE() NOT NULL
);

CREATE TABLE suppliers (
    id INT IDENTITY(1,1) PRIMARY KEY,
    code VARCHAR(20) UNIQUE NOT NULL,
    name NVARCHAR(200) NOT NULL,
    contact_person NVARCHAR(100) NULL,
    phone VARCHAR(20) NULL,
    email VARCHAR(100) NULL,
    address NVARCHAR(MAX) NULL,
    tax_code VARCHAR(50) NULL,
    is_active BIT DEFAULT 1 NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE() NOT NULL,
    updated_at DATETIME2 DEFAULT GETDATE() NOT NULL
);

CREATE TABLE inventories (
    warehouse_id INT NOT NULL FOREIGN KEY REFERENCES warehouses(id) ON DELETE NO ACTION,
    variant_id INT NOT NULL FOREIGN KEY REFERENCES product_variants(id) ON DELETE NO ACTION,
    quantity_on_hand INT DEFAULT 0 NOT NULL,
    quantity_reserved INT DEFAULT 0 NOT NULL,
    reorder_point INT DEFAULT 10 NOT NULL,
    last_updated DATETIME2 DEFAULT GETDATE() NOT NULL,
    PRIMARY KEY (warehouse_id, variant_id)
);

CREATE TABLE product_serials (
    id INT IDENTITY(1,1) PRIMARY KEY,
    variant_id INT NOT NULL FOREIGN KEY REFERENCES product_variants(id) ON DELETE NO ACTION,
    serial_number VARCHAR(100) UNIQUE NOT NULL,
    warehouse_id INT NOT NULL FOREIGN KEY REFERENCES warehouses(id) ON DELETE NO ACTION,
    status VARCHAR(20) DEFAULT 'AVAILABLE' NOT NULL,
    inbound_date DATETIME2 DEFAULT GETDATE() NOT NULL,
    outbound_date DATETIME2 NULL,
    warranty_expire_date DATE NULL,
    notes NVARCHAR(MAX) NULL,
    created_at DATETIME2 DEFAULT GETDATE() NOT NULL,
    updated_at DATETIME2 DEFAULT GETDATE() NOT NULL
);

CREATE TABLE stock_movements (
    id INT IDENTITY(1,1) PRIMARY KEY,
    warehouse_id INT NOT NULL FOREIGN KEY REFERENCES warehouses(id),
    variant_id INT NOT NULL FOREIGN KEY REFERENCES product_variants(id),
    quantity INT NOT NULL,
    movement_type VARCHAR(50) NOT NULL FOREIGN KEY REFERENCES stock_movement_types(code),
    reference_id INT NULL,
    reference_type VARCHAR(50) NULL,
    supplier_id INT NULL FOREIGN KEY REFERENCES suppliers(id),
    note NVARCHAR(MAX) NULL,
    created_by INT NULL FOREIGN KEY REFERENCES users(id),
    created_at DATETIME2 DEFAULT GETDATE() NOT NULL,
    updated_at DATETIME2 DEFAULT GETDATE() NOT NULL
);
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

CREATE TABLE cart_items (
    id INT IDENTITY(1,1) PRIMARY KEY,
    cart_id INT NOT NULL FOREIGN KEY REFERENCES carts(id) ON DELETE CASCADE,
    variant_id INT NOT NULL FOREIGN KEY REFERENCES product_variants(id) ON DELETE NO ACTION,
    quantity INT NOT NULL CHECK (quantity > 0),
    added_at DATETIME2 DEFAULT GETDATE() NOT NULL,
    updated_at DATETIME2 DEFAULT GETDATE() NOT NULL,
    CONSTRAINT UQ_CartItems_CartVariant UNIQUE (cart_id, variant_id)
);
GO

-- =====================================================
-- 7. MODULE: ORDERS & PAYMENTS
-- =====================================================

CREATE TABLE orders (
    id INT IDENTITY(1,1) PRIMARY KEY,
    code VARCHAR(20) UNIQUE NOT NULL,
    user_id INT NOT NULL FOREIGN KEY REFERENCES users(id),
    order_type VARCHAR(20) DEFAULT 'ONLINE' NOT NULL,
    subtotal DECIMAL(15,2) NOT NULL,
    discount DECIMAL(15,2) DEFAULT 0 NOT NULL,
    shipping_fee DECIMAL(15,2) DEFAULT 0 NOT NULL,
    tax_amount DECIMAL(15,2) DEFAULT 0 NOT NULL,
    total DECIMAL(15,2) NOT NULL,
    payment_method VARCHAR(50) NOT NULL,
    payment_status VARCHAR(20) DEFAULT 'PENDING' NOT NULL,
    status VARCHAR(20) DEFAULT 'PENDING' NOT NULL,
    shipping_address NVARCHAR(MAX) NOT NULL, -- JSON
    note NVARCHAR(MAX) NULL,
    staff_note NVARCHAR(MAX) NULL,
    created_at DATETIME2 DEFAULT GETDATE() NOT NULL,
    updated_at DATETIME2 DEFAULT GETDATE() NOT NULL
);

CREATE TABLE order_items (
    id INT IDENTITY(1,1) PRIMARY KEY,
    order_id INT NOT NULL FOREIGN KEY REFERENCES orders(id) ON DELETE CASCADE,
    variant_id INT NOT NULL FOREIGN KEY REFERENCES product_variants(id),
    product_name NVARCHAR(255) NOT NULL,
    sku VARCHAR(50) NOT NULL,
    quantity INT NOT NULL,
    unit_price DECIMAL(15,2) NOT NULL,
    cost_price DECIMAL(15,2) NULL,
    total_price DECIMAL(15,2) NOT NULL,
    discount_amount DECIMAL(15,2) DEFAULT 0 NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE() NOT NULL,
    updated_at DATETIME2 DEFAULT GETDATE() NOT NULL
);

CREATE TABLE order_item_serials (
    order_item_id INT NOT NULL FOREIGN KEY REFERENCES order_items(id) ON DELETE CASCADE,
    serial_number VARCHAR(100) NOT NULL,
    PRIMARY KEY (order_item_id, serial_number)
);

CREATE TABLE order_status_history (
    id INT IDENTITY(1,1) PRIMARY KEY,
    order_id INT NOT NULL FOREIGN KEY REFERENCES orders(id) ON DELETE CASCADE,
    status VARCHAR(20) NOT NULL,
    changed_by INT NULL FOREIGN KEY REFERENCES users(id),
    note NVARCHAR(MAX) NULL,
    created_at DATETIME2 DEFAULT GETDATE() NOT NULL,
    updated_at DATETIME2 DEFAULT GETDATE() NOT NULL
);

CREATE TABLE payments (
    id INT IDENTITY(1,1) PRIMARY KEY,
    order_id INT UNIQUE NOT NULL FOREIGN KEY REFERENCES orders(id),
    payment_gateway VARCHAR(50) NOT NULL,
    transaction_id VARCHAR(100) UNIQUE NULL,
    amount DECIMAL(15,2) NOT NULL,
    fee DECIMAL(15,2) DEFAULT 0 NOT NULL,
    status VARCHAR(20) DEFAULT 'PENDING' NOT NULL,
    paid_at DATETIME2 NULL,
    webhook_data NVARCHAR(MAX) NULL,
    created_at DATETIME2 DEFAULT GETDATE() NOT NULL,
    updated_at DATETIME2 DEFAULT GETDATE() NOT NULL
);
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
    discount_value DECIMAL(15,2) NOT NULL,
    max_discount_amount DECIMAL(15,2) NULL,
    min_order_value DECIMAL(15,2) DEFAULT 0 NOT NULL,
    applicable_product_ids NVARCHAR(MAX) NULL, -- JSON array
    applicable_category_ids NVARCHAR(MAX) NULL, -- JSON array
    start_date DATETIME2 NOT NULL,
    end_date DATETIME2 NOT NULL,
    usage_limit INT NULL,
    usage_per_user INT DEFAULT 1 NOT NULL,
    usage_count INT DEFAULT 0 NOT NULL,
    is_active BIT DEFAULT 1 NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE() NOT NULL,
    updated_at DATETIME2 DEFAULT GETDATE() NOT NULL
);

CREATE TABLE voucher_usages (
    id INT IDENTITY(1,1) PRIMARY KEY,
    voucher_id INT NOT NULL FOREIGN KEY REFERENCES vouchers(id),
    user_id INT NOT NULL FOREIGN KEY REFERENCES users(id),
    order_id INT NOT NULL FOREIGN KEY REFERENCES orders(id),
    discount_amount DECIMAL(15,2) NOT NULL,
    used_at DATETIME2 DEFAULT GETDATE() NOT NULL,
    updated_at DATETIME2 DEFAULT GETDATE() NOT NULL,
    CONSTRAINT UQ_VoucherUsage_Order UNIQUE (voucher_id, order_id)
);
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
    created_at DATETIME2 DEFAULT GETDATE() NOT NULL,
    updated_at DATETIME2 DEFAULT GETDATE() NOT NULL
);

CREATE TABLE support_tickets (
    id INT IDENTITY(1,1) PRIMARY KEY,
    ticket_number VARCHAR(20) UNIQUE NOT NULL,
    user_id INT NOT NULL FOREIGN KEY REFERENCES users(id),
    subject NVARCHAR(200) NOT NULL,
    priority VARCHAR(20) DEFAULT 'MEDIUM' NOT NULL,
    status VARCHAR(20) DEFAULT 'OPEN' NOT NULL,
    assigned_to INT NULL FOREIGN KEY REFERENCES users(id),
    order_id INT NULL FOREIGN KEY REFERENCES orders(id),
    product_id INT NULL FOREIGN KEY REFERENCES products(id),
    last_message_at DATETIME2 NULL,
    created_at DATETIME2 DEFAULT GETDATE() NOT NULL,
    updated_at DATETIME2 DEFAULT GETDATE() NOT NULL
);

CREATE TABLE returns (
    id INT IDENTITY(1,1) PRIMARY KEY,
    return_number VARCHAR(20) UNIQUE NOT NULL,
    order_id INT NOT NULL FOREIGN KEY REFERENCES orders(id),
    order_item_id INT NOT NULL FOREIGN KEY REFERENCES order_items(id),
    user_id INT NOT NULL FOREIGN KEY REFERENCES users(id),
    reason NVARCHAR(500) NOT NULL,
    status VARCHAR(20) DEFAULT 'REQUESTED' NOT NULL,
    refund_amount DECIMAL(15,2) NULL,
    refund_method VARCHAR(50) NULL,
    created_at DATETIME2 DEFAULT GETDATE() NOT NULL,
    updated_at DATETIME2 DEFAULT GETDATE() NOT NULL,
    resolved_at DATETIME2 NULL
);
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
GO

-- =====================================================
-- 11. TRIGGERS (Update Timestamp)
-- =====================================================

CREATE OR ALTER TRIGGER trg_UpdateProducts
ON products AFTER UPDATE AS BEGIN UPDATE p SET updated_at = GETDATE() FROM products p JOIN inserted i ON p.id = i.id; END;
GO
CREATE OR ALTER TRIGGER trg_UpdateOrders ON orders AFTER UPDATE AS BEGIN UPDATE o SET updated_at = GETDATE() FROM orders o JOIN inserted i ON o.id = i.id; END;
-- Tương tự cho các bảng khác cần tracked updated_at
GO

-- =====================================================
-- 12. STORED PROCEDURES (Unified & Fixed)
-- =====================================================

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
        DECLARE @Quantity INT;
        SELECT @Quantity = COUNT(*) FROM OPENJSON(@ListIMEI);

        INSERT INTO product_serials (variant_id, warehouse_id, serial_number, status, inbound_date, notes)
        SELECT @VariantID, @WarehouseID, value, 'AVAILABLE', GETDATE(), N'Nhập kho lô mới'
        FROM OPENJSON(@ListIMEI);

        MERGE inventories AS target
        USING (SELECT @WarehouseID AS warehouse_id, @VariantID AS variant_id) AS source
        ON (target.warehouse_id = source.warehouse_id AND target.variant_id = source.variant_id)
        WHEN MATCHED THEN
            UPDATE SET quantity_on_hand = quantity_on_hand + @Quantity, last_updated = GETDATE()
        WHEN NOT MATCHED THEN
            INSERT (warehouse_id, variant_id, quantity_on_hand, quantity_reserved)
            VALUES (@WarehouseID, @VariantID, @Quantity, 0);

        INSERT INTO stock_movements (warehouse_id, variant_id, quantity, movement_type, supplier_id, note, created_by)
        VALUES (@WarehouseID, @VariantID, @Quantity, 'PURCHASE', @SupplierID, N'Nhập hàng từ nhà cung cấp', @CreatedBy);

        UPDATE product_variants SET cost_price = @CostPrice, updated_at = GETDATE() WHERE id = @VariantID;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
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
        SET @OrderCode = 'ORD-' + FORMAT(GETDATE(), 'yyyyMMddHHmmss');
        
        DECLARE @Subtotal DECIMAL(15,2);
        SELECT @Subtotal = SUM(CAST(JSON_VALUE(value, '$.quantity') AS INT) * CAST(JSON_VALUE(value, '$.price') AS DECIMAL(15,2)))
        FROM OPENJSON(@OrderItemsJSON);

        INSERT INTO orders (code, user_id, subtotal, total, payment_method, shipping_address, status)
        VALUES (@OrderCode, @UserID, @Subtotal, @Subtotal, @PaymentMethod, @ShippingAddress, 'PENDING');
        SET @OrderID = SCOPE_IDENTITY();

        INSERT INTO order_items (order_id, variant_id, product_name, sku, quantity, unit_price, total_price)
        SELECT @OrderID, CAST(JSON_VALUE(value, '$.variant_id') AS INT), 
               p.name + ISNULL(' ' + v.variant_name, ''), v.sku, 
               CAST(JSON_VALUE(value, '$.quantity') AS INT), CAST(JSON_VALUE(value, '$.price') AS DECIMAL(15,2)),
               CAST(JSON_VALUE(value, '$.quantity') AS INT) * CAST(JSON_VALUE(value, '$.price') AS DECIMAL(15,2))
        FROM OPENJSON(@OrderItemsJSON)
        JOIN product_variants v ON v.id = CAST(JSON_VALUE(value, '$.variant_id') AS INT)
        JOIN products p ON v.product_id = p.id;

        -- Update inventories (Reserve)
        UPDATE inv
        SET quantity_reserved = quantity_reserved + CAST(JSON_VALUE(item.value, '$.quantity') AS INT),
            last_updated = GETDATE()
        FROM inventories inv
        JOIN OPENJSON(@OrderItemsJSON) item ON inv.variant_id = CAST(JSON_VALUE(item.value, '$.variant_id') AS INT)
        WHERE inv.warehouse_id = 1; -- Giả định warehouse 1

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

PRINT N'DATABASE INITIALIZED SUCCESSFULLY!';
