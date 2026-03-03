# Khắc phục sự cố - ECommerce HUIT

Hướng dẫn xử lý các lỗi thường gặp trong quá trình phát triển và triển khai.

---

## 🐛 Lỗi phổ biến

### 1. Database Connection Failures

**Lỗi:** `Cannot connect to SQL Server` / `A network-related or instance-specific error`

**Nguyên nhân:**
- SQL Server service không chạy
- Sai connection string
- Port 1433 bị blocked bởi firewall
- SQL Server không cho phép remote connections

**Giải pháp:**

```bash
# Kiểm tra SQL Server container/service đang chạy
docker ps | grep sqlserver
# hoặc
systemctl status mssql-server

# Test connection locally
telnet localhost 1433
# hoặc
/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P 'YourStrong@Passw0rd' -Q "SELECT @@VERSION"

# Nếu dùng Docker, check logs
docker logs ecommerce-huit-sqlserver

# Verify connection string trong appsettings.json
ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=HuitShopDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true"
```

**Note:** `TrustServerCertificate=true` cần thiết cho dev self-signed cert. Trong production, dùng certificate hợp lệ.

---

### 2. Migration Failures

**Lỗi:** `Unable to create an object of type` / `The entity type requires a primary key`

**Nguyên nhân:**
- Entity không khai báo `[Key]` hoặc `Id` property
- Composite key chưa được cấu hình đúng bằng Fluent API

**Giải pháp:**

Kiểm tra entity class:

```csharp
public class OrderItem
{
    public int Id { get; set; } // ✅ Phải có primary key
    public int OrderId { get; set; }
    public int VariantId { get; set; }
    // ...
}
```

Nếu composite key (như Inventory), cấu hình trong `Configuration.cs`:

```csharp
builder.HasKey(i => new { i.VariantId, i.WarehouseId });
```

---

### 3. Stored Procedure Not Found

**Lỗi:** `Could not find stored procedure 'sp_CreateOrder'`

**Nguyên nhân:**
- Chưa chạy `init.sql` hoặc `sp_complete.sql`
- Database name khác với connection string
- Chạy script trên wrong database

**Giải pháp:**

```bash
# Connect to database
/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P 'password' -d HuitShopDB

# List stored procedures
SELECT name FROM sys.procedures;

# Nếu không thấy, chạy lại init.sql
/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P 'password' -i DATABASE/init.sql
```

---

### 4. Insufficient Stock Error

**Lỗi:** `The reserve quantity exceeds available quantity`

**Nguyên nhân:**
- Nhiều user cùng lúc đặt hàng (race condition)
- Inventory `quantity_on_hand` thấp hơn `quantity_reserved` do SP lỗi

**Giải pháp:**

Kiểm tra inventory:

```sql
SELECT v.sku, i.quantity_on_hand, i.quantity_reserved
FROM inventories i
JOIN product_variants v ON i.variant_id = v.id
WHERE v.sku = 'YOUR_SKU';

-- Fix: Reset quantity_reserved nếu cần
UPDATE inventories
SET quantity_reserved = 0
WHERE variant_id = (SELECT id FROM product_variants WHERE sku = 'YOUR_SKU');
```

**Prevention:** SP đã dùng `UPDLOCK, HOLDLOCK` để lock row. Đảm bảo transaction isolation level >= READ COMMITTED (default).

---

### 5. JWT Token Invalid / Expired

**Lỗi:** `401 Unauthorized` / `The token is expired`

**Nguyên nhân:**
- Token hết hạn (default 24h)
- Clock skew (server time khác client)
- Wrong signing key

**Giải pháp:**

Client gọi refresh endpoint:

```bash
curl -X POST "https://api.huit.com/api/auth/refresh" \
  -H "Content-Type: application/json" \
  -d '{"refresh_token":"your_refresh_token"}'
```

**Server config check:**

```csharp
options.TokenValidationParameters = new TokenValidationParameters
{
    ValidateLifetime = true,
    ClockSkew = TimeSpan.Zero, // optional: reduce grace period
    // ...
};
```

