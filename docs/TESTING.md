# Testing Guide - ECommerce HUIT

Tài liệu này hướng dẫn chạy test cho backend ECommerce HUIT.

---

## 1. Cấu trúc Test Projects

```
ecommerce-huit/
├── tests/
│   ├── UnitTests/          # Unit tests
│   │   ├── UnitTests.csproj
│   │   ├── AuthServiceTests.cs
│   │   ├── ProductServiceTests.cs
│   │   ├── OrderServiceTests.cs      (new)
│   │   ├── CartServiceTests.cs       (new)
│   │   └── JwtTokenGeneratorTests.cs (new)
│   │
│   └── IntegrationTests/   # Integration tests
│       ├── IntegrationTests.csproj
│       ├── AuthEndpointTests.cs
│       ├── ProductsEndpointTests.cs
│       ├── CartEndpointTests.cs      (new)
│       └── OrderEndpointTests.cs     (new)
```

---

## 2. Yêu cầu

- **.NET 8 SDK** (latest)<br>Download: https://dotnet.microsoft.com/download/dotnet/8.0
- **SQL Server** (optional for local DB tests)<br>Docker: `docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=YourStrong@Passw0rd' -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest`
- **Redis** (optional)<br>Docker: `docker run -d -p 6379:6379 redis:7-alpine`

---

## 3. Chạy tất cả tests

```bash
# Restore packages
dotnet restore ecommerce-huit.sln

# Build solution
dotnet build ecommerce-huit.sln -c Release

# Run all tests (Unit + Integration)
dotnet test ecommerce-huit.sln -c Release --logger "trx;LogFileName=test_results.trx"
```

---

## 4. Chạy riêng Unit Tests

```bash
cd ecommerce-huit/tests/UnitTests
dotnet test
```

### Coverage (optional)

```bash
dotnet test --collect:"XPlat Code Coverage"
# Reports in TestResults/<guid>/coverage.cobertura.xml
```

---

## 5. Chạy riêng Integration Tests

```bash
cd ecommerce-huit/tests/IntegrationTests
dotnet test
```

Integration tests use **in-memory database** by default, no external DB required.

---

## 6. Test coverage

Hiện tại, dự kiến có các test phạm vi:

| Service/Endpoint       | Unit | Integration |
|------------------------|------|-------------|
| AuthService            | ✅   | ✅ (register, login) |
| ProductService         | ✅   | ✅ (list, detail, 404) |
| CartService            | ✅   | ✅ (CRUD, voucher) |
| OrderService           | ✅   | ✅ (create, get, cancel, pagination) |
| JwtTokenGenerator      | ✅   | - |
| Admin endpoints        | -    | ⚠️ (planned) |
| Payment endpoints      | -    | ⚠️ (planned) |

---

## 7. Notes for developers

### Writing Unit Tests

- Use `ApplicationDbContext` with **InMemory** provider:
  ```csharp
  var options = new DbContextOptionsBuilder<ApplicationDbContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString())
      .Options;
  var context = new ApplicationDbContext(options);
  ```
- Seed required data (users, products, variants, inventory) before testing.
- Use `Moq` for mocking dependencies (IJwtTokenGenerator, etc.).
- Use `FluentAssertions` for readable assertions (already configured).

### Writing Integration Tests

- Use `WebApplicationFactory<Program>` to host API.
- Override DB with in-memory:
  ```csharp
  services.AddDbContext<ApplicationDbContext>(options =>
      options.UseInMemoryDatabase("TestDb"));
  ```
- For endpoints requiring userId, pass as query param: `?userId=1`
- For authentication: currently tests bypass JWT by design; for protected endpoints you may need to configure JWT settings in test host.

### Test Data

Keep test data minimal and isolated. Use Guid-generated DB names to avoid collision between tests.

---

## 8. CI/CD Integration

GitHub Actions workflow `.github/workflows/ci.yml` automatically runs:

- **Build-and-test job:**
  - Restore, build
  - Spin up SQL Server + Redis services
  - Run database migrations (if any)
  - Run Unit tests
  - Run Integration tests
  - Upload test results as artifact

- **Lint job:**
  - Run `dotnet format --verify-no-changes`

All tests must pass before merge to `main` or `develop`.

---

## 9. Troubleshooting

### InMemory database not resetting between tests

Use unique DB name per test class or use `IAsyncLifetime` to ensure clean state.

### Stored procedures not found during integration tests

Ensure `init.sql` runs before tests. For in-memory DB, stored procedures are not supported. Integration tests that need SPs should use SQL Server (see `ci.yml` example) or mock SP calls.

### Port conflicts (1433, 6379)

Change ports in `ci.yml` or stop local services.

---

## 10. Planned improvements

- [ ] Add more admin endpoint tests (inventory import, reports)
- [ ] Add payment webhook tests
- [ ] Add serial/IMEI tracking tests
- [ ] Add multi-warehouse tests
- [ ] Add performance/load tests

---

**Happy testing!** 🧪
