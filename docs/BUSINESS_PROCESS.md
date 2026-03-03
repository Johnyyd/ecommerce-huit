# Quy trình nghiệp vụ (Business Process)

Tài liệu này mô tả các quy trình chính trong hệ thống bán hàng điện tử HUIT.

---

## 1. Quy trình Mua hàng (Customer Journey)

### Các bước:

1. **Đăng nhập / Đăng ký**
   - Customer truy cập trang chủ
   - Nếu chưa có tài khoản → `/auth/register`
   - Nếu có → `/auth/login`
   - Lưu JWT vào localStorage

2. **Duyệt sản phẩm**
   - Xem danh mục (categories tree)
   - Filter theo brand, price range
   - Search sản phẩm
   - Xem chi tiết sản phẩm, chọn variant

3. **Thêm vào giỏ hàng**
   - POST `/cart/items` với `variant_id` và `quantity`
   - Kiểm tra tồn kho real-time (frontend gọi API kiểm tra quantity available)

4. **Xem giỏ hàng**
   - GET `/cart`
   - Có thể thay đổi số lượng, xóa item
   - Click "Apply voucher" → POST `/cart/apply-voucher`

5. **Checkout**
   - Chọn địa chỉ giao hàng (từ addresses hoặc thêm mới)
   - Chọn phương thức thanh toán (MOMO, VNPAY, COD, BANKING)
   - Xác nhận đơn hàng → POST `/orders`
   - Backend:
     - Validate tồn kho
     - Tạo order (status: PENDING)
     - Reserve inventory: `inventories.quantity_reserved` += quantity
     - Insert stock_movement (type=SALE_RESERVED)
     - Xóa cart items
   - Return order_id, order_code
   - Nếu thanh toán online → trả về payment_url để redirect

6. **Thanh toán**
   - Đối với COD: không cần làm gì, khi nhận hàng mới thanh toán
   - Đối với online: redirect đến cổng thanh toán
   - Cổng thanh toán gọi webhook `/payments/webhook` để thông báo kết quả
   - Backend cập nhật `orders.payment_status` = PAID/FAILED

7. **Xử lý đơn hàng (Staff)**
   - Admin/Staff xem đơn hàng mới trong `/admin/orders?status=PENDING`
   - Click "Confirm" → PUT `/admin/orders/{code}/status` với status=CONFIRMED
   - Gọi stored procedure `sp_ConfirmOrder`:
     * Tạo order_status_history
     * Có thể gửi email xác nhận

8. ** Xuất kho & Đóng gói**
   - Staff vào chi tiết order, click "Prepare Shipment"
   - System hiển thị danh sách serial numbers cần lấy từ kho (theo variant)
   - Staff scan từng serial, hoặc chọn tự động allocate từ kho tồn kho (serial status AVAILABLE)
   - Khi đủ, click "Ship":
     * Cập nhật `product_serials.status` = SOLD (chỉ những serial đã RESERVED)
     * Cập nhật `inventories.quantity_reserved` -= quantity, `quantity_on_hand` giữ nguyên (đã trừ khi reserve)
     * Insert stock_movement type='SALE_SHIP'
     * Update order.status = SHIPPING
   - Gửi email thông báo cho customer (tracking number, expected delivery)

9. **Giao hàng & Hoàn tất**
   - Khi khách hàng nhận hàng, staff click "Complete Order" (hoặc tự động sau X ngày)
   - Backend:
     * Update order.status = COMPLETED
     * Ghi `order_status_history`
     * Tính toán warranty_expire_date cho serial (nếu cần)
     * Tăng doanh thu (daily_revenue)

10. **Đánh giá**
    - Customer có thể review sản phẩm sau khi completed
    - POST `/api/reviews`
    - Admin có thể duyệt review trước khi public

---

## 2. Quy trình Nhập kho (Purchase & Receiving)

### 2.1 Tạo Purchase Order ( supplier → warehouse )
1. Admin/Staff tạo purchase order (bảng `suppliers` và `stock_movements` với type='PURCHASE')
2. Gửi đơn đặt hàng cho supplier

### 2.2 Nhận hàng (Receiving)
1. Khi hàng về kho:
   - Scan từng serial number (IMEI)
   - Nhập vào form: `warehouse_id`, `variant_id`, `cost_price`, danh sách serials
   - Gọi API `/admin/inventory/import` (hoặc chạy stored procedure `sp_ImportStock`)
   - Backend:
     * Insert `product_serials` với status=AVAILABLE
     * Upsert `inventories` → `quantity_on_hand` += count
     * Insert `stock_movement` type=PURCHASE
     * Update `product_variants.cost_price` (giá nhập mới nhất)