---

### 6. FluentValidation Not Triggered

**Lỗi:** Validation không chạy, invalid data vẫn pass

**Nguyên nhân:**
- Chưa register FluentValidation services
- Controller không có `[ApiController]` attribute
- Custom validator không đúng namespace

**Giải pháp:**

Trong `Program.cs`:

```csharp
builder.Services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();
// hoặc từ Application assembly
builder.Services.AddValidatorsFromAssembly(typeof(RegisterDtoValidator).Assembly);

builder.Services.AddControllers()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Program>());
```

---

### 7. Docker Networking Issues

**Lỗi:** `No such host: sqlserver` / backend không connect được database

**Nguyên nhân:**
- Service name trong `docker-compose.yml` khác
- Backend chạy trước database (race condition)

**Giải pháp:**

`docker-compose.yml`:

```yaml
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: ecommerce-sqlserver
    networks:
      - ecommerce-network
    # ...

  backend:
    build:
      context: ./BACKEND/src/ECommerce.Huit.API
    networks:
      - ecommerce-network
    environment:
      ConnectionStrings__DefaultConnection: "Server=sqlserver,1433;..."
    depends_on:
      - sqlserver
    # ...

networks:
  ecommerce-network:
    driver: bridge
```

---

### 8. Redis Connection Timeout

**Lỗi:** `No connection could be made because the target machine actively refused it.`

**Giải pháp:**

```csharp
// Trong Program.cs, configuration
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "ECommerceHuit:";
});
```

Test connectivity:

```bash
redis-cli -h localhost -p 6379 ping
# Should return PONG
```

---

### 9. EF Core Lazy Loading Infinite Loop

**Lỗi:** `JsonException` / StackOverflow khi serialize entity với circular reference

**Nguyên nhân:**
- Entity có navigation property tham chiếu ngược lại (User.Orders, Order.User)
- JSON serializer tự động serialize cả hai chiều

**Giải pháp:**

**Option 1: Dùng DTO** (recommended) - Never serialize entities directly.

**Option 2: Ignore navigation properties:**

```csharp
public class Order
{
    public int Id { get; set; }
    // ...

    [JsonIgnore]
    public User User { get; set; }
}
```

**Option 3: Configure JsonSerializerOptions:**

```csharp
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});
```

---

### 10. File Upload fails (ASP.NET Core)

**Lỗi:** `Request body too large` / `Multipart body length limit exceeded`

**Giải pháp:**

```csharp
// Program.cs
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50MB
});
// Hoặc dùng attribute trên controller
[RequestSizeLimit(50_000_000)]
[RequestFormLimits(MultipartBodyLengthLimit = 50_000_000)]
```

---

### 11. CORS Errors

**Lỗi:** `Access-Control-Allow-Origin` header missing

**Giải pháp:**

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("https://app.huit.com", "https://admin.huit.com")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

