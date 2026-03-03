# Database Schema - ECommerce HUIT

## Tổng quan

Schema được thiết kế cho hệ thống bán hàng điện tử với các module:
- User & Authentication
- Product Catalog
- Inventory & Warehouse (multi-warehouse, serial tracking)
- Orders & Payments
- Marketing (vouchers)
- Support (reviews, tickets, returns)
- Audit & Logging

**Database:** Microsoft SQL Server 2019+
**Character Set:** UTF-8 (NVARCHAR cho tiếng Việt)

---

## Entity Relationship Diagram (ERD) - Mô tả

```
users ──< addresses
  ├──< carts ──< cart_items
  ├──< orders ──< order_items ──< order_item_serials
  │      ├──< order_status_history
  │      ├──< voucher_usages
  │      └──< payments
  ├──< reviews
  ├──< support_tickets
  ├──< returns
  └──< audit_logs

categories (parent_id → categories.id)
  └──< products ──< product_variants ──< product_images
         ├──< inventories (multiple warehouses)
         └──< product_serials

warehouses ──< inventories
         └──< product_serials
         └──< stock_movements

suppliers ──< stock_movements

brands ──< products

vouchers ──< voucher_usages
```

---

## Chi tiết Bảng

### 1. users

| Column | Type | Constraint | Mô tả |
|--------|------|------------|-------|
| id | INT IDENTITY(1,1) | PK | Khóa chính, tự tăng |
| full_name | NVARCHAR(100) | NOT NULL | Họ tên |
| email | VARCHAR(100) | UNIQUE, NOT NULL | Email đăng nhập |
| phone | VARCHAR(20) | UNIQUE | Số điện thoại |
| password_hash | VARCHAR(255) | NOT NULL | Mật khẩu đã hash |
| role | VARCHAR(20) | DEFAULT 'CUSTOMER', CHECK (role IN ('ADMIN','STAFF','WAREHOUSE','CUSTOMER')) | Vai trò |
| status | VARCHAR(20) | DEFAULT 'ACTIVE', CHECK (status IN ('ACTIVE','BANNED')) | Trạng thái tài khoản |
| avatar_url | VARCHAR(500) | NULL | URL ảnh đại diện |
| created_at | DATETIME2 | DEFAULT GETDATE() | Ngày tạo |
| last_login | DATETIME2 | NULL | Lần đăng nhập cuối |

**Indexes:**
- `UQ_users_email` (email)
- `UQ_users_phone` (phone)
- `idx_users_role` (role)
- `idx_users_status` (status)

---

### 2. addresses

| Column | Type | Constraint | Mô tả |
|--------|------|------------|-------|
| id | INT IDENTITY(1,1) | PK | |
| user_id | INT | FK → users(id), NOT NULL | Chủ sở hữu |
| label | NVARCHAR(50) | NOT NULL | 'Nhà', 'Văn phòng', ... |
| receiver_name | NVARCHAR(100) | NOT NULL | Người nhận |
| receiver_phone | VARCHAR(20) | NOT NULL | SĐT người nhận |
| province | NVARCHAR(100) | NOT NULL | Tỉnh/Thành |
| district | NVARCHAR(100) | NOT NULL | Quận/Huyện |
| ward | NVARCHAR(100) | NOT NULL | Phường/Xã |
| street_address | NVARCHAR(255) | NOT NULL | Địa chỉ chi tiết |
| is_default | BIT | DEFAULT 0 | Địa chỉ mặc định |
| created_at | DATETIME2 | DEFAULT GETDATE() | |

**Indexes:**
- `idx_addresses_user` (user_id)

---

### 3. categories

| Column | Type | Constraint | Mô tả |
|--------|------|------------|-------|
| id | INT IDENTITY(1,1) | PK | |
| parent_id | INT | FK → categories(id), NULLABLE | Category cha (NULL = root) |
| name | NVARCHAR(100) | NOT NULL | Tên |
| slug | VARCHAR(100) | UNIQUE, NOT NULL | SEO URL |
| description | NVARCHAR(MAX) | NULL | Mô tả |
| is_active | BIT | DEFAULT 1 | Kích hoạt |
| sort_order | INT | DEFAULT 0 | Thứ tự hiển thị |

**Indexes:**
- `idx_categories_parent` (parent_id)
- `idx_categories_active` (is_active)

---

### 4. brands

| Column | Type | Constraint | Mô tả |
|--------|------|------------|-------|
| id | INT IDENTITY(1,1) | PK | |
| name | NVARCHAR(100) | NOT NULL, UNIQUE | Tên thương hiệu |
| logo_url | VARCHAR(500) | NULL | URL logo |
| origin | NVARCHAR(50) | NULL | Xuất xứ |
| description | NVARCHAR(MAX) | NULL | Mô tả |
| website | VARCHAR(200) | NULL | Website |

**Indexes:**
- `idx_brands_name` (name)

---

### 5. products