2. Nếu hàng về nhiều lô, có thể tạo nhiều movement entry.

---

## 3. Quy trình Quản lý Tồn kho (Inventory Management)

### 3.1 Kiểm kê (Stocktake)
1. Staff chọn warehouse
2. xuất báo cáo tồn kho hiện tại: `vw_InventoryDashboard`
3. So sánh với thực tế (đếm)
4. Nếu chênh lệch → create stock movement type='ADJUSTMENT' với số lượng âm/dương

### 3.2 Cảnh báo hết hàng
- Cron job chạy hàng ngày:
  * Query `inventories` where `quantity_on_hand <= reorder_point`
  * Gửi email cho warehouse manager
  * Tạo task trong hệ thống

### 3.3 Chuyển kho (Transfer)
- Khi cần chuyển hàng từ kho tổng → showroom:
  1. Tạo stock movement type='TRANSFER' (từ kho A đến kho B)
  2. Giảm kho A: `quantity_on_hand` -= qty
  3. Tăng kho B: `quantity_on_hand` += qty (nên dùng transaction)
  4. Cập nhật `product_serials.warehouse_id` (nếu theo dõi serial cần update từng serial)

---

## 4. Quy trình Trả hàng & Hoàn tiền (Returns & Refunds)

### 4.1 Yêu cầu trả hàng
1. Customer vào trang "My Orders", chọn đơn đã COMPLETED
2. Click "Return Request" → chọn lý do, chọn items muốn trả (có serial)
3. POST `/orders/{code}/return`
4. Backend tạo `returns` record với status=REQUESTED
5. Thông báo Admin/Staff

### 4.2 Duyệt trả hàng
1. Staff xem chi tiết return request
2. Kiểm tra điều kiện:
   - Trong thời gian bảo hành? (warranty_expire_date >= TODAY)
   - Lý do hợp lệ?
3. Nếu approve:
   - Change return.status = APPROVED
   - Gọi process:
     * Serial status từ SOLD → AVAILABLE (nếu còn tốt) hoặc DEFECTIVE
     * Inventories.quantity_on_hand += 1 (từng serial)
     * Stock movement type='RETURN'
     * Refund: gọi API payment gateway để hoàn tiền (có thể thủ công)
     * Return.status = REFUNDED/COMPLETED
4. Nếu reject: ghi lý do, thông báo customer.

---

## 5. Quy trình Voucher & Khuyến mãi

### 5.1 Tạo Voucher (Admin)
- Vào admin panel → Vouchers → Create
- Điền:
  - code (HUIT2026)
  - discount_type (PERCENT/FIXED)
  - discount_value
  - min_order_value
  - usage_limit (optional)
  - start_date, end_date
  - applicable_products/categories (optional)

### 5.2 Sử dụng Voucher (Customer)
1. Trên trang cart, nhập mã voucher
2. Gọi `/api/vouchers/validate`
   - Kiểm tra active, date range, min order value, user usage count
3. Nếu hợp lệ → apply vào cart, tính discount
4. Khi checkout, voucher_code được lưu vào `orders` và `voucher_usages`
5. Sau khi order COMPLETED, increment `vouchers.usage_count`

---

## 6. Quy trình Thanh toán (Payments)

### 6.1 Thanh toán COD
1. Customer chọn COD tại checkout
2. Order được tạo với `payment_method = COD`, `payment_status = PENDING`
3. Shipper giao hàng, thu tiền mặt
4. Staff cập nhật `payment_status = PAID` sau khi có tiền

### 6.2 Thanh toán Online (MOMO/VNPAY)
1. Customer chọn MOMO/VNPAY
2. POST `/orders` trả về `payment_url` (nếu chưa có, tạo transaction với gateway)
3. Redirect customer đến gateway
4. Customer xác nhận thanh toán
5. Gateway gọi webhook `/api/payments/webhook`
   - Xác thực signature
   - Kiểm tra trạng thái transaction
   - Update `orders.payment_status` và tạo `payments` record
   - Nếu PAID → có thể tự động update order.status sang CONFIRMED (hoặc deffer)

### 6.3 Webhook Security
- Mỗi gateway có cách sign khác nhau
- Backend verify signature trước khi process
- Log toàn bộ payload vào `payments.webhook_data` để debug/audit

---

## 7. Quy trình Warehouse Operations

### 7.1 Nhận hàng (Receiving)
Xem 2.1.

