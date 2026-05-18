# 📋 KẾ HOẠCH PHÁT TRIỂN & TRIỂN KHAI HỆ THỐNG E-COMMERCE HUIT

Tài liệu này dùng để theo dõi tiến độ phát triển các giao diện và chức năng của dự án **E-Commerce HUIT** (ASP.NET MVC 5 + LINQ to SQL).

---

## 🛠️ PHÂN CHIA GIAI ĐOẠN PHÁT TRIỂN (ROADMAP)

```mermaid
gantt
    title Lộ trình Phát triển E-Commerce HUIT
    dateFormat  YYYY-MM-DD
    section Giai đoạn 1: Core System
    Hệ thống Kho & Tồn kho (Admin)         :active, des1, 2026-04-10, 2026-04-20
    Hệ thống Quản lý Người dùng (Admin)    :active, des2, 2026-04-12, 2026-04-20
    Đánh giá & Phản hồi (Admin & User)    :active, des3, 2026-04-15, 2026-04-21
    Quản lý Bảo hành (All)                :active, des4, 2026-04-18, 2026-04-21
    section Giai đoạn 2: User Access
    Đăng ký / Đăng nhập                   :des5, 2026-05-19, 2026-05-22
    Quản lý hồ sơ cá nhân                 :des6, 2026-05-23, 2026-05-25
    section Giai đoạn 3: UX & Catalog
    Bộ lọc sản phẩm & Tìm kiếm            :des7, 2026-05-26, 2026-05-29
    Giỏ hàng & Danh sách yêu thích         :des8, 2026-05-30, 2026-06-02
    section Giai đoạn 4: Admin Product
    Quản lý sản phẩm (Admin CRUD)          :des9, 2026-06-03, 2026-06-07
    section Giai đoạn 5: Transactions
    Áp dụng Khuyến mãi & Voucher          :des10, 2026-06-08, 2026-06-11
    Thanh toán (Checkout Flow)            :des11, 2026-06-12, 2026-06-16
    Quản lý Đơn hàng (Admin & User)       :des12, 2026-06-17, 2026-06-21
```

---

## 📝 DANH SÁCH CHI TIẾT CÁC NHIỆM VỤ (TASK LIST)

### 🟢 GIAI ĐOẠN 1: CORE SYSTEM & INFRASTRUCTURE (ĐÃ HOÀN THÀNH)

- [x] **1. Quản lý kho hàng (Admin)**
  - [x] Thiết kế Database cho các bảng `warehouses`, `inventories`, `product_serials`, `stock_movements`, `suppliers`
  - [x] Viết `IInventoryService` & `InventoryService` xử lý nhập/xuất kho, thống kê tồn kho nâng cao
  - [x] Tạo `InventoryController` phục vụ Admin
  - [x] Thiết kế view `Dashboard.cshtml` biểu diễn KPI, biểu đồ phân phối hàng hóa
  - [x] Thiết kế view `ReorderReport.cshtml` báo cáo hàng sắp hết thông minh (Urgent/Warning/OK)
  - [x] Thiết kế views nhập hàng (`Import.cshtml`), chuyển kho (`Transfer.cshtml`), lịch sử xuất nhập (`History.cshtml`)
  
- [x] **2. Quản lý người dùng (Admin)**
  - [x] Thiết kế Database cho các bảng `users`, `addresses`, `permissions`, `role_permissions`
  - [x] Viết `IUserService` & `UserService` hỗ trợ lọc nâng cao, log lịch sử hoạt động
  - [x] Tích hợp chức năng Khóa / Mở khóa hàng loạt (`BulkUpdateUserStatusAsync`)
  - [x] Tích hợp đổi phân quyền hàng loạt (`BulkUpdateUserRoleAsync`)
  - [x] Giao diện `IndexEnhanced.cshtml` tích hợp hộp kiểm chọn nhiều hàng loạt, bộ lọc thông minh
  - [x] View chi tiết hoạt động của người dùng (`Details.cshtml`)

- [x] **3. Đánh giá và phản hồi (Admin & User)**
  - [x] Thiết kế Database cho bảng `reviews` (Đánh giá) và `review_responses` (Admin phản hồi)
  - [x] Cài đặt `IReviewService` & `ReviewService` xử lý phê duyệt, gửi đánh giá, xếp hạng trung bình
  - [x] Sửa lỗi compiler và liên kết bảng trong file `HuitShopDB.designer.cs` cho thực thể `review_responses`
  - [x] Thiết kế view `Submit.cshtml` cho khách hàng viết review (tích hợp star rating, verify badge, upload tối đa 5 hình ảnh)
  - [x] Thiết kế view `Manage.cshtml` cho Admin kiểm duyệt và phản hồi bình luận trực quan

