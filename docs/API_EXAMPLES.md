# API Examples - ECommerce HUIT

Collection of example API requests using cURL, JavaScript (fetch), and Python.

---

## 📚 Người dùng & Xác thực

### Đăng ký tài khoản mới

```http
POST /api/auth/register
Content-Type: application/json

{
  "full_name": "Nguyen Van A",
  "email": "a@example.com",
  "phone": "0909123456",
  "password": "StrongPass123"
}
```

**cURL:**
```bash
curl -X POST "https://api.huit.com/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "full_name": "Nguyen Van A",
    "email": "a@example.com",
    "phone": "0909123456",
    "password": "StrongPass123"
  }'
```

**Python:**
```python
import requests

url = "https://api.huit.com/api/auth/register"
payload = {
    "full_name": "Nguyen Van A",
    "email": "a@example.com",
    "phone": "0909123456",
    "password": "StrongPass123"
}
response = requests.post(url, json=payload)
print(response.json())
```

**Response (201 Created):**
```json
{
  "id": 1,
  "email": "a@example.com",
  "full_name": "Nguyen Van A",
  "role": "CUSTOMER",
  "access_token": "eyJhbGciOiJIUzI1NiIs...",
  "refresh_token": "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4..."
}
```

---

### Đăng nhập

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "a@example.com",
  "password": "StrongPass123"
}
```

**cURL:**
```bash
curl -X POST "https://api.huit.com/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"a@example.com","password":"StrongPass123"}'
```

**Response (200 OK):**
```json
{
  "id": 1,
  "email": "a@example.com",
  "full_name": "Nguyen Van A",
  "role": "CUSTOMER",
  "access_token": "eyJhbG...",
  "refresh_token": "dGhpcy..."
}
```

---

### Refresh Access Token

```http
POST /api/auth/refresh
Content-Type: application/json

