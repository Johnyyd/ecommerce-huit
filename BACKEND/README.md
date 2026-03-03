# Backend API - ECommerce HUIT

## 🛠️ Technology Stack

- **Framework:** ASP.NET Core 8.0 (or later)
- **Database:** Microsoft SQL Server 2022
- **ORM:** Entity Framework Core 8
- **Authentication:** JWT Bearer Tokens
- **Validation:** FluentValidation
- **Logging:** Serilog
- **Caching:** Redis
- **Testing:** xUnit, Moq
- **Documentation:** Swagger/OpenAPI

---

## 📁 Project Structure

```
BACKEND/
├── src/
│   └── ECommerce.Huit.API/          # Main Web API project
│       ├── Controllers/              # API endpoints
│       │   ├── AuthController.cs
│       │   ├── UsersController.cs
│       │   ├── ProductsController.cs
│       │   ├── CartController.cs
│       │   ├── OrdersController.cs
│       │   ├── AdminController.cs
│       │   └── VoucherController.cs
│       ├── Services/                 # Business logic layer
│       │   ├── IOrderService.cs
│       │   ├── OrderService.cs
│       │   ├── IInventoryService.cs
│       │   ├── InventoryService.cs
│       │   ├── IVoucherService.cs
│       │   └── ...
│       ├── Data/                     # Entity Framework
│       │   ├── ApplicationDbContext.cs
│       │   ├── Configurations/       # Entity configurations (Fluent API)
│       │   └── Migrations/           # EF Core migrations
│       ├── Models/                   # DTOs, ViewModels, Entities
│       │   ├── DTOs/
│       │   ├── ViewModels/
│       │   └── Entities/             # POCO classes (reuse from DB schema)
│       ├── Middleware/
│       │   ├── ErrorHandlingMiddleware.cs
│       │   ├── JwtMiddleware.cs
│       │   └── RateLimitMiddleware.cs
│       ├── Validators/               # FluentValidation validators
│       ├── Utilities/
│       │   ├── JwtHelper.cs
│       │   ├── PasswordHasher.cs
│       │   └── FileHelper.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       └── Program.cs
├── tests/
│   ├── UnitTests/
│   │   ├── Services/
│   │   └── Validators/
│   ├── IntegrationTests/
│   │   └── Controllers/
│   └── TestHelpers/
├── docker-compose.yml (backend part)
├── .env.example
└── README.md (this file)
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server 2019+ (or Docker)
- Redis (optional for caching)

### 1. Clone and Navigate

```bash
cd BACKEND/src/ECommerce.Huit.API
```

### 2. Configure appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=HuitShopDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true"
  },
  "Jwt": {
    "Key": "YOUR_SUPER_SECRET_KEY_MIN_32_CHARS_CHANGE_THIS",
    "Issuer": "ECommerceHuit",
    "Audience": "ECommerceHuitClient",
    "DurationInMinutes": 1440
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "Username": "your_email@gmail.com",
    "Password": "your_app_password",
    "From": "noreply@huit.edu.vn"
  },
  "Payment": {
    "Momo": {
      "MerchantId": "...",
      "AccessKey": "...",
      "SecretKey": "..."
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**Don''t commit real secrets!** Use User Secrets or environment variables in production.

### 3. Install Dependencies

```bash
dotnet restore
```

### 4. Run Migrations (Optional - if using EF migrations)

```bash
dotnet ef database update
```

Or just run the init.sql script directly (recommended for initial setup):

```bash
sqlcmd -S localhost -U SA -P 'YourStrong@Passw0rd' -i ../../DATABASE/init.sql
sqlcmd -S localhost -U SA -P 'YourStrong@Passw0rd' -i ../../DATABASE/seed.sql
```

### 5. Run the API

```bash
dotnet run
```

API sẽ chạy tại `https://localhost:5001` và `http://localhost:5000`

### 6. Test API với Swagger

Mở trình duyệt: `https://localhost:5001/swagger`

---

## 🔐 Authentication Flow

1. **Register:** `POST /api/auth/register`
2. **Login:** `POST /api/auth/login` → trả về `access_token` và `refresh_token`
3. **Sử dụng API:** Thêm header `Authorization: Bearer {access_token}`
4. **Refresh token:** `POST /api/auth/refresh-token` khi access_token hết hạn
5. **Logout:** `POST /api/auth/logout` (xóa refresh token)