### 7.2 Picking (Lấy hàng)
1. System generates pick list cho đơn hàng (theo variant và serial)
2. Warehouse staff dùng scanner/tablet xem list
3. Scan từng serial, hệ thống verify:
   - Serial tồn tại?
   - Serial.status == AVAILABLE?
   - Serial.warehouse_id == đúng kho?
   - Nếu đúng → chuyển sang RESERVED (temporary) hoặc giữ AVAILABLE cho đến khi ship
4. Khi đủ, click "Complete Picking"

### 7.3 Packing & Shipping
- Như bước 8 của quy trình mua hàng.

---

## 8. Use Cases (Tóm tắt)

| Use Case | Actor | Mô tả |
|----------|-------|-------|
| UC-001 | Customer | Đăng ký tài khoản |
| UC-002 | Customer | Đăng nhập |
| UC-003 | Customer | Duyệt & Search sản phẩm |
| UC-004 | Customer | Thêm vào giỏ hàng |
| UC-005 | Customer | Áp dụng voucher |
| UC-006 | Customer | Checkout & tạo đơn hàng |
| UC-007 | Customer | Thanh toán online |
| UC-008 | Customer | Xem lịch sử đơn hàng |
| UC-009 | Customer | Yêu cầu trả hàng |
| UC-010 | Customer | Đánh giá sản phẩm |
| UC-011 | Admin | Quản lý sản phẩm (CRUD) |
| UC-012 | Admin | Quản lý danh mục, thương hiệu |
| UC-013 | Admin | Quản lý voucher |
| UC-014 | Admin | Xem & cập nhật đơn hàng |
| UC-015 | Admin | Phê duyệt trả hàng, hoàn tiền |
| UC-016 | Admin | Xem báo cáo doanh thu, tồn kho |
| UC-017 | Warehouse Staff | Nhận hàng nhập kho (scan IMEI) |
| UC-018 | Warehouse Staff | Xuất kho (picking & shipping) |
| UC-019 | Warehouse Staff | Kiểm kê, điều chỉnh tồn kho |
| UC-020 | Warehouse Staff | Chuyển kho giữa các kho |

---

## 9. Transaction Boundaries

Mỗi operation quan trọng cần chạy trong 1 transactionDB:

1. **Checkout** (`sp_CreateOrder`):
   - BEGIN TRAN
   - Check stock (SELECT ... FOR UPDATE/UPDLOCK nếu cần lock)
   - Insert order
   - Insert order_items
   - Update inventories (quantity_reserved +=)
   - Insert stock_movements
   - COMMIT (ROLLBACK nếu lỗi)

2. **ImportStock** (`sp_ImportStock`):
   - Insert product_serials
   - Update inventories
   - Insert stock_movement
   - Update cost_price

3. **ShipOrder** (`sp_ShipOrder`):
   - Check serials are RESERVED
   - Update serials to SOLD
   - Update inventories (quantity_reserved -=)
   - Update order status
   - Insert stock_movement type=SALE_SHIP

**Lưu ý:** SQL Server dùng `BEGIN TRANSACTION` và `TRY...CATCH` để handle lỗi.

---

## 10. Concurrency & Locking

**Vấn đề:** Khi 2 customer cùng mua 1 item, cả hai có thể check quantity và thấy còn hàng, dẫn đến overselling.

**Giải pháp:**

### Option 1: Pessimistic Lock (UPDLOCK, HOLDLOCK)
Trong transaction, lock row inventories trước khi check:

```sql
BEGIN TRANSACTION;
SELECT quantity_on_hand, quantity_reserved
FROM inventories WITH (UPDLOCK, HOLDLOCK)
WHERE warehouse_id = @WarehouseID AND variant_id = @VariantID;

IF (quantity_on_hand - quantity_reserved) < @RequestedQty
    ROLLBACK; -- insufficient stock
ELSE
    UPDATE inventories SET quantity_reserved = quantity_reserved + @RequestedQty
    ... continue
COMMIT;
```

### Option 2: Optimistic Concurrency (version column)
Thêm `row_version` TIMESTAMP/ROWVERSION vào bảng `inventories`. Khi update, kiểm tra row_version không đổi.

### Option 3: Application-level lock (Redis)
Dùng Redis lock (SETNX) để đảm bảo chỉ 1 process được modify inventory của 1 variant tại 1 thời điểm.

**Reccommend:** Dùng pessimistic lock trong stored procedure vì đơn giản và reliable với SQL Server.

---

## 11. Notifications & Events

