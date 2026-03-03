# Backend API - ECommerce HUIT

Hệ thống backend cho trang web bán hàng điện tử HUIT, được xây dựng với ASP.NET Core 8.0 và SQL Server.

---

## 🏗️ Kiến trúc

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                       │
│              Controllers (API Endpoints)                   │
├─────────────────────────────────────────────────────────────┤
│                    Application Layer                       │
│        Services, DTOs, Validators, MediatR                │
├─────────────────────────────────────────────────────────────┤
│                    Domain Layer                            │
│          Entities, Enums, Value Objects, Interfaces       │
├─────────────────────────────────────────────────────────────┤
│                    Infrastructure Layer                   │
│    EF Core, Repositories, Redis, External Services        │
└─────────────────────────────────────────────────────────────┘
```

---

## 📦 Công nghệ sử dụng

| Công nghệ | Version | Mục đích |
|-----------|---------|----------|
| .NET | 8.0 | Framework |
| ASP.NET Core | 8.0 | Web API |
| Entity Framework Core | 8.0 | ORM |
| SQL Server | 2019+ | Database |
| JWTBearer | 8.0 | Authentication |
| FluentValidation | 11.x | Validation |
| Swagger/OpenAPI | 6.x | API Documentation |
| Serilog | 8.x | Logging |
| Redis | 2.7.x | Caching/Session |

---

## 🗂️ Cấu trúc thư mục

```
BACKEND/
├── src/
│   ├── ECommerce.Huit.Domain/          # Domain layer
│   │   ├── Entities/                   # POCO entities
│   │   ├── Enums/                      # Enumeration types
│   │   └── ValueObjects/               # Value objects (if any)
│   ├── ECommerce.Huit.Application/     # Application layer
│   │   ├── Common/
│   │   │   ├── Interfaces/             # Service interfaces
│   │   │   └── MappingProfiles/        # AutoMapper profiles
│   │   ├── DTOs/                       # Data Transfer Objects
│   │   │   ├── Auth/
│   │   │   ├── Cart/
│   │   │   ├── Order/
│   │   │   ├── Product/
│   │   │   ├── User/
│   │   │   ├── Voucher/
│   │   │   └── Admin/
│   │   ├── Services/                   # Business logic implementations
│   │   ├── Validators/                 # FluentValidation validators
│   │   └── ECommerce.Huit.Application.csproj
│   ├── ECommerce.Huit.Infrastructure/  # Infrastructure layer
│   │   ├── Data/
│   │   │   ├── Configurations/         # EF Core Fluent API configs
│   │   │   └── ApplicationDbContext.cs
│   │   ├── Services/                   # External service integrations
│   │   └── ECommerce.Huit.Infrastructure.csproj
│   └── ECommerce.Huit.API/             # Presentation layer
│       ├── Controllers/
│       ├── Middleware/
│       ├── Program.cs
│       ├── appsettings.json
│       └── ECommerce.Huit.API.csproj
├── tests/
│   ├── UnitTests/
│   ├── IntegrationTests/
│   └── TestHelpers/
├── docker-compose.yml (backend part)
├── .env.example
└── README.md (this file)
```

---

## 🚀 Cài đặt và chạy

### Yêu cầu

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server 2019+ (hoặc dùng Docker)
- Redis (tuỳ chọn)

### 1. Clone repository

```bash
git clone https://github.com/Johnyyd/ecommerce-huit.git
cd ecommerce-huit/BACKEND
```

### 2. Restore dependencies

```bash
cd src
dotnet restore
```

### 3. Cấu hình connection string

Sao chép `appsettings.example.json` thành `appsettings.json` trong thư mục `ECommerce.Huit.API`:

```bash
cd ECommerce.Huit.API
cp appsettings.example.json appsettings.json
```

Chỉnh sửa `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=HuitShopDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true"
  },
  "Jwt": {
    "Key": "YOUR_RANDOM_64_CHARACTER_SECRET_KEY_CHANGE_THIS",
    "Issuer": "ECommerceHuit",
    "Audience": "ECommerceHuitClient",
    "DurationInMinutes": 1440
  }
}
```

### 4. Tạo database

Chạy script SQL:

```bash
# Sử dụng sqlcmd
sqlcmd -S localhost -U SA -P 'YourStrong@Passw0rd' -i ../../DATABASE/init.sql
sqlcmd -S localhost -U SA -P 'YourStrong@Passw0rd' -i ../../DATABASE/seed.sql
```

Hoặc dùng Docker Compose (đã có trong root):

```bash
cd ../../
docker-compose up -d sqlserver
# Sau đó chạy init.sql thủ công
```

### 5. Chạy ứng dụng

```bash
cd src/ECommerce.Huit.API
dotnet run
```

API sẽ chạy tại:
- **HTTPS:** https://localhost:5001
- **HTTP:** http://localhost:5000

### 6. Test API với Swagger

Mở trình duyệt: https://localhost:5001/swagger

---

## 🔐 Authentication

API sử dụng JWT Bearer Token.

### Đăng ký
```
POST /api/auth/register
Content-Type: application/json

