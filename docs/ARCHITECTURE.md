# Kiến trúc Hệ thống - ECommerce HUIT

Tài liệu mô tả chi tiết kiến trúc phần mềm của hệ thống e-commerce HUIT.

---

## 📋 Mục lục

1. [Tổng quan](#tổng-quan)
2. [Kiến trúc Layered (Clean Architecture)](#kiến-trúc-layered-clean-architecture)
3. [Domain Layer](#domain-layer)
4. [Application Layer](#application-layer)
5. [Infrastructure Layer](#infrastructure-layer)
6. [Presentation Layer](#presentation-layer)
7. [Luồng Dữ liệu](#luồng-dữ-liệu)
8. [Quyết định thiết kế](#quyết định-thiết-kế)
9. [Bảo mật](#bảo-mật)
10. [Khả năng mở rộng](#khả-năng-mở-rộng)

---

## Tổng quan

Hệ thống được thiết kế với nguyên tắc **Separation of Concerns** và **Dependency Rule** (theo Clean Architecture). Lớp trong cùng (Domain) không phụ thuộc lớp ngoài cùng (Infrastructure, Presentation).

### Tech Stack Summary

| Layer | Technology | Mục đích |
|-------|------------|----------|
| **Domain** | POCO Entities, Enums, Value Objects | Business logic core, không phụ thuộc framework |
| **Application** | .NET 8, CQRS (optional), MediatR (optional), FluentValidation | Use cases, DTOs, validation, service interfaces |
| **Infrastructure** | Entity Framework Core 8, SQL Server, Redis, SMTP, Stripe/Momo | Data persistence, external integrations |
| **Presentation** | ASP.NET Core Web API, Swagger | RESTful endpoints, documentation |

---

## Kiến trúc Layered (Clean Architecture)

```
┌──────────────────────────────────────────────┐
│        Presentation Layer (API)              │
│  • Controllers                               │
│  • Middleware                                │
│  • Filters, Formatters                      │
├──────────────────────────────────────────────┤
│        Application Layer                    │
│  • Services (Auth, Product, Order, Cart)   │
│  • DTOs                                      │
│  • Validators (FluentValidation)           │
│  • Interfaces                               │
├──────────────────────────────────────────────┤
│        Domain Layer                         │
│  • Entities                                 │
│  • Enums                                    │
│  • Value Objects (if needed)               │
│  • Domain Events (if needed)               │
├──────────────────────────────────────────────┤
│        Infrastructure Layer                │
│  • EF Core (DbContext, Configurations)     │
│  • External Services (Email, Payment)      │
│  • Repositories (optional)                 │
└──────────────────────────────────────────────┘
```

**Dependency Direction:** → outward (inner layers define interfaces, outer layers implement)

---

## Domain Layer

**Project:** `ECommerce.Huit.Domain`

### Entities

Là các POCO classes đại diện cho bảng trong database, không chứa business logic phức tạp, chỉ có properties và validation cơ bản.

**Danh sách Entities (26):**

- `User` - Người dùng (customer, admin, staff, warehouse)
- `Address` - Địa chỉ giao hàng/lưu trữ
- `Product` - Sản phẩm
- `Brand` - Thương hiệu
- `Category` - Danh mục (hierarchical)
- `ProductVariant` - Biến thể sản phẩm (SKU, size, color)
- `ProductImage` - Ảnh sản phẩm
- `ProductSerial` - Số serial/IMEI (cho hàng điện tử)
- `Warehouse` - Kho hàng
- `Inventory` - Tồn kho (per variant + warehouse)
- `Cart` - Giỏ hàng
- `CartItem` - Item trong giỏ
- `Order` - Đơn hàng
- `OrderItem` - Item trong đơn hàng
- `OrderItemSerial` - Mapping order_item → serial (nếu có)
- `OrderStatusHistory` - Lịch sử trạng thái đơn hàng
- `Voucher` - Mã giảm giá
- `VoucherUsage` - Lịch sử sử dụng voucher
- `Payment` - Thanh toán
- `Review` - Đánh giá sản phẩm
- `Return` - Yêu cầu trả hàng
- `ReturnItem` - Chi tiết trả hàng
- `SupportTicket` - Ticket hỗ trợ
- `Supplier` - Nhà cung cấp
- `StockMovement` - Nhật ký chuyển động kho
- `AuditLog` - Nhật ký audit (thay đổi dữ liệu)
- `Permission` - Quyền (ví dụ: products.read)
- `RolePermission` - Mapping role ↔ permission

### Enums (12)

- `UserRole`: ADMIN, STAFF, WAREHOUSE, CUSTOMER
- `UserStatus`: ACTIVE, BANNED
- `OrderStatus`: PENDING, CONFIRMED, SHIPPING, COMPLETED, CANCELLED, RETURNED
- `PaymentStatus`: PENDING, PAID, REFUNDED, FAILED
- `PaymentMethod`: CASH, COD, BANK_TRANSFER, MOMO, VNPAY, CREDIT_CARD
- `ProductStatus`: DRAFT, ACTIVE, HIDDEN, OUT_OF_STOCK
- `SerialStatus`: AVAILABLE, RESERVED, SOLD, RETURNED, DEFECTIVE
- `WarehouseType`: PHYSICAL, VIRTUAL, DROPSHIP
- `MovementType`: PURCHASE, SALE_RESERVED, SALE_SHIP, RETURN, TRANSFER_IN, TRANSFER_OUT, ADJUSTMENT_IN, ADJUSTMENT_OUT, INITIAL
- `ReturnStatus`: REQUESTED, APPROVED, REJECTED, RECEIVED
- `TicketStatus`: OPEN, IN_PROGRESS, RESOLVED, CLOSED
- `TicketPriority`: LOW, MEDIUM, HIGH, URGENT
- `DiscountType`: PERCENT, FIXED

### Value Objects (Optional)

Có thể thêm:
- `Money` (value object cho decimal + currency)
- `Email`
- `PhoneNumber`

---

## Application Layer

**Project:** `ECommerce.Huit.Application`

### Responsibilities

- Chứa **use cases** của ứng dụng
- **Không** chứa logic truy cập database trực tiếp
- Giao tiếp với Domain qua interfaces (Repository pattern)
- Validate input bằng FluentValidation
- Mapping DTO ↔ Entity (AutoMapper)

### Structure

```
Application/
├── Common/
│   ├── Interfaces/           # Service interfaces (IAuthService, IOrderService, etc.)
│   ├── Behaviors/            # Pipeline behaviors (logging, validation)
│   └── MappingProfiles/      # AutoMapper profiles
├── DTOs/
│   ├── Auth/                 # LoginDto, RegisterDto, AuthResponseDto
│   ├── Product/              # ProductListDto, ProductDetailDto, ProductQueryParams
│   ├── Cart/                 # CartDto, AddCartItemRequest
│   ├── Order/                # CreateOrderRequest, OrderResponseDto, UpdateOrderStatusRequest
│   ├── User/                 # UserDto, UpdateProfileRequest
│   ├── Voucher/              # VoucherDto, ApplyVoucherRequest
│   ├── Admin/                # InventoryDto, RevenueReportDto, LowStockAlertDto
│   └── Common/               # PaginationResponse<T>, ApiResponse<T>
├── Services/                 # Implementations (AuthService, ProductService, OrderService, CartService)
├── Validators/               # FluentValidation validators per DTO
│   ├── Auth/
│   ├── Product/
│   ├── Cart/
│   └── Order/
└── ECommerce.Huit.Application.csproj
```

### Key Services

#### `IAuthService`

```csharp
Task<AuthResponseDto?> RegisterAsync(RegisterDto dto);
Task<AuthResponseDto?> LoginAsync(LoginDto dto);
Task LogoutAsync(int userId);
Task<TokenResponse> RefreshAccessTokenAsync(string refreshToken);
```

#### `IProductService`

```csharp
Task<PaginationResponse<ProductListDto>> GetProductsAsync(ProductQueryParams query);
Task<ProductDetailDto?> GetProductByIdAsync(int id);
Task<IEnumerable<CategoryDto>> GetCategoriesAsync();
Task<IEnumerable<BrandDto>> GetBrandsAsync();
```

#### `ICartService`

```csharp
Task<CartDto> GetCartAsync(int userId);
Task<CartItemDto> AddItemAsync(int userId, AddCartItemRequest request);
Task RemoveItemAsync(int userId, int cartItemId);
Task ClearCartAsync(int userId);
Task<CartDto> ApplyVoucherAsync(int userId, string voucherCode);
```

#### `IOrderService`

```csharp
Task<OrderResponseDto> CreateOrderAsync(int userId, CreateOrderRequest request);
Task<OrderResponseDto?> GetOrderAsync(int userId, int orderId);
Task<IEnumerable<OrderResponseDto>> GetUserOrdersAsync(int userId);
Task UpdateOrderStatusAsync(int orderId, string status); // admin/staff
Task ConfirmOrderAsync(int orderId);
Task ShipOrderAsync(int orderId, string trackingNumber, string shippingProvider);
Task CompleteOrderAsync(int orderId);
Task CancelOrderAsync(int orderId, string reason);
Task ProcessReturnAsync(int returnId, string action, string adminNote); // approve/reject
```

#### `IInventoryService`

```csharp
Task ImportStockAsync(int supplierId, int warehouseId, List<ImportStockItem> items);
Task TransferStockAsync(int fromWarehouse, int toWarehouse, int variantId, int quantity);
Task AdjustInventoryAsync(int variantId, int warehouseId, int quantity, string reason);
Task<IEnumerable<InventoryDto>> GetAllInventoryAsync();
Task<LowStockReportDto> GetLowStockReportAsync(int threshold);
```

### Validation (FluentValidation)

Mỗi DTO có validator tương ứng:

```csharp
public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Họ tên không được để trống")
            .MaximumLength(100).WithMessage("Họ tên tối đa 100 ký tự");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email không được để trống")
            .EmailAddress().WithMessage("Email không hợp lệ")
            .MaximumLength(100);

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Số điện thoại không được để trống")
            .Matches(@"^[0-9]{10,15}$").WithMessage("Số điện thoại không hợp lệ");

        RuleFor(x => x.Password)
            .MinimumLength(6).WithMessage("Mật khẩu tối thiểu 6 ký tự")
            .MaximumLength(100)
            .Matches("[A-Z]").WithMessage("Mật khẩu phải chứa ít nhất 1 chữ hoa")
            .Matches("[0-9]").WithMessage("Mật khẩu phải chứa ít nhất 1 chữ số");
    }
}
```

---

## Infrastructure Layer

**Project:** `ECommerce.Huit.Infrastructure`

### Data (EF Core)

**DbContext:** `ApplicationDbContext`

```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Cart> Carts { get; set; }
    // ... other DbSets

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
```

### Configurations (Fluent API)

Mỗi entity có configuration class (ví dụ: `UserConfiguration`, `ProductConfiguration`) để define:

- Table name
- Column names, types, lengths, nullability
- Primary keys, composite keys
- Foreign keys
- Indexes
- Relationships (one-to-many, many-to-many)
- Check constraints
- Triggers (via `HasTrigger`)

**Ví dụ: OrderConfiguration**

```csharp
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Code).HasMaxLength(20).IsRequired();
        builder.Property(o => o.TotalAmount).HasPrecision(15, 2).IsRequired();
        builder.Property(o => o.Status).HasMaxLength(20).IsRequired();

        builder.HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Warehouse)
            .WithMany(w => w.Orders)
            .HasForeignKey(o => o.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

### External Services

**Redis:** Distributed cache cho session, rate limiting

**SMTP:** Gửi email (xác nhận đơn hàng, reset password)

**Payment Gateway:** MoMo, VNPAY (chưa implement)

---

## Presentation Layer

**Project:** `ECommerce.Huit.API`

### Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto) { ... }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto) { ... }
}

[Route("api/[controller]")]
public class ProductsController : BaseController
{
    private readonly IProductService _productService;

    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] ProductQueryParams query) { ... }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id) { ... }
}

[Authorize]
public class CartController : BaseController
{
    private readonly ICartService _cartService;

    [HttpGet]
    public async Task<IActionResult> GetCart() { ... }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem(AddCartItemRequest request) { ... }
}

[Authorize]
public class OrdersController : BaseController
{
    private readonly IOrderService _orderService;

    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderRequest request) { ... }

    [HttpGet]
    public async Task<IActionResult> GetMyOrders() { ... }

    [HttpGet("{orderCode}")]
    public async Task<IActionResult> GetOrder(string orderCode) { ... }
}
```

### Middleware

- **ErrorHandlingMiddleware** - Global exception handler
- **RequestLoggingMiddleware** - Log request/response
- **JwtMiddleware** - Validate JWT tokens
- **RateLimitingMiddleware** - Giới hạn request (optional)

### Authentication & Authorization

**JWT Bearer:**

```csharp
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});
```

**Role-based Authorization:**

```csharp
[Authorize(Roles = "ADMIN,STAFF")]
[HttpGet("admin/orders")]
public IActionResult GetAllOrders() { }
```

### Swagger / OpenAPI

Được cấu hình trong `Program.cs`:

```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ECommerce Huit API", Version = "v1" });
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        BearerFormat = "JWT",
        Scheme = "bearer",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Enter JWT Bearer token",
    };
    c.AddSecurityDefinition("JWT", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { securityScheme, Array.Empty<string>() } });
});
```

---

## Luồng Dữ liệu

### Ví dụ: Tạo đơn hàng (Create Order)

```
Client → API (POST /api/orders)
   ↓
[Auth] Middleware: Validate JWT → UserID
   ↓
OrdersController.CreateOrderAsync()
   ↓
IOrderService.CreateOrderAsync()
   ↓
Validate: cart items, stock availability
   ↓
Calculate: subtotal, discount, tax, shipping, total
   ↓
Call sp_CreateOrder (stored procedure)
   ↓
Inside SP:
  - Tạo Order (status=PENDING)
  - Tạo OrderItems
  - Reserve inventory (UPDATE inventories SET quantity_reserved += ...)
  - Ghi stock_movement (type=SALE_RESERVED)
  - Apply voucher
  - Clear cart
   ↓
Return OrderID, OrderCode
   ↓
Controller → 201 Created (OrderResponseDto)
```

### Luồng Xác nhận đơn hàng (Confirm)

```
Client → PUT /api/orders/{id}/confirm
   ↓
[Authorize(Roles=ADMIN,STAFF)]
   ↓
IOrderService.ConfirmOrderAsync()
   ↓
Call sp_ConfirmOrder
   ↓
Update orders.status = CONFIRMED
Insert order_status_history
   ↓
Return success
```

---

## Quyết định thiết kế

### 1. Tại sao dùng Stored Procedures cho critical operations?

- **Transaction consistency:** SP đảm bảo atomicity giữa nhiều bảng (orders, order_items, inventories)
- **Pessimistic locking:** `WITH (UPDLOCK, HOLDLOCK)` ngăn overselling khi nhiều user cùng mua
- **Performance:** SQL chạy server-side, giảm round-trip
- **Auditability:** SP có thể được version và rollback dễ

**Nhược điểm:** Khó test, cần deploy DB changes riêng.

### 2. Tại sao không dùng Repository Pattern với EF Core?

EF Core `DbContext` đã là Unit of Work + Repository abstraction. Thêm wrapper repository là redundant.

Sử dụng trực tiếp `DbContext` trong Infrastructure layer (trong Application.Services, inject `ApplicationDbContext` qua interface `IApplicationDbContext` nếu cần, hoặc inject trực tiếp).

### 3. Tại sao dùng FluentValidation thay DataAnnotations?

- Richer rule set (conditional validation, collection validation)
- Separation of concerns (DTO không bị pollute bởi attributes)
- Dependency injection trong validator (có thể inject services để validate cross-field)
- Better error messages với localization support

### 4. Tại sao DTO riêng biệt cho từng use case?

- **Security:** Không expose entity properties không cần thiết
- **Flexibility:** API contract không bị ràng buộc bởi DB schema
- **Versioning:** Có thể thêm fields mới mà không ảnh hưởng DB

### 5. Multi-warehouse Inventory

- Mỗi variant có nhiều inventory records (one per warehouse)
- Composite PK: (variant_id, warehouse_id)
- SP hiện tại hardcode `warehouse_id=1`. Cần mở rộng sau.

---

## Bảo mật

1. **Authentication:** JWTBearer với Access Token (24h) + Refresh Token (30d stored DB)
2. **Authorization:** Role-based + Permission checks (role_permissions table)
3. **Secrets:** Không hardcode secrets; dùng environment variables / Secret Manager
4. **Passwords:** Hash bằng BCrypt (hiện tại đang dùng Base64 placeholder → cần sửa)
5. **SQL Injection:** EF Core parameterized queries + SP (an toàn)
6. **XSS:** ASP.NET Core auto-encodes; thêm CSP headers
7. **Rate Limiting:** Sẽ implement middleware (chưa có)
8. **CORS:** Restrict to trusted origins (chưa config)
9. **HTTPS:** Enforce trong production
10. **Audit Log:** Bảng `audit_logs` ghi lại tất cả thay đổi dữ liệu

---

## Khả năng mở rộng

### Vertical Scaling (Scale Up)

- Tăng CPU/RAM cho server
- Tăngmax degree of parallelism cho SQL Server

### Horizontal Scaling (Scale Out)

**API Layer:**
- Deploy nhiều instances
- Load balancer (NGINX, HAProxy)
- Stateless: session trong Redis

**Database:**
- Read replicas (Always On Availability Groups)
- Sharding (advanced)

**Redis:**
- Cluster mode (sharding)
- Sentinel (high availability)

### Caching Strategy

- **Distributed Cache (Redis):** Session, rate limiting, hot products
- **In-Memory Cache:** Như MemoryCache cho configs
- **CDN:** Static assets (images, JS, CSS)

---

## Performance Considerations

1. **Database Indexes:** Đã tạo indexes trên các field hay query (user_id, status, created_at, SKU)
2. **Pagination:** Tất cả list endpoints đều support skip/take
3. **Projection:** SELECT chỉ lấy fields cần thiết (DTO mapping)
4. **Async All The Way:** Services và Controllers dùng async/await
5. **Connection Pooling:** EF Core tự động quản lý

---

## Monitoring & Observability

- **Structured Logging:** Serilog → Console + File + (optionally) ELK
- **Metrics:** Prometheus (endpoint /metrics)
- **Tracing:** OpenTelemetry (chưa implement)
- **Health Checks:** `/health`, `/ready` endpoints

---

## Future Enhancements

- [ ] Event-driven architecture (Domain Events → RabbitMQ/Kafka)
- [ ] CQRS + MediatR
- [ ] Multi-tenancy (if need SaaS)
- [ ] GraphQL API (alternative to REST)
- [ ] Real-time notifications (SignalR)
- [ ] Background jobs (Hangfire, Quartz.NET)
- [ ] API versioning (v1, v2)
- [ ] Feature flags (LaunchDarkly)
- [ ] Mobile API (GraphQL/REST)

---

**Last Updated:** 2025-03-03