| Column | Type | Constraint | Mô tả |
|--------|------|------------|-------|
| id | INT IDENTITY(1,1) | PK | |
| name | NVARCHAR(255) | NOT NULL | Tên sản phẩm |
| slug | VARCHAR(255) | UNIQUE, NOT NULL | SEO |
| brand_id | INT | FK → brands(id) | Thương hiệu |
| category_id | INT | FK → categories(id) | Danh mục |
| short_description | NVARCHAR(500) | NULL | Mô tả ngắn |
| description | NVARCHAR(MAX) | NULL | Mô tả chi tiết HTML |
| specifications | NVARCHAR(MAX) | CHECK (ISJSON(specifications)=1) | JSON specs |
| meta_title | NVARCHAR(200) | NULL | SEO title |
| meta_description | NVARCHAR(500) | NULL | SEO desc |
| status | VARCHAR(20) | DEFAULT 'DRAFT', CHECK (status IN ('DRAFT','ACTIVE','HIDDEN')) | Trạng thái |
| is_featured | BIT | DEFAULT 0 | Nổi bật |
| created_at | DATETIME2 | DEFAULT GETDATE() | |
| updated_at | DATETIME2 | DEFAULT GETDATE() | |
| created_by | INT | FK → users(id) | Người tạo |

**Indexes:**
- `idx_products_slug` (slug)
- `idx_products_brand` (brand_id)
- `idx_products_category` (category_id)
- `idx_products_status` (status)
- `idx_products_featured` (is_featured)

**Trigger:** `trg_products_updatetimestamp` - tự động update `updated_at`

---

### 6. product_variants

| Column | Type | Constraint | Mô tả |
|--------|------|------------|-------|
| id | INT IDENTITY(1,1) | PK | |
| product_id | INT | FK → products(id) ON DELETE CASCADE, NOT NULL | |
| sku | VARCHAR(50) | UNIQUE, NOT NULL | Mã SKU |
| variant_name | NVARCHAR(255) | NULL | Tên biến thể (ví dụ: '256GB - Black') |
| price | DECIMAL(15,2) | NOT NULL CHECK (price >= 0) | Giá bán |
| original_price | DECIMAL(15,2) | NULL | Giá gốc (trước khuyến mãi) |
| cost_price | DECIMAL(15,2) | NULL | Giá vốn |
| thumbnail_url | VARCHAR(500) | NULL | Ảnh đại diện |
| display_order | INT | DEFAULT 0 | Thứ tự hiển thị |
| is_active | BIT | DEFAULT 1 | Kích hoạt |
| weight_grams | INT | NULL | Trọng lượng (g) |
| dimensions | NVARCHAR(100) | NULL | JSON: {"length":...,"width":...,"height":...} |

**Indexes:**
- `idx_variants_sku` (sku)
- `idx_variants_product` (product_id)
- `idx_variants_active` (is_active)

---

### 7. product_images

| Column | Type | Constraint | Mô tả |
|--------|------|------------|-------|
| id | INT IDENTITY(1,1) | PK | |
| variant_id | INT | FK → product_variants(id) ON DELETE CASCADE, NOT NULL | |
| image_url | VARCHAR(500) | NOT NULL | URL ảnh |
| alt_text | NVARCHAR(200) | NULL | Mô tả ảnh |
| sort_order | INT | DEFAULT 0 | Thứ tự |

**Indexes:**
- `idx_images_variant` (variant_id)

---

### 8. warehouses

| Column | Type | Constraint | Mô tả |
|--------|------|------------|-------|
| id | INT IDENTITY(1,1) | PK | |
| code | VARCHAR(20) | UNIQUE, NOT NULL | Mã kho (WH001) |
| name | NVARCHAR(100) | NOT NULL | Tên kho |
| address | NVARCHAR(255) | NULL | Địa chỉ |
| type | VARCHAR(20) | DEFAULT 'PHYSICAL', CHECK (type IN ('PHYSICAL','VIRTUAL')) | Loại kho |
| phone | VARCHAR(20) | NULL | SĐT liên hệ |
| manager | NVARCHAR(100) | NULL | Người quản lý |
| is_active | BIT | DEFAULT 1 | Kích hoạt |

**Indexes:**
- `idx_warehouses_code` (code)
- `idx_warehouses_active` (is_active)

---

### 9. inventories

| Column | Type | Constraint | Mô tả |
|--------|------|------------|-------|
| warehouse_id | INT | FK → warehouses(id), NOT NULL | |
| variant_id | INT | FK → product_variants(id), NOT NULL | |
| quantity_on_hand | INT | DEFAULT 0 CHECK (quantity_on_hand >= 0) | Tồn thực tế |
| quantity_reserved | INT | DEFAULT 0 CHECK (quantity_reserved >= 0) | Đã đặt trước |
| reorder_point | INT | DEFAULT 10 | Mức đặt hàng lại |
| last_updated | DATETIME2 | DEFAULT GETDATE() | |
| PRIMARY KEY | (warehouse_id, variant_id) | Composite PK | |

**Indexes:**
- `idx_inventories_variant` (variant_id)
- `idx_inventories_lowstock` (quantity_on_hand, reorder_point) - để cảnh báo hết hàng