- [x] **4. Quản lý bảo hành**
  - [x] Thiết kế Database cho bảng bảo hành & chính sách
  - [x] Viết `IWarrantyService` & `WarrantyService` kiểm tra bảo hành qua Serial/IMEI, duyệt/từ chối yêu cầu
  - [x] Tạo `WarrantyController` với các action gửi claim và xử lý phía Admin
  - [x] Thiết kế view `Claim.cshtml` cho phép khách hàng tra cứu IMEI và điền đơn yêu cầu (Sửa chữa, Đổi mới, Hoàn tiền)
  - [x] Thiết kế view `MyClaims.cshtml` hiển thị tiến trình xử lý yêu cầu bảo hành dưới dạng timeline trực quan
  - [x] Thiết kế view `ManageClaims.cshtml` cho Admin phân công kỹ thuật viên, cập nhật trạng thái yêu cầu

---

### 🟡 GIAI ĐOẠN 2: HỆ THỐNG TRUY CẬP & ĐỊNH DANH (CHƯA TRIỂN KHAI)

- [ ] **5. Đăng ký / Đăng nhập**
  - [ ] Thiết kế và tạo `AuthController.cs` trong thư mục `Controllers`
  - [ ] Thiết kế View Đăng nhập (`Views/Auth/Login.cshtml`) giao diện hiện đại, responsive
  - [ ] Thiết kế View Đăng ký (`Views/Auth/Register.cshtml`) hỗ trợ validate mật khẩu mạnh, email, số điện thoại
  - [ ] Tích hợp cơ chế Authentication Cookie hoặc Session lưu trữ trạng thái đăng nhập
  - [ ] Thiết kế logic mã hóa mật khẩu sử dụng SHA-256 / BCrypt trong `AuthService`
  - [ ] Xử lý Phân quyền và Bảo mật (Authorize Attribute) phân chia Admin, Staff, Customer

- [ ] **6. Quản lý hồ sơ (User Profile)**
  - [ ] Bổ sung các action Profile vào `UserController` phục vụ Client
  - [ ] Thiết kế View `Views/User/Profile.cshtml` hiển thị thông tin cá nhân cơ bản (Họ tên, email, sđt, avatar)
  - [ ] Thiết kế View Thay đổi mật khẩu (`Views/User/ChangePassword.cshtml`)
  - [ ] Tích hợp tính năng Quản lý Sổ địa chỉ (`Addresses`) - cho phép thêm/sửa/xóa nhiều địa chỉ giao hàng
  - [ ] Hiển thị lịch sử hoạt động cá nhân của người dùng ngay trên trang cá nhân

---

### 🔵 GIAI ĐOẠN 3: DUYỆT SẢN PHẨM & TRẢI NGHIỆM DUYỆT WEB (CHƯA TRIỂN KHAI)

- [ ] **7. Tìm kiếm và bộ lọc**
  - [ ] Nâng cấp view `Views/Product/Index.cshtml` với giao diện Sidebar lọc sản phẩm hiện đại
  - [ ] Phát triển bộ lọc Danh mục sản phẩm (cây danh mục đa cấp dựa trên `parent_id`)
  - [ ] Phát triển bộ lọc Thương hiệu (Brand checklist) kết hợp bộ lọc giá bán (Price Range Slider sử dụng NoUiSlider)
  - [ ] Tích hợp tính năng Tìm kiếm nhanh (Instant Search) với gợi ý từ khóa AJAX
  - [ ] Tích hợp các bộ lọc sắp xếp (Mới nhất, Giá tăng dần, Giá giảm dần, Bán chạy nhất)
  
- [ ] **8. Giỏ hàng & Yêu thích (Cart & Wishlist)**
  - [ ] Xây dựng `CartController.cs` kết nối với `CartService` đã có
  - [ ] Thiết kế trang Giỏ hàng (`Views/Cart/Index.cshtml`) cho phép cập nhật số lượng trực tiếp (AJAX) và xóa sản phẩm
  - [ ] Thiết kế Drawer/Mini-cart trượt từ bên phải để tăng tính tương tác trên trang chủ
  - [ ] Phát triển tính năng Danh sách yêu thích (Wishlist) cho phép lưu sản phẩm yêu thích của User
  - [ ] Lưu trạng thái giỏ hàng vào Database đối với User đã đăng nhập, Cookie/Session đối với khách vãng lai