---

## 📦 Key Services

### OrderService
- `CreateOrderAsync()` – tạo đơn hàng, reserve inventory
- `ConfirmOrderAsync()` – xác nhận đơn
- `ShipOrderAsync()` – xuất kho, gán serials, chuyển sang SHIPPING
- `CompleteOrderAsync()` – hoàn tất
- `CancelOrderAsync()` – hủy, hoàn trả tồn kho

**Important:** All order operations are wrapped in transactions.

### InventoryService
- `ImportStockAsync()` – nhập kho với serial tracking
- `TransferStockAsync()` – chuyển kho
- `AdjustStockAsync()` – điều chỉnh thủ công
- `GetLowStockReportAsync()` – báo cáo hàng sắp hết

### VoucherService
- `ValidateVoucherAsync()` – kiểm tra voucher hợp lệ
- `ApplyVoucherAsync()` – áp dụng vào giỏ hàng
- `RedeemVoucherAsync()` – đổi khi checkout

---

## 🔄 Database Stored Procedures

Backend nên gọi stored procedures thay vì raw query để đảm bảo consistency với business logic:

- `sp_CreateOrder`
- `sp_ImportStock`
- `sp_ShipOrder`
- `sp_CompleteOrder`
- `sp_CancelOrder`
- `sp_ProcessReturn`
- `sp_GetRevenueReport`
- `sp_GetLowStockReport`

Có thể wrapper trong C# qua `DbContext.Database.ExecuteSqlRawAsync()`.

---

## 🧪 Testing

### Unit Tests
```bash
cd tests/UnitTests
dotnet test
```

Mocks cho DbContext, repositories.

### Integration Tests
```bash
cd tests/IntegrationTests
dotnet test
```

Sử dụng Testcontainers để chạy SQL Server & Redis trong Docker.

---

## 📝 API Documentation

Xem file `docs/API.md` trong root repository.

Swagger UI sẽ được generate tự động từ controller attributes.

---

## 🏗 Architecture Layers

```
Controller (thin) → Service (business logic) → Repository/EF Core → Database
      ▲                     ▲                       ▲
   Validate            Transactions              Stored
   Authorize             Rules                   Procedures
```

**Guideline:**
- Controllers chỉ responsibilities: authenticate/authorize, validate input, call service, return result.
- Business logic (transaction, complex calc) nằm trong Service layer.
- Data access nên dùng stored procedures cho critical operations (order, inventory) để đảm bảo consistency.

---

## 🔧 Common Tasks

### Add New API Endpoint

1. Tạo DTO (request/response) trong `Models/DTOs/`
2. Tạo Validator (FluentValidation) trong `Validators/`
3. Thêm method vàoService interface & implementation
4. Thêm action vào Controller với `[HttpPost]`, `[Authorize(Roles="...")]`
5. Thêm route vào `Program.cs` nếu cần
6. Update `docs/API.md`

### Add New Database Table

1. Update `init.sql` → tạo bảng
2. Tạo entity class `Models/Entities/YourTable.cs`
3. (Optional) tạo repository interface/class
4. Thêm seed data vào `seed.sql`
5. Update ERD diagram in docs

---

## 🐛 Debugging

- Dùng **Visual Studio 2022** hoặc **VS Code** với C# extension
- Set breakpoint trong code
- Logging: Serilog writes to console & file (logs/ folder)
- Kiểm tra SQL queries bằng **SQL Server Profiler** hoặc **Extended Events**

---

## 🚢 Deployment

### Docker

```bash
docker build -t ecommerce-huit-api .
docker run -p 5000:80 -e ConnectionStrings__DefaultConnection="..." ecommerce-huit-api
```

Dockerfile được cung cấp trong repo.

### Azure App Service

- Publish từ Visual Studio
- Hoặc GitHub Actions (CI/CD)
- Set environment variables trong portal

---

## 🧹 Code Quality

- Run `dotnet format` trước commit
- Analyze với `dotnet analyze`
- Coveragetarget: >80% (code coverage)

---

## ❓ Questions?

Tạo issue trong repo hoặc liên hệ maintainer.

---

## 🙏 Thank You

Happy coding! May your merges be conflict-free 🚀