---

### 10. product_serials

Quản lý serial number/IMEI cho từng variant.

| Column | Type | Constraint | Mô tả |
|--------|------|------------|-------|
| id | INT IDENTITY(1,1) | PK | |
| variant_id | INT | FK → product_variants(id), NOT NULL | |
| serial_number | VARCHAR(100) | UNIQUE, NOT NULL | IMEI/Serial |
| warehouse_id | INT | FK → warehouses(id) | Kho hiện tại |
| status | VARCHAR(20) | DEFAULT 'AVAILABLE', CHECK (status IN ('AVAILABLE','RESERVED','SOLD','DEFECTIVE','TRANSFERRING','RETURNED')) | Trạng thái |
| inbound_date | DATETIME2 | DEFAULT GETDATE() | Ngày nhập kho |
| outbound_date | DATETIME2 | NULL | Ngày xuất kho |
| warranty_expire_date | DATE | NULL | Ngày hết hạn bảo hành |
| notes | NVARCHAR(MAX) | NULL | Ghi chú |

**Indexes:**
- `idx_serials_number` (serial_number) - RẤT quan trọng cho tra cứu IMEI
- `idx_serials_variant` (variant_id)
- `idx_serials_warehouse` (warehouse_id)
- `idx_serials_status` (status)

---

### 11. stock_movements

Nhật ký nhập/xuất kho.

| Column | Type | Constraint | Mô tả |
|--------|------|------------|-------|
| id | INT IDENTITY(1,1) | PK | |
| warehouse_id | INT | FK → warehouses(id), NOT NULL | |
| variant_id | INT | FK → product_variants(id), NOT NULL | |
| quantity | INT | NOT NULL CHECK (quantity != 0) | Số lượng (dương = nhập, âm = xuất) |
| movement_type | VARCHAR(50) | NOT NULL | Loại: PURCHASE, SALE, RETURN, TRANSFER, ADJUSTMENT, ... |
| reference_id | INT | NULL | ID tham chiếu (order_id, purchase_id, ...) |
| reference_type | VARCHAR(50) | NULL | Loại tham chiếu ('ORDER', 'PURCHASE_ORDER', 'TRANSFER') |
| supplier_id | INT | FK → suppliers(id), NULLABLE | Nhà cung cấp (nếu là nhập hàng) |
| note | NVARCHAR(MAX) | NULL | Ghi chú |
| created_by | INT | FK → users(id) | Người tạo |
| created_at | DATETIME2 | DEFAULT GETDATE() | |

**Indexes:**
- `idx_movements_warehouse` (warehouse_id)
- `idx_movements_variant` (variant_id)
- `idx_movements_created` (created_at)
- `idx_movements_ref` (reference_id, reference_type)

---

### 12. suppliers

| Column | Type | Constraint | Mô tả |
|--------|------|------------|-------|
| id | INT IDENTITY(1,1) | PK | |
| code | VARCHAR(20) | UNIQUE, NOT NULL | Mã nhà cung cấp |
| name | NVARCHAR(200) | NOT NULL | |
| contact_person | NVARCHAR(100) | NULL | Người liên hệ |
| phone | VARCHAR(20) | NULL | |
| email | VARCHAR(100) | NULL | |
| address | NVARCHAR(MAX) | NULL | Địa chỉ |
| tax_code | VARCHAR(50) | NULL | Mã số thuế |
| bank_account | NVARCHAR(100) | NULL | STK ngân hàng |
| is_active | BIT | DEFAULT 1 | |

---

### 13. orders

| Column | Type | Constraint | Mô tả |
|--------|------|------------|-------|
| id | INT IDENTITY(1,1) | PK | |
| code | VARCHAR(20) | UNIQUE, NOT NULL | Mã đơn hàng (ORD-20260214001) |
| user_id | INT | FK → users(id), NOT NULL | |
| order_type | VARCHAR(20) | DEFAULT 'ONLINE', CHECK (order_type IN ('ONLINE','POS','B2B')) | Loại đơn |
| subtotal | DECIMAL(15,2) | NOT NULL CHECK (subtotal >= 0) | Tổng tiền hàng |
| discount | DECIMAL(15,2) | DEFAULT 0 | Giảm giá từ voucher |
| shipping_fee | DECIMAL(15,2) | DEFAULT 0 | Phí vận chuyển |
| tax_amount | DECIMAL(15,2) | DEFAULT 0 | Thuế (VAT) |
| total | DECIMAL(15,2) | NOT NULL CHECK (total >= 0) | Tổng thanh toán |
| payment_method | VARCHAR(50) | NOT NULL | 'CASH','MOMO','VNPAY','BANKING','COD' |
| payment_status | VARCHAR(20) | DEFAULT 'PENDING', CHECK (payment_status IN ('PENDING','PAID','FAILED','REFUNDED')) | |
| status | VARCHAR(20) | DEFAULT 'PENDING', CHECK (status IN ('PENDING','CONFIRMED','PROCESSING','SHIPPING','COMPLETED','CANCELLED','RETURNED')) | |
| shipping_address | NVARCHAR(MAX) | CHECK (ISJSON(shipping_address)=1) | JSON address |
| note | NVARCHAR(MAX) | NULL | Ghi chú khách hàng |
| staff_note | NVARCHAR(MAX) | NULL | Ghi chú nội bộ |
| created_at | DATETIME2 | DEFAULT GETDATE() | |
| updated_at | DATETIME2 | DEFAULT GETDATE() | |