{
  "full_name": "Nguyen Van A",
  "email": "a@example.com",
  "phone": "0909123456",
  "password": "StrongPass123"
}
```

### Đăng nhập
```
POST /api/auth/login
{
  "email": "a@example.com",
  "password": "StrongPass123"
}
```

### Sử dụng token
Thêm header:
```
Authorization: Bearer {access_token}
```

---

## 📝 API Endpoints

### Public (không cần authenticate)
- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/products` - Danh sách sản phẩm
- `GET /api/products/{id}` - Chi tiết sản phẩm
- `GET /api/products/categories` - Danh mục
- `GET /api/products/brands` - Thương hiệu

### Authenticated (cần JWT)
- `GET /api/cart` - Giỏ hàng
- `POST /api/cart/items` - Thêm vào giỏ
- `POST /api/orders` - Tạo đơn hàng
- `GET /api/orders` - Lịch sử đơn hàng
- `GET /api/orders/{code}` - Chi tiết đơn

### Admin/Staff (cần role ADMIN/STAFF)
- `GET /admin/orders` - Tất cả đơn hàng
- `PUT /admin/orders/{id}/status` - Cập nhật trạng thái
- `POST /admin/inventory/import` - Nhập kho
- `GET /admin/reports/revenue` - Báo cáo doanh thu
- `GET /admin/inventory` - Xem tồn kho

Xem chi tiết trong [`docs/API.md`](../../docs/API.md).

---

## 🧪 Testing

### Unit Tests
```bash
cd tests/UnitTests
dotnet test
```

### Integration Tests
```bash
cd tests/IntegrationTests
dotnet test
```

---

## 🐛 Debugging

### Enable EF Core logging
Trong `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

### View SQL queries
Logging sẽ hiển thị SQL queries trong console khi chạy development.

---

## 🚢 Deployment

### Docker

Build image:
```bash
docker build -t ecommerce-huit-api -f src/ECommerce.Huit.API/Dockerfile .
```

Run:
```bash
docker run -p 5000:80 \
  -e ConnectionStrings__DefaultConnection="..." \
  -e Jwt__Key="..." \
  ecommerce-huit-api
```

### Environment Variables

Thay vì dùng `appsettings.json`, có thể dùng environment variables:

- `ConnectionStrings__DefaultConnection`
- `Jwt__Key`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Redis__ConnectionString`

---

## 📊 Database Stored Procedures

Backend có thể gọi trực tiếp stored procedures:

```csharp
// Ví dụ: tạo đơn hàng
var orderId = await _db.Database.ExecuteSqlRawAsync(
    "EXEC sp_CreateOrder @UserID={0}, @ShippingAddress={1}, @PaymentMethod={2}, @OrderItemsJSON={3}",
    userId, addressJson, paymentMethod, itemsJson);
```

Các SP chính:
- `sp_CreateOrder`
- `sp_ImportStock`
- `sp_ShipOrder`
- `sp_CompleteOrder`
- `sp_CancelOrder`
- `sp_GetRevenueReport`
- `sp_GetLowStockReport`

---

## 🏗️ Build & publish

### Create production build
```bash
dotnet publish -c Release -o ./publish
```

### Create self-contained deployment (no .NET runtime needed on server)
```bash
dotnet publish -c Release -r win-x64 --self-contained true
# hoặc linux-x64, osx-x64
```

---

## 🔧 Thiết lập quyền nâng cao (RBAC)

Bảng `permissions` và `role_permissions` đã được seed trong `init.sql`.

Mặc định:
- **ADMIN:** all permissions
- **STAFF:** products.read, orders.read, orders.update, inventory.read, reports.read
- **WAREHOUSE:** products.read, inventory.read, inventory.import, inventory.transfer
- **CUSTOMER:** không có permissions đặc biệt

Để kiểm tra permission trong code:

```csharp
[Authorize(Roles = "ADMIN,STAFF")]
[HttpPut("status")]
public async Task<IActionResult> UpdateStatus(int orderId, [FromBody] UpdateStatusRequest request)
{
    // ...
}
```

---

## 📈 Logging & Monitoring

Serilog được cấu hình trong `appsettings.json`:

- Ghi log ra Console
- Ghi log vào file `logs/log-.txt` (rollover daily)
- Enrich with `FromLogContext`

Production nên dùng:
- Application Insights (Azure)
- ELK Stack (Elasticsearch, Logstash, Kibana)
- Prometheus + Grafana

---

## 🧹 Code Style

- **Naming conventions:**
  - PascalCase cho classes, methods, properties
  - camelCase cho local variables, parameters
  - _camelCase cho private fields
  - I prefix cho interfaces

- **Async suffix:** Async cho methods trả về Task
- **Nullability:** Enable nullable reference types
- **XML comments:** Thêm comment cho public members

---

## ❓ Hỗ trợ

- Tạo issue trong GitHub repository
- Tham khảo tài liệu trong `docs/`
- Kiểm tra Swagger UI khi chạy local

---

**Happy coding! 🚀**