---

### 🟠 GIAI ĐOẠN 4: HỆ THỐNG QUẢN TRỊ SẢN PHẨM (CHƯA TRIỂN KHAI)

- [ ] **9. Quản lý sản phẩm (Admin - CRUD)**
  - [ ] Bổ sung khu vực quản trị Admin cho Products trong `ProductController`
  - [ ] Thiết kế View danh sách quản lý sản phẩm của Admin với tính năng lọc, phân trang
  - [ ] Thiết kế View Thêm mới sản phẩm (`Views/Product/Create.cshtml`) tích hợp chọn danh mục, thương hiệu, nhập specs dạng JSON
  - [ ] Thiết kế View Chỉnh sửa sản phẩm (`Views/Product/Edit.cshtml`)
  - [ ] Phát triển giao diện quản lý Biến thể (Variants) - cấu hình giá, SKU, kho hàng cho từng màu sắc/dung lượng sản phẩm
  - [ ] Tích hợp thư viện upload nhiều ảnh sản phẩm, sắp xếp vị trí hiển thị ảnh (`product_images`)

---

### 🔴 GIAI ĐOẠN 5: GIAO DỊCH THƯƠNG MẠI & ĐƠN HÀNG (CHƯA TRIỂN KHAI)

- [ ] **10. Thanh toán (Checkout Flow)**
  - [ ] Xây dựng quy trình thanh toán đa bước (Multi-step Checkout) trực quan:
    - *Bước 1*: Thông tin vận chuyển (Chọn từ Sổ địa chỉ hoặc nhập mới)
    - *Bước 2*: Lựa chọn hình thức thanh toán (COD, Chuyển khoản, Ví điện tử)
    - *Bước 3*: Đơn vị vận chuyển và Áp dụng mã giảm giá (Voucher)
    - *Bước 4*: Xác nhận đơn hàng & Tóm tắt chi phí (Tạm tính, Giảm giá, Phí ship, Tổng thanh toán)
  - [ ] Thiết kế View `Views/Cart/Checkout.cshtml` phục vụ quy trình này
  - [ ] Viết logic kiểm tra tồn kho tại thời điểm checkout, trừ lượng `quantity_on_hand` và tăng `quantity_reserved`
  - [ ] Mô phỏng Cổng thanh toán (VNPAY / MoMo / Stripe) phục vụ môi trường Demo

- [ ] **11. Quản lý đơn hàng**
  - [ ] Xây dựng `OrderController.cs` xử lý đơn hàng
  - [ ] Thiết kế trang Lịch sử mua hàng (`Views/Order/History.cshtml`) cho khách hàng xem danh sách đơn hàng đã mua
  - [ ] Thiết kế trang Chi tiết đơn hàng (`Views/Order/Details.cshtml`) kèm theo timeline trạng thái đơn hàng và thông tin serial/IMEI của sản phẩm được mua
  - [ ] Xây dựng trang Quản lý Đơn hàng cho Admin (`Views/Order/Manage.cshtml`)
    - Hỗ trợ đổi trạng thái đơn hàng (PENDING -> CONFIRMED -> PROCESSING -> SHIPPING -> COMPLETED/CANCELLED)
    - Cơ chế gán Serial Number cụ thể vào Order Item khi đóng gói hàng

- [ ] **12. Quản lý khuyến mãi (Promotion)**
  - [ ] Tạo `VoucherController.cs` phục vụ nghiệp vụ Admin & áp dụng Voucher
  - [ ] Thiết kế giao diện Quản lý Voucher cho Admin (`Views/Voucher/Index.cshtml`, `Create.cshtml`)
    - Cấu hình mã voucher, loại giảm giá (% hoặc số tiền), giá trị giảm, mức giảm tối đa, giá trị đơn hàng tối thiểu, ngày bắt đầu/kết thúc
  - [ ] Tích hợp API áp dụng voucher bằng AJAX ngay tại trang giỏ hàng và thanh toán
  - [ ] Cập nhật số lượng sử dụng voucher (`usage_count`) và ghi nhận vào bảng `voucher_usages` khi thanh toán thành công