**Indexes:**
- `idx_orders_code` (code)
- `idx_orders_user` (user_id)
- `idx_orders_created` (created_at)
- `idx_orders_status` (status)
- `idx_orders_payment_status` (payment_status)

**Trigger:** `trg_orders_updatetimestamp`

---

### 14. order_items

| Column | Type | Constraint | Mô tả |
|--------|------|------------|-------|
| id | INT IDENTITY(1,1) | PK | |
| order_id | INT | FK → orders(id) ON DELETE CASCADE, NOT NULL | |
| variant_id | INT | FK → product_variants(id), NOT NULL | |
| product_name | NVARCHAR(255) | NOT NULL | Snapshot tên SP (không đổi) |
| sku | VARCHAR(50) | NOT NULL | Snapshot SKU |
| quantity | INT | NOT NULL CHECK (quantity > 0) | |
| unit_price | DECIMAL(15,2) | NOT NULL | Snapshot giá bán lúc đặt |
| cost_price | DECIMAL(15,2) | NULL | Snapshot giá vốn |
| total_price | DECIMAL(15,2) | NOT NULL | quantity * unit_price |
| discount_amount | DECIMAL(15,2) | DEFAULT 0 | Giảm giá trên line item |

**Indexes:**
- `idx_orderitems_order` (order_id)
- `idx_orderitems_variant` (variant_id)

---

### 15. order_item_serials

Gắn serial number với từng order item (quan trọng với điện tử).

| Column | Type | Constraint | Mô tả |
|--------|------|------------|-------|
| order_item_id | INT | FK → order_items(id) ON DELETE CASCADE, NOT NULL | |
| serial_number | VARCHAR(100) | NOT NULL | Serial/IMEI |
| PRIMARY KEY | (order_item_id, serial_number) | Composite | |

**Indexes:**
- `idx_ois_serial` (serial_number) - để tra cứu nhanh

---

### 16. order_status_history

Theo dõi lịch sử thay đổi trạng thái đơn hàng.

| Column | Type | Constraint | Mô tả |
|--------|------|------------|-------|
| id | INT IDENTITY(1,1) | PK | |
| order_id | INT | FK → orders(id) ON DELETE CASCADE, NOT NULL | |
| status | VARCHAR(20) | NOT NULL | Trạng thái mới |
| changed_by | INT | FK → users(id), NULL | Người thay đổi (NULL = hệ thống) |
| note | NVARCHAR(MAX) | NULL | Ghi chú thay đổi |
| created_at | DATETIME2 | DEFAULT GETDATE() | |

**Indexes:**
- `idx_osh_order` (order_id)
- `idx_osh_created` (created_at)

---

### 17. vouchers

| Column | Type | Constraint | Mô tả |
|--------|------|------------|-------|
| id | INT IDENTITY(1,1) | PK | |
| code | VARCHAR(20) | UNIQUE, NOT NULL | Mã voucher (HUIT2026) |
| name | NVARCHAR(255) | NOT NULL | Tên voucher |
| description | NVARCHAR(500) | NULL | Mô tả chi tiết |
| discount_type | VARCHAR(10) | CHECK (discount_type IN ('PERCENT','FIXED')) | |
| discount_value | DECIMAL(15,2) | NOT NULL CHECK (discount_value > 0) | % or số tiền |
| max_discount_amount | DECIMAL(15,2) | NULL | Giảm tối đa (cho percent) |
| min_order_value | DECIMAL(15,2) | DEFAULT 0 | Giá trị đơn tối thiểu |
| applicable_product_ids | NVARCHAR(MAX) | NULL | JSON array product IDs (NULL = all) |
| applicable_category_ids | NVARCHAR(MAX) | NULL | JSON array category IDs |
| start_date | DATETIME2 | NOT NULL | |
| end_date | DATETIME2 | NOT NULL | |
| usage_limit | INT | NULL | Tổng số lần dùng được (NULL = unlimited) |
| usage_per_user | INT | DEFAULT 1 | Số lần mỗi user được dùng |
| is_active | BIT | DEFAULT 1 | |

**Indexes:**
- `idx_vouchers_code` (code)
- `idx_vouchers_dates` (start_date, end_date, is_active)

---

### 18. voucher_usages

| Column | Type | Constraint | Mô tả |
|--------|------|------------|-------|
| id | INT IDENTITY(1,1) | PK | |
| voucher_id | INT | FK → vouchers(id), NOT NULL | |
| user_id | INT | FK → users(id), NOT NULL | |
| order_id | INT | FK → orders(id), NOT NULL | |
| discount_amount | DECIMAL(15,2) | NOT NULL | Số tiền giảm thực tế |
| used_at | DATETIME2 | DEFAULT GETDATE() | |
| UNIQUE (voucher_id, order_id) | | | |

