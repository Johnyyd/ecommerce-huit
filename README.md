# 🛒 ECommerce HUIT - Hệ thống bán hàng điện tử

**Thiết kế database & business logic hoàn chỉnh cho cửa hàng bán thiết bị điện tử.**

[![GitHub stars](https://img.shields.io/github/stars/Johnyyd/ecommerce-huit?style=social)](https://github.com/Johnyyd/ecommerce-huit)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![SQL Server](https://img.shields.io/badge/Database-Microsoft%20SQL%20Server-0078F6?logo=microsoft-sql-server)](https://www.microsoft.com/en-us/sql-server)
[![.NET](https://img.shields.io/badge/Backend-ASP.NET%20Core-512BD4?logo=dotnet)](https://dotnet.microsoft.com)

---

## 📖 Mục lục

- [Tổng quan](#tổng-quan)
- [Tính năng](#tính-năng)
- [Kiến trúc](#kiến-trúc)
- [Cài đặt & Chạy](#cài-đặt--chạy)
- [Thiết kế Database](#thiết-kế-database)
- [API](#api)
- [Quy trình nghiệp vụ](#quy-trình-nghiệp-vụ)
- [Triển khai](#triển-khai)
- [Đóng góp](#đóng-góp)
- [License](#license)

---

## Tổng quan

Dự án này cung cấp một **bộ khung vững chắc, dễ mở rộng** cho hệ thống bán hàng điện tử, tập trung vào quản lý kho, serial number (IMEI), voucher, và đa kho (multi-warehouse). Được thiết kế cho các cửa hàng thiết bị điện tử cần theo dõi serial, warranty, và quản lý tồn kho chi tiết.

---

## Tính năng

### 👥 Người dùng & Phân quyền
- **Roles:** ADMIN, STAFF, WAREHOUSE, CUSTOMER
- Quản lý user với status (ACTIVE/BANNED)
- Xác thực qua email/phone

### 🏷️ Danh mục & Thương hiệu
- Category hierarchy (parent-child)
- Brand management với xuất xứ
- Slug cho SEO-friendly URL

### 📦 Sản phẩm & Biến thể
- Product với specifications (JSON)
- Variants (SKU, price, cost, thumbnail)
- Manage multiple variants per product

### 🏢 Kho & Tồn kho
- **Multi-warehouse:** Hỗ trợ nhiều kho (thực vật, ảo)
- **Serial tracking:** Quản lý IMEI/serial number cho từng variant
- **Inventory:** theo dõi `quantity_on_hand`, `quantity_reserved`
- **Stock movements:** Nhập/xuất kho đầy đủ (audit trail)

### 🛒 Đơn hàng & Thanh toán
- Order với nhiều status (PENDING → CONFIRMED → PROCESSING → SHIPPING → COMPLETED/CANCELLED/RETURNED)
- Order items với price snapshot
- **Serial linking:** Gắn serial cụ thể vào order item
- Shipping address lưu JSON
- Payment method & status tracking
- **Voucher system:** Percent/Fixed discount, usage limit, min order value

### 🎟️ Khuyến mãi
- Voucher code với thời hạn
- Giảm giá theo % hoặc số tiền cố định
- Theo dõi usage count

### 📊 Báo cáo
- Doanh thu theo ngày/tháng
- Tồn kho
- Order metrics

---

## Kiến trúc

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Frontend      │    │   Backend API   │    │   Database      │
│  (React/Vue)    │◄──►│  (ASP.NET Core) │◄──►│  (SQL Server)   │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         │                       │                       ▼
         │                       │              ┌─────────────────┐
         │                       └─────────────►│   Redis Cache   │
         │                                      └─────────────────┘
         │                                               │
         ▼                                               ▼
┌─────────────────┐                              ┌─────────────────┐
│   Mobile App    │                              │  Email Service  │
│  (Flutter/React)│                              └─────────────────┘
└─────────────────┘
```

---

## Cài đặt & Chạy

### Yêu cầu
- **SQL Server 2019+** (hoặc Docker image)
- **.NET 8 SDK** (nếu chạy backend)
- **Node.js 18+** (nếu chạy frontend)
- **Redis** (cached)

### 1. Clone repo
```bash
git clone https://github.com/Johnyyd/ecommerce-huit.git
cd ecommerce-huit
```

### 2. Cấu hình Database
- Mở SQL Server Management Studio (SSMS) hoặc dùng `sqlcmd`
- Chạy script: `DATABASE/init.sql` để tạo schema và seed data

```bash
# Sử dụng sqlcmd
sqlcmd -S localhost -U SA -P YourPassword -i DATABASE/init.sql
```

### 3. Chạy Backend (API)
```bash
cd BACKEND/src
cp appsettings.example.json appsettings.json
# Sửa connection string trong appsettings.json
dotnet run
```

API sẽ chạy tại `https://localhost:5001` hoặc `http://localhost:5000`

### 4. Chạy Frontend
```bash
cd FRONTEND/src
npm install
npm run dev
```

Frontend sẽ chạy tại `http://localhost:3000`

---

## Thiết kế Database

Schema hoàn chỉnh bao gồm **18 bảng** với các module:

### Core Modules

| Module | Bảng | Mô tả |
|--------|------|-------|
| **User & Auth** | `users` | Quản lý tài khoản user với roles |
| | `addresses` | Địa chỉ của user (multiple) |
| | `permissions` | Permits chi tiết |
| | `role_permissions` | Mapping role-permission |
| **Catalog** | `categories` | Danh mục sản phẩm (hierarchy) |
| | `brands` | Thương hiệu |
| | `products` | Sản phẩm chính |
| | `product_variants` | Biến thể (SKU) |
| | `product_images` | Hình ảnh nhiều bản ghi |
| **Inventory** | `warehouses` | Kho hàng (physical/virtual) |
| | `inventories` | Tồn kho theo variant & warehouse |
| | `product_serials` | Serial number tracking |
| | `stock_movements` | Nhật ký nhập/xuất |
| | `suppliers` | Nhà cung cấp |
| **Order** | `orders` | Đơn hàng |
| | `order_items` | Chi tiết đơn hàng |
| | `order_item_serials` | Gắn serial vào order item |
| | `order_status_history` | Lịch sử thay đổi trạng thái |
| **Marketing** | `vouchers` | Mã giảm giá |
| | `voucher_usages` | Lịch sử sử dụng voucher |
| **Support** | `reviews` | Đánh giá sản phẩm |
| | `support_tickets` | Hỗ trợ khách hàng |
| | `returns` | Trả hàng/hoàn tiền |
| **Payments** | `payments` | Chi tiết giao dịch |
| **Cart** | `carts` | Giỏ hàng |
| | `cart_items` | Sản phẩm trong giỏ |
| **Audit** | `audit_logs` | Ghi log thay đổi dữ liệu |

---

## API Endpoints

### Authentication
```
POST   /api/auth/register
POST   /api/auth/login
POST   /api/auth/logout
POST   /api/auth/refresh-token
```

### Products
```
GET    /api/products               # List (with filters: category, brand, price range)
GET    /api/products/{id}          # Detail
GET    /api/products/{id}/variants # Variants
GET    /api/categories             # Tree
GET    /api/brands                 # List
```

### Cart
```
GET    /api/cart                   # My cart
POST   /api/cart/items             # Add item
PUT    /api/cart/items/{id}        # Update quantity
DELETE /api/cart/items/{id}        # Remove
POST   /api/cart/apply-voucher     # Apply voucher
```

### Orders
```
POST   /api/orders                 # Checkout
GET    /api/orders/{id}            # Detail order
GET    /api/orders                 # My orders history
POST   /api/orders/{id}/cancel     # Cancel order
POST   /api/orders/{id}/return     # Request return
```

### Admin (requires ADMIN/STAFF role)
```
GET    /admin/orders               # List all orders (with filters)
PUT    /admin/orders/{id}/status   # Update status
GET    /admin/inventory            # Stock levels
POST   /admin/inventory/import     # Import stock (with serials)
GET    /admin/reports/revenue      # Revenue report
GET    /admin/reports/top-products # Best sellers
```

---

## Quy trình nghiệp vụ

### 1. Order Flow (Mua hàng)
```
1. Customer browse products → filter by category/brand
2. Select variant → add to cart
3. Apply voucher (if any)
4. Checkout:
   - Create order (status: PENDING)
   - Reserve inventory (quantity_reserved++)
   - Deduct quantity_on_hand
   - Record stock movement (type: SALE_RESERVED)
5. Payment success → Update payment_status = PAID
6. Staff process order:
   - Pick items from warehouse (update serial status: RESERVED → SOLD)
   - Pack & ship → Update order status: SHIPPING
7. Customer receives → status: COMPLETED (trigger warranty start)
```

### 2. Inventory Import (Nhập kho)
```
1. Create purchase order from supplier
2. Receive goods at warehouse:
   - Scan serial numbers (IMEI)
   - Call sp_ImportStock (provided) to:
     * Insert serials with status AVAILABLE
     * Update inventories (quantity_on_hand++)
     * Log stock_movement (type: PURCHASE)
     * Update cost_price
```

### 3. Return/Refund
```
1. Customer request return within warranty period
2. Admin/Staff review → approve
3. Create return record:
   - Return serial to warehouse (status: AVAILABLE)
   - Refund via payment gateway
   - Update order status: RETURNED
   - Restock inventory
```

### 4. Voucher Redemption
```
- Validate: active, within date range, usage limit not exceeded, min order value
- Apply discount at checkout
- Record usage in voucher_usages
- Increment usage_count
```

---

## Triển khai

### Production Recommendations

- **Database:** SQL Server 2022 với AlwaysOn HA, daily backups
- **Backend:** Docker + Kubernetes (EKS/AKS) hoặc App Service
- **Frontend:** S3 + CloudFront (static hosting)
- **Cache:** Redis cluster
- **Queue:** RabbitMQ/Kafka cho async tasks (send email, update inventory)
- **Search:** ElasticSearch cho full-text product search
- **Monitoring:** Application Insights, Prometheus+Grafana
- **CI/CD:** GitHub Actions

### Security
- HTTPS everywhere
- JWT authentication với refresh token rotation
- Rate limiting (1000 req/5min per user)
- Input validation & SQL injection prevention (parameterized queries)
- CORS policy
- audit_logs cho tất cả thay đổi quan trọng

---

## 📚 Tài liệu bổ sung

- **Kiến trúc hệ thống:** [`docs/ARCHITECTURE.md`](./docs/ARCHITECTURE.md)
- **Ví dụ API calls:** [`docs/API_EXAMPLES.md`](./docs/API_EXAMPLES.md)
- **Khắc phục sự cố:** [`docs/TROUBLESHOOTING.md`](./docs/TROUBLESHOOTING.md)
- **Stored procedures:** [`DATABASE/init.sql`](../DATABASE/init.sql)
- **API reference (OpenAPI):** [`docs/openapi.yaml`](./docs/openapi.yaml)

---

## Đóng góp

1. Fork repo
2. Tạo feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add AmazingFeature'`)
4. Push (`git push origin feature/AmazingFeature`)
5. Mở Pull Request

---

## License

MIT License - Xem file [LICENSE](LICENSE) để biết thêm chi tiết.

---

## Liên hệ

**Author:** Tri Nguyen
**Email:** trinm2102@gmail.com
**Repo:** https://github.com/Johnyyd/ecommerce-huit

---

## 🙏 Cảm ơn

Được phát triển với mục đích học thuật & tham khảo cho các dự án thực tế.