{
  "refresh_token": "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4..."
}
```

**Response:**
```json
{
  "access_token": "new.jwt.token.here",
  "refresh_token": "optional-new-refresh-token"
}
```

---

## 🛍️ Sản phẩm

### Lấy danh sách sản phẩm (có filter, pagination)

```http
GET /api/products?page=1&pageSize=20&category=smartphone&brand=apple&minPrice=10000000&maxPrice=30000000&sortBy=price&order=desc
```

**Query Params:**

| Param | Type | Description |
|-------|------|-------------|
| `page` | int | Page number (default: 1) |
| `pageSize` | int | Items per page (default: 20, max: 100) |
| `category` | string | Category slug filter |
| `brand` | string | Brand name filter |
| `minPrice` | decimal | Minimum price |
| `maxPrice` | decimal | Maximum price |
| `status` | string | Product status (ACTIVE, DRAFT, HIDDEN) |
| `sortBy` | string | Field: name, price, created_at |
| `order` | string | asc or desc |

**Response (200 OK):**
```json
{
  "data": [
    {
      "id": 1,
      "name": "iPhone 15 Pro Max",
      "slug": "iphone-15-pro-max",
      "short_description": "Điện thoại cao cấp Apple",
      "price": 28990000,
      "original_price": 31990000,
      "thumbnail_url": "https://...",
      "brand_name": "Apple",
      "category_name": "Smartphone",
      "is_featured": true
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalItems": 150,
    "totalPages": 8
  }
}
```

---

### Lấy chi tiết sản phẩm (bao gồm biến thể, ảnh, tồn kho)

```http
GET /api/products/{id}
```

**cURL:**
```bash
curl -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  "https://api.huit.com/api/products/1"
```

**Response (200 OK):**
```json
{
  "id": 1,
  "name": "iPhone 15 Pro Max",
  "slug": "iphone-15-pro-max",
  "description": "Full description...",
  "specifications": { "chip": "A17 Pro", "ram": "8GB" },
  "brand": { "id": 1, "name": "Apple", "logo_url": "https://..." },
  "category": { "id": 1, "name": "Smartphone", "slug": "smartphone" },
  "variants": [
    {
      "id": 1,
      "sku": "IP15PM-256-BLK",
      "variant_name": "256GB - Black Titanium",
      "price": 28990000,
      "original_price": 31990000,
      "thumbnail_url": "https://...",
      "quantity_available": 15,
      "images": [
        { "image_url": "https://...", "alt_text": "Front view" },
        { "image_url": "https://...", "alt_text": "Back view" }
      ]
    }
  ],
  "reviews": [
    {
      "id": 1,
      "user_name": "Nguyen Van B",
      "rating": 5,
      "comment": "Rất tốt!",
      "created_at": "2025-03-01T10:30:00Z"
    }
  ],
  "is_featured": true,
  "created_at": "2025-02-15T08:00:00Z"
}
```

---

### Lấy danh sách danh mục

```http
GET /api/products/categories
```

**Response:**
```json
[
  {
    "id": 1,
    "name": "Smartphone",
    "slug": "smartphone",
    "parent_id": null,
    "description": "Điện thoại thông minh"
  },
  {
    "id": 2,
    "name": "iPhone",
    "slug": "iphone",
    "parent_id": 1,
    "description": "Apple iPhone series"
  }
]
```

---

### Lấy danh sách thương hiệu

```http
GET /api/products/brands
```

**Response:**
```json
[
  {
    "id": 1,
    "name": "Apple",
    "logo_url": "https://...",
    "origin": "USA"
  },
  {
    "id": 2,
    "name": "Samsung",
    "logo_url": "https://...",
    "origin": "South Korea"
  }
]
```

---

## 🛒 Giỏ hàng

**Tất cả endpoints giỏ hàng cần authentication**

### Lấy giỏ hàng hiện tại

```http
GET /api/cart
Authorization: Bearer {access_token}
```

**Response (200 OK):**
```json
{
  "id": 1,
  "user_id": 1,
  "items": [
    {
      "id": 1,
      "variant_id": 1,
      "sku": "IP15PM-256-BLK",
      "product_name": "iPhone 15 Pro Max",
      "variant_name": "256GB - Black Titanium",
      "price": 28990000,
      "quantity": 2,
      "subtotal": 57980000,
      "thumbnail_url": "https://..."
    }
  ],
  "subtotal": 57980000,
  "voucher_code": null,
  "discount_amount": 0,
  "shipping_fee": 0,
  "total_amount": 57980000
}
```

---

### Thêm sản phẩm vào giỏ

```http
POST /api/cart/items
Content-Type: application/json
Authorization: Bearer {access_token}

{
  "variant_id": 1,
  "quantity": 2
}
```

**Validation:**
- `quantity` phải > 0
- Kiểm tra variant tồn tại và `is_active=1`
- Kiểm tra tồn kho `quantity_available`

**Response (200 OK):**
```json
{
  "item": {
    "id": 1,
    "variant_id": 1,
    "quantity": 2,
    "price": 28990000,
    "subtotal": 57980000
  },
  "message": "Đã thêm vào giỏ hàng"
}
```

---

### Cập nhật số lượng

```http
PUT /api/cart/items/{itemId}
Content-Type: application/json
Authorization: Bearer {access_token}

{
  "quantity": 3
}
```

---

### Xóa item khỏi giỏ

```http
DELETE /api/cart/items/{itemId}
Authorization: Bearer {access_token}
```

---

### Xóa toàn bộ giỏ

```http
DELETE /api/cart
Authorization: Bearer {access_token}
```

---

### Áp dụng voucher

```http
POST /api/cart/voucher
Content-Type: application/json
Authorization: Bearer {access_token}

{
  "voucher_code": "WELCOME10"
}
```

**Response:**
```json
{
  "voucher_code": "WELCOME10",
  "discount_amount": 2899000,
  "new_total": 55091000,
  "message": "Voucher đã được áp dụng"
}
```

---

## 📦 Đơn hàng

### Tạo đơn hàng từ giỏ hàng

```http
POST /api/orders
Content-Type: application/json
Authorization: Bearer {access_token}

{
  "shipping_address_id": 1,
  "payment_method": "COD",
  "voucher_code": "WELCOME10"
}
```

Hoặc dùng `shipping_address` trực tiếp:

```json
{
  "shipping_address": {
    "receiver_name": "Nguyen Van A",
    "receiver_phone": "0909123456",
    "province": "HCMC",
    "district": "District 1",
    "ward": "Ben Nghe",
    "street_address": "123 Le Loi St."
  },
  "payment_method": "MOMO"
}
```

**Response (201 Created):**
```json
{
  "id": 1,
  "code": "ORD2025030300001",
  "user_id": 1,
  "subtotal": 57980000,
  "tax_amount": 5798000,
  "shipping_fee": 0,
  "discount_amount": 2899000,
  "total_amount": 60879000,
  "status": "PENDING",
  "payment_method": "COD",
  "payment_status": "PENDING",
  "shipping_address": "{...}",
  "items": [
    {
      "variant_id": 1,
      "sku": "IP15PM-256-BLK",
      "product_name": "iPhone 15 Pro Max",
      "variant_name": "256GB - Black Titanium",
      "quantity": 2,
      "price": 28990000,
      "subtotal": 57980000
    }
  ],
  "created_at": "2025-03-03T08:45:12Z"
}
```

---

### Lấy lịch sử đơn hàng của mình

```http
GET /api/orders?page=1&pageSize=10
Authorization: Bearer {access_token}
```

**Response:**
```json
{
  "data": [
    {
      "id": 1,
      "code": "ORD2025030300001",
      "total_amount": 60879000,
      "status": "PENDING",
      "payment_status": "PENDING",
      "item_count": 2,
      "created_at": "2025-03-03T08:45:12Z"
    }
  ],
  "pagination": { ... }
}
```

---

### Lấy chi tiết đơn hàng

```http
GET /api/orders/{orderCode}
Authorization: Bearer {access_token}
```

**Response:**
```json
{
  "id": 1,
  "code": "ORD2025030300001",
  "user": { "full_name": "Nguyen Van A", "email": "a@example.com" },
  "subtotal": 57980000,
  "tax_amount": 5798000,
  "shipping_fee": 0,
  "discount_amount": 2899000,
  "total_amount": 60879000,
  "status": "PENDING",
  "payment_method": "COD",
  "payment_status": "PENDING",
  "shipping_address": { ... },
  "items": [
    {
      "id": 1,
      "variant_id": 1,
      "sku": "IP15PM-256-BLK",
      "product_name": "iPhone 15 Pro Max",
      "variant_name": "256GB - Black Titanium",
      "quantity": 2,
      "price": 28990000,
      "subtotal": 57980000,
      "serials": [
        { "serial_number": "123456789012345", "status": "RESERVED" },
        { "serial_number": "123456789012346", "status": "RESERVED" }
      ]
    }
  ],
  "status_history": [
    { "status": "PENDING", "note": "Đơn hàng được tạo", "created_at": "..." },
    { "status": "CONFIRMED", "note": "Đã xác nhận", "created_at": "..." }
  ],
  "created_at": "2025-03-03T08:45:12Z"
}
```

---

### Hủy đơn hàng (chỉ PENDING/CONFIRMED)

```http
DELETE /api/orders/{orderCode}
Content-Type: application/json
Authorization: Bearer {access_token}

{
  "reason": "Tôi muốn thay đổi địa chỉ giao hàng"
}
```

**Response (200 OK):**
```json
{
  "message": "Đơn hàng đã được hủy",
  "refund_amount": 0
}
```

---

### Yêu cầu trả hàng (sau khi COMPLETED)

```http
POST /api/orders/{orderCode}/return
Content-Type: application/json
Authorization: Bearer {access_token}

{
  "reason": "Sản phẩm bị lỗi",
  "items": [
    {
      "order_item_id": 1,
      "quantity": 1,
      "serial_number": "123456789012345"
    }
  ]
}
```

**Response (201):**
```json
{
  "id": 1,
  "order_id": 1,
  "status": "REQUESTED",
  "items": [...],
  "created_at": "2025-03-03T09:30:00Z"
}
```

---

## 👨‍💼 Admin/Staff Endpoints

**Yêu cầu role ADMIN hoặc STAFF**

### Lấy tất cả đơn hàng (admin)

```http
GET /admin/orders?status=PENDING&page=1&pageSize=20
Authorization: Bearer {admin_token}
```

---

### Cập nhật trạng thái đơn hàng (admin/staff)

```http
PUT /admin/orders/{orderId}/status
Content-Type: application/json
Authorization: Bearer {staff_token}

{
  "status": "CONFIRMED"
}
```

Valid transitions:
- PENDING → CONFIRMED
- CONFIRMED → SHIPPING (cần `tracking_number`)
- SHIPPING → COMPLETED
- PENDING/CONFIRMED → CANCELLED

---

### Xác nhận đơn hàng → SHIPPING (gán serial)

```http
PUT /admin/orders/{orderId}/ship
Content-Type: application/json
Authorization: Bearer {staff_token}

{
  "tracking_number": "VIETTEL123456789",
  "shipping_provider": "Viettel Post"
}
```

**Backend sẽ:**
- Kiểm tra order ở CONFIRMED
- Gán serial numbers cho những sản phẩm có serial tracking
- Update status → SHIPPING
- Ghi `shipped_at`

---

### Hoàn thành đơn hàng

```http
PUT /admin/orders/{orderId}/complete
Authorization: Bearer {staff_token}
```

---

### Nhập kho (bulk import)

```http
POST /admin/inventory/import
Content-Type: application/json
Authorization: Bearer {warehouse_token}

{
  "warehouse_id": 1,
  "supplier_id": 1,
  "items": [
    {
      "variant_id": 1,
      "quantity": 100,
      "unit_cost": 25000000,
      "note": "Nhập lô mới"
    },
    {
      "variant_id": 2,
      "quantity": 50,
      "unit_cost": 12000000
    }
  ]
}
```

---

### Báo cáo doanh thu

```http
GET /admin/reports/revenue?startDate=2025-03-01&endDate=2025-03-31
Authorization: Bearer {staff_token}
```

**Response:**
```json
[
  {
    "sale_date": "2025-03-01",
    "order_count": 15,
    "customer_count": 14,
    "total_revenue": 450000000,
    "total_tax": 45000000,
    "total_shipping": 150000
  }
]
```

---

### Báo cáo tồn kho thấp

```http
GET /admin/inventory/low-stock?threshold=10
Authorization: Bearer {warehouse_token}
```

**Response:**
```json
[
  {
    "product_id": 1,
    "product_name": "iPhone 15 Pro Max",
    "variant_id": 1,
    "sku": "IP15PM-256-BLK",
    "warehouse": "Kho chính",
    "quantity_on_hand": 5
  }
]
```

---

## 🔐 Authentication Headers

Tất cả endpoints trừ `/api/auth/*` và `/api/products` (read-only) đều cần header:

```
Authorization: Bearer {access_token}
```

**Lấy token từ đăng nhập hoặc register.**

**Token hết hạn (401):** Gọi `/api/auth/refresh` với `refresh_token` để lấy access token mới.

---

## 📊 Response Format Tiêu chuẩn

**Success:**
```json
{
  "success": true,
  "data": { ... },
  "message": "Optional success message",
  "pagination": { ... } // optional
}
```

**Error:**
```json
{
  "success": false,
  "error": "VALIDATION_ERROR",
  "message": "Mô tả lỗi",
  "details": [ // optional
    { "field": "email", "message": "Email không hợp lệ" }
  ]
}
```

**HTTP Status Codes:**

| Code | Meaning |
|------|---------|
| 200 | OK |
| 201 | Created |
| 204 | No Content (delete success) |
| 400 | Bad Request (validation error) |
| 401 | Unauthorized (no token or invalid) |
| 403 | Forbidden (insufficient permissions) |
| 404 | Not Found |
| 409 | Conflict (duplicate email, insufficient stock) |
| 422 | Unprocessable Entity (business rule violation) |
| 500 | Internal Server Error |

---

## 💾 Serial Number Tracking (IMEI)

Khi sản phẩm có `requires_serial_tracking = true` (điện tử), quy trình:

1. **Đặt hàng:** `sp_CreateOrder` reserve inventory
2. **Xác nhận → Gửi hàng:** `sp_ShipOrder` sẽ:
   - Lấy các `product_serial` có status `AVAILABLE` cho variant đó
   - Gán vào `order_item_serials`
   - Update serial status → `RESERVED`
3. **Hoàn thành:** `sp_CompleteOrder` update serial → `SOLD`

**API Response sẽ bao gồm `serials` array trong `OrderItem`.**

---

## 🧪 Testing với Postman/Insomnia

Import file OpenAPI vào Postman:

```bash
# Từ thư mục dự án
curl -o openapi.json https://raw.githubusercontent.com/Johnyyd/ecommerce-huit/main/docs/openapi.yaml
# Import vào Postman
```

---

**Happy API Testing! 🎯**