**Indexes:**
- `idx_vusage_voucher` (voucher_id)
- `idx_vusage_user` (user_id)

---

### 19. reviews

| Column | Type | Constraint | Mô tả |
|--------|------|------------|-------|
| id | INT IDENTITY(1,1) | PK | |
| user_id | INT | FK → users(id), NOT NULL | |
| product_id | INT | FK → products(id), NOT NULL | |
| variant_id | INT | FK → product_variants(id), NULL | Có thể review cho variant cụ thể |
| rating | INT | CHECK (rating BETWEEN 1 AND 5) | |
| title | NVARCHAR(200) | NULL | Tiêu đề |
| content | NVARCHAR(MAX) | NOT NULL | Nội dung |
| is_verified_purchase | BIT | DEFAULT 0 | Đã mua hàng thật? |
| is_approved | BIT | DEFAULT 0 | Kiểm duyệt |
| created_at | DATETIME2 | DEFAULT GETDATE() | |

**Indexes:**
- `idx_reviews_product` (product_id)
- `idx_reviews_user` (user_id)
- `idx_reviews_rating` (rating)

---

### 20. support_tickets

| Column | Type | Constraint | Mô tả |
|--------|------|------------|-------|
| id | INT IDENTITY(1,1) | PK | |
| ticket_number | VARCHAR(20) | UNIQUE, NOT NULL | TICK-20260214001 |
| user_id | INT | FK → users(id), NOT NULL | |
| subject | NVARCHAR(200) | NOT NULL | |
| priority | VARCHAR(20) | DEFAULT 'MEDIUM', CHECK (priority IN ('LOW','MEDIUM','HIGH','URGENT')) | |
| status | VARCHAR(20) | DEFAULT 'OPEN', CHECK (status IN ('OPEN','IN_PROGRESS','WAITING_CUSTOMER','RESOLVED','CLOSED')) | |
| assigned_to | INT | FK → users(id), NULL | Staff được gán |
| order_id | INT | FK → orders(id), NULL | Liên kết đơn hàng |
| product_id | INT | FK → products(id), NULL | Sản phẩm liên quan |
| last_message_at | DATETIME2 | NULL | Lần message cuối |
| created_at | DATETIME2 | DEFAULT GETDATE() | |

**Indexes:**
- `idx_tickets_user` (user_id)
- `idx_tickets_status` (status)
- `idx_tickets_priority` (priority)
- `idx_tickets_assigned` (assigned_to)

---

### 21. returns

| Column | Type | Constraint | Mô tả |
|--------|------|------------|-------|
| id | INT IDENTITY(1,1) | PK | |
| return_number | VARCHAR(20) | UNIQUE, NOT NULL | RET-20260214001 |
| order_id | INT | FK → orders(id), NOT NULL | |
| order_item_id | INT | FK → order_items(id), NOT NULL | |
| user_id | INT | FK → users(id), NOT NULL | |
| reason | NVARCHAR(500) | NOT NULL | Lý do trả |
| status | VARCHAR(20) | DEFAULT 'REQUESTED', CHECK (status IN ('REQUESTED','APPROVED','REJECTED','RECEIVED','REFUNDED','COMPLETED')) | |
| refund_amount | DECIMAL(15,2) | NULL | Số tiền hoàn lại |
| refund_method | VARCHAR(50) | NULL | 'BANK','MOMO','STORE_CREDIT' |
| created_at | DATETIME2 | DEFAULT GETDATE() | |
| resolved_at | DATETIME2 | NULL | |

**Indexes:**
- `idx_returns_order` (order_id)
- `idx_returns_user` (user_id)
- `idx_returns_status` (status)

---

### 22. payments

| Column | Type | Constraint | Mô tả |
|--------|------|------------|-------|
| id | INT IDENTITY(1,1) | PK | |
| order_id | INT | FK → orders(id), UNIQUE, NOT NULL | 1 order = 1 payment (có thể split nhưng để đơn giản) |
| payment_gateway | VARCHAR(50) | NOT NULL | 'MOMO','VNPAY','BANKING','CASH' |
| transaction_id | VARCHAR(100) | UNIQUE | Transaction ID từ gateway |
| amount | DECIMAL(15,2) | NOT NULL | |
| fee | DECIMAL(15,2) | DEFAULT 0 | Phí giao dịch |
| status | VARCHAR(20) | DEFAULT 'PENDING', CHECK (status IN ('PENDING','SUCCESS','FAILED','CANCELLED','REFUNDED')) | |
| paid_at | DATETIME2 | NULL | Thời gian thanh toán |
| webhook_data | NVARCHAR(MAX) | NULL | JSON chứa full response từ gateway (cho audit) |
| created_at | DATETIME2 | DEFAULT GETDATE() | |

**Indexes:**
- `idx_payments_order` (order_id)
- `idx_payments_transaction` (transaction_id)
- `idx_payments_status` (status)