app.UseCors("AllowFrontend");
```

---

### 12. SQL Deadlocks

**Lỗi:** `Transaction (Process ID ...) was deadlocked on lock resources`

**Nguyên nhân:**
- Nhiều transactions cùng access相同的 resources với thứ tự khác nhau
- Lock escalation

**Giải pháp:**

- Luôn giảm transaction scope (chỉ lock những gì cần)
- Đảm bảo access tables theo thứ tự nhất quán (ví dụ: luôn `products` trước `inventories`)
- Điều chỉnh isolation level nếu cần (READ COMMITTED thường đủ)
- Retry logic với exponential backoff

```csharp
public async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, int maxRetries = 3)
{
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            return await operation();
        }
        catch (SqlException ex) when (ex.Number == 1205) // deadlock
        {
            if (i == maxRetries - 1) throw;
            await Task.Delay(100 * (int)Math.Pow(2, i));
        }
    }
    throw new InvalidOperationException();
}
```

---

### 13. Docker Build Slow / Out of Memory

**Lỗi:** `Killed` during build / `no space left on device`

**Giải pháp:**

- Clean up unused Docker objects:
  ```bash
  docker system prune -a --volumes
  ```
- Build với `--no-cache` nếu cần rebuild từ scratch
- Tăng swap space trên host

---

### 14. SSL Certificate Error in Development

**Lỗi:** `The SSL connection could not be established`

**Giải pháp:**

Trust dev certificate:

```bash
dotnet dev-certs https --trust
```

Hoặc tắt HTTPS verification (chỉ dev):

```bash
export DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0
# hoặc trong launchSettings.json: "sslPort": 0
```

---

## 🐘 Debugging Tips

### Enable EF Core Logging

`appsettings.Development.json`:

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

Console sẽ hiển thị tất cả SQL queries.

---

### Inspect SQL Generated

```csharp
var sql = context.Orders.Where(o => o.Id == 1).ToQueryString();
Console.WriteLine(sql);
```

---

### Use SQL Server Profiler / Extended Events

Để xem SP calls, execution plans, deadlocks.

---

## 📈 Performance Issues

### Slow Query

Chạy `SET STATISTICS IO, TIME ON;` trước query trong SSMS.

Kiểm tra execution plan, thiếu indexes:

```sql
SELECT * FROM sys.dm_db_missing_index_details;
```

---

### High Memory Usage

Kiểm tra Redis cache hit rate:
```bash
redis-cli info stats | grep keyspace_hits
redis-cli info stats | grep keyspace_misses
```

Tweak eviction policy: `maxmemory-policy allkeys-lru`

---

## 🧪 Testing Issues

### Integration Tests Fail: Database Already Exists

```csharp
options.UseInMemoryDatabase(Guid.NewGuid().ToString());
// hoặc xóa database trước mỗi test
context.Database.EnsureDeleted();
```

---

## 📦 Dependency Issues

**Lỗi:** `Package not found` / `NU1101`

**Giải pháp:**

Clear NuGet cache:
```bash
dotnet nuget locals all --clear
```

Update package sources:
```bash
dotnet restore --source https://api.nuget.org/v3/index.json
```

---

## 🔐 Security Checks

### 1. Verify JWT Validation

```bash
# Decode JWT (no verification)
curl -H "Authorization: Bearer {token}" https://jwt.io
```

Check signature matches your Jwt:Key.

### 2. Test RBAC

Login với role khác nhau, thử access:
- `GET /admin/orders` với user thường → 403
- `PUT /admin/orders/1/ship` với role STAFF → 200

---

## 📊 Monitoring Issues

### Health Check Endpoint Returns 503

Kiểm tra dependencies trong health check:

```csharp
public class HealthCheckService : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            return HealthCheckResult.Healthy("Database connected");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database connection failed", ex);
        }
    }
}
```

---

## 🗄️ Database Recovery

### Restore từ Backup

```sql
RESTORE DATABASE HuitShopDB_Dev
FROM DISK = '/var/opt/mssql/backup/HuitShopDB_full_20250303.bak'
WITH MOVE 'HuitShopDB_Data' TO '/var/opt/mssql/data/HuitShopDB_Dev.mdf',
MOVE 'HuitShopDB_Log' TO '/var/opt/mssql/data/HuitShopDB_Dev_log.ldf',
RECOVERY, REPLACE;
```

---

## 🚨 Emergency Checklist

**API Down:**
- [ ] Check server resources (CPU, RAM, Disk)
- [ ] Check Docker container logs
- [ ] Check database connectivity
- [ ] Rollback to previous image nếu vừa deploy
- [ ] Check SSL certificate expiry

**High Error Rate:**
- [ ] Check Serilog logs cho exceptions
- [ ] Monitor Prometheus metrics (500 errors spike)
- [ ] Database connection pool exhaustion?

**Slow Performance:**
- [ ] Database query execution plans
- [ ] Redis memory usage
- [ ] Network latency between services
- [ ] GC pressure (check dotnet-counters)

---

**Still stuck?** Tạo issue trên GitHub repository với:
- Log snippets
- SQL query (nếu có)
- Steps to reproduce
- Environment details (OS, .NET version, SQL version)

---

**Last Updated:** 2025-03-03