Các sự kiện quan trọng cần thông báo (email/SMS/push):

- Order Confirmed → gửi email xác nhận
- Order Shipped → gửi tracking info
- Payment Success/Failed → gửi email
- Return Approved/Rejected → thông báo
- Low Stock Alert → gửi email cho warehouse manager
- New Order cho Staff → notify trong dashboard (có thể dùng SignalR)

Implementation:
- Backend publish event vào message queue (RabbitMQ) hoặc direct gọi email service.
- Để đơn giản, có thể gọi SMTP trực tiếp trong code (nhưng nên async).

---

## 12. Security Considerations

1. **Authentication:** JWT với short expiry (24h) + refresh token (30d)
2. **Authorization:** Role-based, check role trên từng endpoint
3. **Input Validation:** Validate mọi request (data annotations, FluentValidation)
4. **SQL Injection:** Dùng parameterized queries, EF Core
5. **XSS:** Sanitize user input khi hiển thị (dangerous HTML trong product description nên sanitize)
6. **CSRF:** Bảo vệ state-changing endpoints (dùng anti-forgery token)
7. **Rate Limiting:** Throttle API bằng middleware
8. **Secrets:** Không commit password, secret key → dùng environment variables
9. **Logging:** Log authentication events, payment events, admin actions
10. **Audit:** Dùng audit_logs để trace thay đổi dữ liệu

---

## 13. Multi-tenancy Considerations (Future)

Nếu muốn scale thành SaaS cho nhiều cửa hàng:
- Thêm `tenant_id` vào hầu hết các bảng (users, products, orders, inventories,...)
- Đảm bảo mọi query có filter theo `tenant_id`
- Có thể dùng Row Level Security (SQL Server) hoặc application-level filter
- Database-per-tenant nếu cần isolation cao

---

## 14. Reporting & BI

Các báo cáo thường xuyên:

1. **Daily Revenue** (theo ngày)
   - Doanh thu, số đơn, AOV, conversion rate
2. **Top Selling Products** (theo period)
3. **Inventory Aging** (hàng tồn kho lâu)
4. **Customer Retention** (repeat purchase rate)
5. **Sales by Warehouse**
   - So sánh performance giữa các kho

Implementation:
- Daily batch job tổng hợp vào bảng `daily_reports`
- Hoặc dùng view/vw và query trực tiếp (nếu data nhỏ)
- Kết nối BI tool (Power BI, Metabase) trực tiếp vào DB (readonly user)

---

## 15. Backup & Recovery

- **Database:** Full backup daily, differential backup every 4h, transaction log backup every 15min.
- **Point-in-time recovery:** có thể restore đến bất kỳ thời điểm nào.
- **Offsite backup:** Upload backup file to Azure Blob/S3.
- **Retention:** giữ 30 ngày.

---

## 16. Maintenance Tasks

Các cron jobs cần thiết:

| Task | Frequency | Mục đích |
|------|-----------|----------|
| Clean expired vouchers | Daily | Đặt is_active=0 nếu voucher hết hạn |
| Archive old orders | Monthly | Chuyển orders cũ (>2 năm) sang archive DB |
| Purge audit logs | Yearly | Xóa audit_logs cũ (>5 năm) |
| Recalculate inventory summary | Daily | Sync `vw_InventoryDashboard` cache nếu cần |
| Send daily reports | Daily morning | Gửi email báo cáo doanh thu hôm qua cho admin |

---

## 17. Error Handling & Idempotency

- POST endpoints phải **idempotent** khi có thể (dùng idempotency key để tránh double charge).
- Khi payment webhook gọi, nếu đã xử lý transaction_id → ignore.
- Retry logic cho các external calls (payment gateway, email) với exponential backoff.

---

## 18. Monitoring & Alerting

- **Application insights** ( hoặc Prometheus) theo dõi:
  - Request rate, latency, error rate
  - Database connection count
  - Queue depth (nếu dùng message queue)
- **Alerts:**
  - Error rate > 1%
  - Payment webhook failure
  - Disk space < 20%
  - API response time > 1s (p95)
  - Inventory sync lag

---

## Conclusion

Quy trình nghiệp vụ được thiết kế để:
- **Robust:** Có transaction, audit, error handling
- **Scalable:** Hỗ trợ multi-warehouse, serial tracking, role-based
- **Extensible:** Dễ thêm module mới (subscription, membership,...)

Đảm bảo tuân thủ quy trình trong implementation để tránh lỗi race condition, data inconsistency.

---

_Last updated: 2025-02-14_