---

### 23. carts

| Column | Type | Constraint | Mô tả |
|--------|------|------------|-------|
| id | INT IDENTITY(1,1) | PK | |
| user_id | INT | FK → users(id), UNIQUE, NOT NULL | Mỗi user 1 cart |
| coupon_code | VARCHAR(20) | NULL | Voucher đang apply |
| created_at | DATETIME2 | DEFAULT GETDATE() | |
| updated_at | DATETIME2 | DEFAULT GETDATE() | |

**Indexes:**
- `idx_carts_user` (user_id)

---

### 24. cart_items

| Column | Type | Constraint | Mô tả |
|--------|------|------------|-------|
| id | INT IDENTITY(1,1) | PK | |
| cart_id | INT | FK → carts(id) ON DELETE CASCADE, NOT NULL | |
| variant_id | INT | FK → product_variants(id), NOT NULL | |
| quantity | INT | NOT NULL CHECK (quantity > 0) | |
| added_at | DATETIME2 | DEFAULT GETDATE() | |

**Indexes:**
- `idx_cartitems_cart` (cart_id)
- `idx_cartitems_variant` (variant_id)
- UNIQUE (cart_id, variant_id)

---

### 25. permissions

RBAC chi tiết.

| Column | Type | Constraint | Mô tả |
|--------|------|------------|-------|
| id | INT IDENTITY(1,1) | PK | |
| code | VARCHAR(50) | UNIQUE, NOT NULL | 'products.read', 'orders.update', ... |
| name | NVARCHAR(100) | NOT NULL | Mô tả quyền |
| module | VARCHAR(50) | NOT NULL | 'PRODUCT','ORDER','INVENTORY',... |

---

### 26. role_permissions

Mapping giữa role và permission.

| Column | Type | Constraint | Mô tả |
|--------|------|------------|-------|
| role | VARCHAR(20) | NOT NULL, FK → users.role (không ràng buộc FK) | |
| permission_id | INT | FK → permissions(id), NOT NULL | |
| PRIMARY KEY | (role, permission_id) | | |

**Ghi chú:** Vì role là string trong users, ta không tạo FK. Giữ mapping ở đây để dễ quản lý.

---

### 27. audit_logs

Ghi nhật ký tất cả thay đổi dữ liệu quan trọng (audit trail).

| Column | Type | Constraint | Mô tả |
|--------|------|------------|-------|
| id | BIGINT IDENTITY(1,1) | PK | |
| table_name | VARCHAR(50) | NOT NULL | Tên bảng bị thay đổi |
| record_id | INT | NOT NULL | ID của bản ghi |
| operation | VARCHAR(10) | CHECK (operation IN ('INSERT','UPDATE','DELETE')) | |
| old_values | NVARCHAR(MAX) | NULL | JSON chứa giá trị cũ (cho UPDATE/DELETE) |
| new_values | NVARCHAR(MAX) | NULL | JSON chứa giá trị mới (cho INSERT/UPDATE) |
| changed_by | INT | FK → users(id), NULL | Người thực hiện |
| changed_at | DATETIME2 | DEFAULT GETDATE() | |
| ip_address | VARCHAR(45) | NULL | IP client |

**Indexes:**
- `idx_audit_table_record` (table_name, record_id)
- `idx_audit_changed` (changed_at)
- `idx_audit_user` (changed_by)

---

## Stored Procedures

### 1. `sp_ImportStock`

Nhập hàng vào kho với serial tracking.

**Parameters:**
- `@WarehouseID` INT
- `@VariantID` INT
- `@CostPrice` DECIMAL(15,2)
- `@SupplierID` INT (nullable)
- `@ListIMEI` NVARCHAR(MAX) - JSON array `["IMEI001","IMEI002"]`

**Actions:**
1. Parse JSON, đếm số lượng
2. Insert vào `product_serials` (status = AVAILABLE)
3. UPSERT `inventories` (quantity_on_hand += @Quantity)
4. Insert `stock_movements` (type = 'PURCHASE')
5. Update `product_variants.cost_price`

---

### 2. `sp_CreateOrder`

Tạo đơn hàng mới.

**Parameters:**
- `@UserID` INT
- `@ShippingAddress` NVARCHAR(MAX) - JSON
- `@PaymentMethod` VARCHAR(50)
- `@OrderItemsJSON` NVARCHAR(MAX) - Array of {variant_id, quantity}

**Actions:**
1. Validate tồn kho (check quantity_on_hand - quantity_reserved >= quantity)
2. Tạo order code: `ORD-{yyyyMMddHHmmss}`
3. Insert order
4. Insert order_items
5. Update inventories: quantity_reserved += quantity
6. Insert stock_movements: type = 'SALE_RESERVED'
7. Return OrderID, OrderCode

**Đề xuất:** nên đặt trong transaction, và lock row inventories để tránh race condition.

---

### 3. `sp_ConfirmOrder`

Xác nhận đơn hàng (chuyển từ PENDING sang CONFIRMED) và bắt đầu xử lý.

