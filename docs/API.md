# API Reference - ECommerce HUIT

**Base URL:** `https://api.huit.edu.vn` (development: `http://localhost:5000`)

**Authentication:** Bearer Token (JWT) trong header `Authorization: Bearer {token}`

---

## 📑 Table of Contents

1. [Authentication](#authentication)
2. [Users](#users)
3. [Products](#products)
4. [Cart](#cart)
5. [Orders](#orders)
6. [Payments](#payments)
7. [Vouchers](#vouchers)
8. [Inventory](#inventory) (Admin/Staff)
9. [Admin Reports](#admin-reports)
10. [Data Types](#data-types)

---

## Authentication

### `POST /api/auth/register`

Đăng ký tài khoản mới.

**Request:**
```json
{
  "full_name": "Nguyễn Văn A",
  "email": "nguyenvana@example.com",
  "phone": "0909123456",
  "password": "StrongP@ss123"
}
```

**Response (201):**
```json
{
  "id": 123,
  "email": "nguyenvana@example.com",
  "full_name": "Nguyễn Văn A",
  "role": "CUSTOMER",
  "access_token": "eyJhbGciOiJIUzI1NiIs...",
  "refresh_token": "eyJhbGciOiJIUzI1NiIs..."
}
```

---

### `POST /api/auth/login`

Đăng nhập.

**Request:**
```json
{
  "email": "nguyenvana@example.com",
  "password": "StrongP@ss123"
}
```

**Response (200):**
```json
{
  "id": 123,
  "email": "nguyenvana@example.com",
  "full_name": "Nguyễn Văn A",
  "role": "CUSTOMER",
  "access_token": "eyJhbGciOiJIUzI1NiIs...",
  "refresh_token": "eyJhbGciOiJIUzI1NiIs..."
}
```

---

### `POST /api/auth/refresh-token`

Lấy access token mới bằng refresh token.

**Request:**
```json
{
  "refresh_token": "eyJhbGciOiJIUzI1NiIs..."
}
```

**Response:**
```json
{
  "access_token": "new_access_token_here"
}
```

---

### `POST /api/auth/logout`

Xóa refresh token (logout). Client nên xóa access_token.

**Headers:**
- `Authorization: Bearer {access_token}`

**Response:** 204 No Content

---

## Users

### `GET /api/users/profile`

Lấy thông tin profile của user hiện tại.

**Headers:** `Authorization`

**Response (200):**
```json
{
  "id": 123,
  "full_name": "Nguyễn Văn A",
  "email": "nguyenvana@example.com",
  "phone": "0909123456",
  "avatar_url": "https://example.com/avatar.jpg",
  "role": "CUSTOMER",
  "status": "ACTIVE",
  "last_login": "2025-02-14T10:30:00Z",
  "created_at": "2025-01-01T08:00:00Z"
}
```

---

### `PUT /api/users/profile`

Cập nhật thông tin user.

**Headers:** `Authorization`

**Request:**
```json
{
  "full_name": "Nguyễn Văn B",
  "avatar_url": "https://example.com/new-avatar.jpg"
}
```

**Response (200):** updated user object

---

### `GET /api/users/addresses`

Lấy danh sách địa chỉ của user.

**Headers:** `Authorization`

**Response:**
```json
[
  {
    "id": 1,
    "label": "Nhà",
    "receiver_name": "Nguyễn Văn A",
    "receiver_phone": "0909123456",
    "province": "TP. Hồ Chí Minh",
    "district": "Quận Tân Phú",
    "ward": "Phường Tân Thới Hòa",
    "street_address": "123 Lê Trọng Tấn",
    "is_default": true
  }
]
```

---

### `POST /api/users/addresses`

Thêm địa chỉ mới.

**Request:**
```json
{
  "label": "Văn phòng",
  "receiver_name": "Nguyễn Văn A",
  "receiver_phone": "0909123456",
  "province": "TP. Hồ Chí Minh",
  "district": "Quận 1",
  "ward": "Phường Bến Nghé",
  "street_address": "45 Nguyễn Huệ",
  "is_default": false
}
```

---

## Products

### `GET /api/products`

Lấy danh sách sản phẩm với filter, pagination.

**Query Parameters:**
- `page` (int, default: 1)
- `pageSize` (int, default: 20, max: 100)
- `categoryId` (int)
- `brandId` (int)
- `minPrice`, `maxPrice` (decimal)
- `search` (string) - search trong name, description
- `sortBy` (string) - 'price_asc', 'price_desc', 'newest', 'name'
- `inStock` (bool) - chỉ lấy còn hàng

**Response (200):**
```json
{
  "data": [
    {
      "id": 1,
      "name": "iPhone 15 Pro Max",
      "slug": "iphone-15-pro-max",
      "brand": { "id": 1, "name": "Apple", "origin": "USA" },
      "category": { "id": 3, "name": "Điện Thoại" },
      "short_description": "...",
      "price_from": 28990000,
      "price_to": 34990000,
      "thumbnail_url": "https://...",
      "rating_average": 4.5,
      "review_count": 120,
      "is_featured": false
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

### `GET /api/products/{id}`

Chi tiết sản phẩm.

**Response (200):**
```json
{
  "id": 1,
  "name": "iPhone 15 Pro Max",
  "slug": "iphone-15-pro-max",
  "description": "<p>Chi tiết sản phẩm...</p>",
  "specifications": {
    "screen": "6.7 inch",
    "chip": "A17 Pro",
    "ram": "8GB",
    "battery": "4422 mAh"
  },
  "brand": { "id": 1, "name": "Apple", "origin": "USA", "logo_url": "..." },
  "category": { "id": 3, "name": "Điện Thoại", "slug": "smart-phone" },
  "variants": [
    {
      "id": 1,
      "sku": "IP15PM-256-TI",
      "variant_name": "256GB - Titan Tự Nhiên",
      "price": 28990000,
      "original_price": 34990000,
      "thumbnail_url": "...",
      "quantity_available": 10,
      "is_active": true
    }
  ],
  "images": [
    { "image_url": "https://...", "alt_text": "Ảnh chính" },
    { "image_url": "https://...", "alt_text": "Góc nhìn 2" }
  ],
  "reviews": [
    {
      "id": 1,
      "user": { "full_name": "Nguyễn Văn X", "avatar_url": "..." },
      "rating": 5,
      "title": "Rất tốt",
      "content": "Điện thoại chạy mượt...",
      "created_at": "2025-02-10T..."
    }
  ],
  "rating_average": 4.5,
  "review_count": 120
}
```

---

### `GET /api/categories`

Lấy toàn bộ category dạng tree.

**Response (200):**
```json
[
  {
    "id": 1,
    "name": "Laptop Gaming",
    "slug": "laptop-gaming",
    "children": [
      { "id": 10, "name": "Laptop Gaming Dell", "slug": "laptop-gaming-dell" }
    ]
  }
]
```

---

### `GET /api/brands`

Danh sách brands (có thể filter, pagination).

---

## Cart

### `GET /api/cart`

Lấy giỏ hàng của user hiện tại.

**Headers:** `Authorization`

**Response (200):**
```json
{
  "id": 5,
  "items": [
    {
      "id": 10,
      "variant": {
        "id": 1,
        "sku": "IP15PM-256-TI",
        "variant_name": "256GB - Titan",
        "price": 28990000,
        "thumbnail_url": "..."
      },
      "quantity": 2,
      "line_total": 57980000
    }
  ],
  "subtotal": 57980000,
  "discount": 0,
  "estimated_shipping": 0,
  "total": 57980000,
  "applied_voucher": null
}
```

---

### `POST /api/cart/items`

Thêm sản phẩm vào giỏ hàng.

**Headers:** `Authorization`

**Request:**
```json
{
  "variant_id": 1,
  "quantity": 1
}
```

**Response (201):** updated cart object

---

### `PUT /api/cart/items/{itemId}`

Cập nhật số lượng.

**Request:**
```json
{
  "quantity": 3
}
```

---

### `DELETE /api/cart/items/{itemId}`

Xóa item khỏi giỏ.

---

### `POST /api/cart/apply-voucher`

Áp dụng voucher vào giỏ hàng.

**Request:**
```json
{
  "code": "HUIT2026"
}
```

**Response (200):**
```json
{
  "success": true,
  "discount_amount": 2899000,
  "new_total": 26091000
}
```

**Response (400):** nếu voucher không hợp lệ, hết hạn, vượt limit.

---

## Orders

### `POST /api/orders` (Checkout)

Tạo đơn hàng mới từ giỏ hàng.

**Headers:** `Authorization`

**Request:**
```json
{
  "shipping_address": {
    "label": "Nhà",
    "receiver_name": "Nguyễn Văn A",
    "receiver_phone": "0909123456",
    "province": "TP. Hồ Chí Minh",
    "district": "Quận Tân Phú",
    "ward": "Phường Tân Thới Hòa",
    "street_address": "123 Lê Trọng Tấn"
  },
  "payment_method": "MOMO",
  "note": "Giao giữa trưa"
}
```

**Process:**
1. Lấy giỏ hàng hiện tại
2. Validate tồn kho (real-time)
3. Tạo order (status: PENDING)
4. Reserve inventory (quantity_reserved++)
5. Log stock_movement
6. Xóa giỏ hàng (cart items)
7. Return order ID + code

**Response (201):**
```json
{
  "order_id": 123,
  "order_code": "ORD-202602140001",
  "total": 28990000,
  "payment_url": "https://momo.vn/..." // nếu cần redirect
}
```

**Errors (400):**
- Out of stock
- Invalid payment method

---

### `GET /api/orders`

Lấy danh sách đơn hàng của user hiện tại (pagination).

**Query:** `page`, `pageSize`

**Response:**
```json
{
  "data": [
    {
      "id": 123,
      "code": "ORD-202602140001",
      "total": 28990000,
      "status": "COMPLETED",
      "payment_status": "PAID",
      "created_at": "2025-02-14T10:00:00Z",
      "items": [
        {
          "product_name": "iPhone 15 Pro Max 256GB",
          "quantity": 1,
          "unit_price": 28990000
        }
      ]
    }
  ],
  "pagination": { ... }
}
```

---

### `GET /api/orders/{orderCode}`

Chi tiết một đơn hàng.

**Response:**
```json
{
  "id": 123,
  "code": "ORD-202602140001",
  "user": { "full_name": "...", "phone": "..." },
  "shipping_address": { ... },
  "subtotal": 28990000,
  "discount": 0,
  "shipping_fee": 0,
  "total": 28990000,
  "payment_method": "MOMO",
  "payment_status": "PAID",
  "status": "COMPLETED",
  "note": "Giao giữa trưa",
  "created_at": "...",
  "items": [
    {
      "product_name": "...",
      "sku": "...",
      "quantity": 1,
      "unit_price": 28990000,
      "serial_numbers": ["IMEI-IP15-003"] // Chỉ hiển thị nếu đã xác nhận & có serial
    }
  ],
  "status_history": [
    { "status": "PENDING", "created_at": "...", "note": "Đặt hàng thành công" },
    { "status": "CONFIRMED", "created_at": "...", "note": "Đã xác nhận" },
    { "status": "COMPLETED", "created_at": "...", "note": "Đã giao hàng" }
  ]
}
```

---

### `POST /api/orders/{orderCode}/cancel`

Hủy đơn hàng.

**Headers:** `Authorization`

**Body:**
```json
{
  "reason": "Đặt nhầm sản phẩm"
}
```

**Response:** 204 No Content (hoặc 400 nếu không thể hủy)

---

### `POST /api/orders/{orderCode}/return`

Yêu cầu trả hàng.

**Headers:** `Authorization`

**Request:**
```json
{
  "reason": "Sản phẩm lỗi",
  "item_ids": [1, 2] // order_item IDs muốn trả
}
```

**Response (201):**
```json
{
  "return_id": 45,
  "return_number": "RET-202602140001",
  "status": "REQUESTED"
}
```

---

## Payments

### `POST /api/payments/webhook`

Webhook từ cổng thanh toán (MOMO, VNPAY).

**Headers:** `X-Signature` (chữ ký để verify)

**Payload:** Từ gateway

**Response:** 200 OK (không trả body)

---

## Vouchers

### `GET /api/vouchers/validate`

Kiểm tra voucher hợp lệ.

**Query:** `code=HUIT2026`

**Response (200):**
```json
{
  "valid": true,
  "voucher": {
    "id": 1,
    "code": "HUIT2026",
    "name": "Chào tân sinh viên 2026",
    "discount_type": "PERCENT",
    "discount_value": 10,
    "max_discount_amount": 500000,
    "min_order_value": 2000000,
    "remaining_usage": 950 // nếu có limit
  }
}
```

**Response (400):** `{ "valid": false, "reason": "Voucher đã hết hạn" }`

---

## Admin (Strictly require ADMIN/STAFF role)

### `GET /admin/orders`

Danh sách tất cả đơn hàng (có filter).

**Query Params:**
- `status` (string)
- `fromDate`, `toDate` (ISO)
- `paymentStatus`
- `userId`
- `page`, `pageSize`

**Response:** similar to `/api/orders` nhưng full admin view.

---

### `PUT /admin/orders/{orderCode}/status`

Cập nhật trạng thái đơn hàng.

**Request:**
```json
{
  "status": "SHIPPING",
  "note": "Đã đóng gói, vận chuyển"
}
```

---

### `GET /admin/inventory`

Xem tồn kho theo warehouse.

**Query:**
- `warehouseId` (optional)
- `lowStockOnly` (bool)

**Response:**
```json
[
  {
    "warehouse": { "id": 1, "name": "Kho Tổng" },
    "variant": { "sku": "IP15PM-256-TI", "product_name": "iPhone 15 Pro Max 256GB" },
    "quantity_on_hand": 10,
    "quantity_reserved": 2,
    "available": 8
  }
]
```

---

### `POST /admin/inventory/import`

Nhập kho hàng mới (với serial numbers).

**Request:**
```json
{
  "warehouse_id": 1,
  "variant_id": 1,
  "cost_price": 26000000,
  "supplier_id": 1,
  "serials": ["IMEI001", "IMEI002", "IMEI003"]
}
```

**Response (201):**
```json
{
  "success": true,
  "imported_serials": 3,
  "new_quantity_on_hand": 13
}
```

---

### `POST /admin/inventory/transfer`

Chuyển kho (transfer giữa các warehouse).

**Request:**
```json
{
  "from_warehouse_id": 1,
  "to_warehouse_id": 2,
  "variant_id": 1,
  "quantity": 5,
  "note": "Chuyển ra showroom Q1"
}
```

---

### `GET /admin/reports/revenue`

Báo cáo doanh thu theo ngày/tháng.

**Query:**
- `fromDate`, `toDate`
- `groupBy` ('day', 'month')

**Response:**
```json
[
  {
    "period": "2025-02-14",
    "orders_count": 25,
    "revenue": 725000000,
    "discount_total": 50000000,
    "average_order_value": 29000000
  }
]
```

---

### `GET /admin/reports/top-products`

Sản phẩm bán chạy.

**Query:** `fromDate`, `toDate`, `limit=10`

**Response:**
```json
[
  {
    "product_id": 1,
    "product_name": "iPhone 15 Pro Max 256GB",
    "quantity_sold": 50,
    "revenue": 1449500000
  }
]
```

---

### `GET /admin/suppliers`

Quản lý nhà cung cấp (CRUD).

---

## Data Types

### UserRole (enum)
- `ADMIN`
- `STAFF`
- `WAREHOUSE`
- `CUSTOMER`

### OrderStatus (enum)
- `PENDING`
- `CONFIRMED`
- `PROCESSING`
- `SHIPPING`
- `COMPLETED`
- `CANCELLED`
- `RETURNED`

### PaymentStatus (enum)
- `PENDING`
- `PAID`
- `FAILED`
- `REFUNDED`

### PaymentMethod (enum)
- `CASH`
- `MOMO`
- `VNPAY`
- `BANKING`
- `COD`

### DiscountType (enum)
- `PERCENT`
- `FIXED`

### SerialStatus (enum)
- `AVAILABLE`
- `RESERVED`
- `SOLD`
- `DEFECTIVE`
- `TRANSFERRING`
- `RETURNED`

### MovementType (string)
- `PURCHASE`
- `SALE_RESERVED`
- `SALE_SHIP`
- `RETURN`
- `TRANSFER`
- `ADJUSTMENT`
- `INITIAL`

---

## Error Handling

All errors return JSON:

```json
{
  "error": "InvalidInput",
  "message": "Mô tả lỗi",
  "details": { "field": "quantity", "issue": "must be greater than 0" }
}
```

**Status Codes:**
- 200: Success
- 201: Created
- 204: No Content (delete, cancel)
- 400: Bad Request (validation)
- 401: Unauthorized
- 403: Forbidden (insufficient permissions)
- 404: Not Found
- 409: Conflict (duplicate, out of stock)
- 422: Unprocessable Entity (business rule violation)
- 500: Internal Server Error

---

## Rate Limiting

- 1000 requests / 5 minutes per user (authenticated)
- 100 requests / 5 minutes per IP (anonymous)

Headers trả về:
```
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 995
X-RateLimit-Reset: 1705219200
```

---

## Pagination

Standard response:

```json
{
  "data": [ ... ],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalItems": 350,
    "totalPages": 18
  }
}
```

---

## Filtering

Multi-field filter dùng query string:

```
GET /api/products?brandId=1&categoryId=3&minPrice=10000000&maxPrice=30000000&inStock=true
```

---

## Sorting

```
GET /api/products?sortBy=price_asc
GET /api/orders?sortBy=created_at_desc
```

---

## HATEOAS (Optional)

Có thể bổ sung links trong response:

```json
{
  "id": 1,
  "name": "...",
  "_links": {
    "self": { "href": "/api/products/1" },
    "variants": { "href": "/api/products/1/variants" }
  }
}
```

---

## OpenAPI Spec

File `openapi.yaml` sẽ được cung cấp trong repo để import vào Swagger/Postman.

---

End of API Reference.