**Parameters:**
- `@OrderID` INT
- `@StaffID` INT

**Actions:**
1. Kiểm tra order status == PENDING
2. Cập nhật status = CONFIRMED
3. Tạo record trong `order_status_history`
4. Có thể trigger gửi email thông báo

---

### 4. `sp_ShipOrder`

Xuất kho & đóng gói.

**Parameters:**
- `@OrderID` INT
- `@WarehouseID` INT
- `@StaffID` INT

**Actions:**
1. Lấy danh sách serial numbers từ order_items (qua order_item_serials)
2. Cập nhật `product_serials.status` = SOLD (WHERE serial IN (...) AND status = 'RESERVED')
   - Nếu có serial không ở trạng thái RESERVED → lỗi (race condition)
3. Cập nhật `inventories.quantity_reserved` -= quantity đã bán
4. Cập nhật order.status = SHIPPING
5. Ghi `stock_movements` với type='SALE_SHIP'
6. Insert `order_status_history`

---

### 5. `sp_CompleteOrder`

Hoàn tất đơn hàng.

**Parameters:**
- `@OrderID` INT

**Actions:**
1. Kiểm tra status == SHIPPING
2. Cập nhật status = COMPLETED
3. Cập nhật `product_serials.warranty_expire_date` = DATEADD(MONTH, 12, GETDATE()) nếu cần
4. Insert `order_status_history`
5. Ghi nhận doanh thu (có thể insert vào bảng `daily_revenue` nếu có)

---

### 6. `sp_CancelOrder`

Hủy đơn hàng.

**Parameters:**
- `@OrderID` INT
- `@Reason` NVARCHAR(MAX)

**Actions:**
1. Kiểm tra status có thể hủy (PENDING, CONFIRMED, PROCESSING)
2. Hoàn trả tồn kho:
   - `inventories.quantity_reserved` -= quantity
3. Cập nhật serial nếu đã RESERVED → trả về AVAILABLE
4. Cập nhật order.status = CANCELLED, note = reason
5. Nếu đã thanh toán → tạo refund request
6. Insert `order_status_history`

---

### 7. `sp_ProcessReturn`

Xử lý trả hàng.

**Parameters:**
- `@ReturnID` INT
- `@Action` VARCHAR(20) - 'APPROVE' or 'REJECT'

**Actions:**
1. Kiểm tra return.status == REQUESTED
2. Nếu APPROVE:
   - Lấy order_item, variant, current warehouse
   - Cập nhật `product_serials.status` = RETURNED (hoặc AVAILABLE nếu còn tốt)
   - Cập nhật `inventories.quantity_on_hand` += 1 (từng serial)
   - Tạo `stock_movements` type='RETURN'
   - Refund qua payment gateway (có thể bước ngoài)
   - Update return.status = REFUNDED/COMPLETED
3. Nếu REJECT: update status = REJECTED

---

### 8. `sp_GetLowStockReport`

Báo cáo hàng sắp hết.

**Parameters:**
- `@WarehouseID` INT (nullable)

**Actions:**
Trả về danh sách variants có `quantity_on_hand <= reorder_point`, kèm thông tin product, warehouse.

---

### 9. `sp_GetRevenueReport`

Báo cáo doanh thu theo ngày/tháng.

**Parameters:**
- `@FromDate` DATE
- `@ToDate` DATE
- `@GroupBy` VARCHAR(20) - 'DAY', 'MONTH', 'YEAR'

**Returns:**
- Ngày/Tháng/Năm
- Tổng orders
- Tổng revenue (total của completed orders)
- Tổng discount
- Tổng shipping fee

---

## Views

### 1. `vw_ProductDetails`

Join products, brands, categories, variants (with inventory summary).

```sql
CREATE VIEW vw_ProductDetails AS
SELECT
    p.id, p.name, p.slug, p.description, p.specifications,
    b.name as brand_name, b.origin,
    c.name as category_name,
    v.id as variant_id, v.sku, v.variant_name, v.price, v.original_price, v.thumbnail_url,
    SUM(i.quantity_on_hand) as total_stock
FROM products p
JOIN brands b ON p.brand_id = b.id
JOIN categories c ON p.category_id = c.id
LEFT JOIN product_variants v ON v.product_id = p.id AND v.is_active = 1
LEFT JOIN inventories i ON i.variant_id = v.id
GROUP BY p.id, p.name, p.slug, p.description, p.specifications,
         b.name, b.origin, c.name,
         v.id, v.sku, v.variant_name, v.price, v.original_price, v.thumbnail_url;
```

---

### 2. `vw_OrderDetails`

Chi tiết order với customer info, items, serials.

```sql
CREATE VIEW vw_OrderDetails AS
SELECT
    o.id, o.code, o.total, o.payment_status, o.status, o.created_at,
    u.full_name as customer_name, u.email, u.phone,
    JSON_VALUE(o.shipping_address, '$.city') as city,
    JSON_VALUE(o.shipping_address, '$.street') as street,
    oi.id as item_id, oi.product_name, oi.sku, oi.quantity, oi.unit_price, oi.total_price,
    ois.serial_number
FROM orders o
JOIN users u ON o.user_id = u.id
LEFT JOIN order_items oi ON oi.order_id = o.id
LEFT JOIN order_item_serials ois ON ois.order_item_id = oi.id;
```

---

### 3. `vw_InventoryDashboard`

Dashboard tồn kho theo warehouse.

```sql
CREATE VIEW vw_InventoryDashboard AS
SELECT
    w.id as warehouse_id, w.name as warehouse_name, w.code as warehouse_code,
    v.id as variant_id, v.sku, p.name as product_name, v.variant_name,
    i.quantity_on_hand, i.quantity_reserved,
    (i.quantity_on_hand - i.quantity_reserved) as available_quantity,
    ps.status as default_serial_status, COUNT(ps.id) as serial_count
FROM warehouses w
CROSS JOIN product_variants v
JOIN products p ON v.product_id = p.id
LEFT JOIN inventories i ON i.warehouse_id = w.id AND i.variant_id = v.id
LEFT JOIN product_serials ps ON ps.warehouse_id = w.id AND ps.variant_id = v.id
GROUP BY w.id, w.name, w.code, v.id, v.sku, p.name, v.variant_name,
         i.quantity_on_hand, i.quantity_reserved, ps.status;
```

---

## Functions

### 1. `ufn_CalculateDiscount`

Tính số tiền giảm giá cho một đơn hàng.

```sql
CREATE FUNCTION ufn_CalculateDiscount(
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
    WHERE id = @VoucherID AND is_active = 1 AND GETDATE() BETWEEN start_date AND end_date;

    IF @DiscountType IS NOT NULL AND @Subtotal >= @MinOrder
    BEGIN
        IF @DiscountType = 'PERCENT'
            SET @Discount = @Subtotal * (@DiscountValue / 100.0);
        ELSE IF @DiscountType = 'FIXED'
            SET @Discount = @DiscountValue;

        IF @Discount > ISNULL(@MaxDiscount, @Discount)
            SET @Discount = @MaxDiscount;
    END

    RETURN @Discount;
END;
```

---

## Indexing Strategy

- **PKs** trên tất cả bảng
- **Unique indexes** cho business keys (email, phone, sku, serial_number, order_code)
- **Foreign key indexes** tự động tạo (SQL Server không tự tạo INDEX cho FK, nên cần tạo thủ công)
- **Covering indexes** cho query thường xuyên:
  - `idx_orders_user_created` (user_id, created_at DESC) - lịch sử đơn hàng
  - `idx_inventories_variant_stock` (variant_id, quantity_on_hand) - check stock
  - `idx_products_category_status` (category_id, status) - browse products
- **Filtered indexes**:
  - `CREATE INDEX idx_serials_available ON product_serials(serial_number) WHERE status = 'AVAILABLE'`
  - `CREATE INDEX idx_orders_pending ON orders(id) WHERE status = 'PENDING'`

---

## Constraints & Triggers

### CHECK Constraints
- Role/status enums
- Price >= 0
- ISJSON cho JSON columns
- quantity >= 0

### Triggers
- `trg_products_updatetimestamp`: tự động update `updated_at` khi update products
- `trg_orders_updatetimestamp`: tự động update `updated_at` khi update orders
- `trg_soldserial_updatestatus` (đã có): sau khi insert vào order_item_serials, tự động set serial=SOLD

---

## Seed Data

Xem file `DATABASE/seed.sql` cho dữ liệu mẫu bao gồm:
- 2 categories: Laptop, Điện thoại
- 5 brands: Apple, Samsung, Dell, Asus, Sony
- 5 products với variants
- 5 warehouses
- 5 users (admin, staff, warehouse, 2 customers)
- 5 orders với các trạng thái khác nhau
- Vouchers
- Stock movements

---

## Notes

1. **Soft delete:** Không thực sự xóa, chỉ set flag `is_active = 0` ở một số bảng (products, categories, users,...). Các bảng quan trọng (orders, order_items) không cho phép xóa.
2. **Multi-tenant:** Schema hiện tại là single-tenant. Nếu cần multi-tenant SaaS, thêm `tenant_id` vào hầu hết các bảng.
3. **Currency:** Tất cả giá dùng VND. Nếu cần multi-currency, thêm bảng `currencies` và lưu `currency_code` trong orders/products.
4. **Performance:** Với dữ liệu lớn, cần partitioning (theo ngày cho orders, theo warehouse cho inventories).
5. **Full-text search:** Nếu cần tìm kiếm sản phẩm theo mô tả, dùng SQL Server Full-Text Search hoặc chuyển sang ElasticSearch.
6. **Audit:** Bảng `audit_logs` nên được populate qua triggers hoặc application-level logging.
7. **JSON columns:** SQL Server 2016+ hỗ trợ JSON functions (JSON_VALUE, OPENJSON). Tuy nhiên, không thể index trực tiếp JSON. Nếu cần query sâu vào JSON, nên tạo computed columns và index chúng.

---

## License

MIT
