using ECommerce.Huit.API;
using ECommerce.Huit.Application;
using ECommerce.Huit.Application.Common.Interfaces;
using ECommerce.Huit.Infrastructure.Data;
using ECommerce.Huit.API.Controllers;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using StackExchange.Redis;
using ECommerce.Huit.Application.Services;
using ECommerce.Huit.Application.Validators.Auth;


var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ECommerce HUIT API",
        Version = "v1",
        Description = "API for Electronic Store System"
    });

    // Add JWT bearer to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost,1433;Database=HuitShopDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register DbContext interface
builder.Services.AddScoped<IApplicationDbContext>(sp => 
    sp.GetRequiredService<ApplicationDbContext>());

// Configure JWT
var jwtSettings = builder.Configuration.GetSection("Jwt");
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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("ADMIN"));
    options.AddPolicy("StaffOnly", policy => policy.RequireRole("ADMIN", "STAFF"));
    options.AddPolicy("WarehouseOnly", policy => policy.RequireRole("WAREHOUSE", "ADMIN", "STAFF"));
});

// Register Application Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

// Register Infrastructure Services
builder.Services.AddSingleton<IConnectionMultiplexer, ConnectionMultiplexer>(sp =>
    StackExchange.Redis.ConnectionMultiplexer.Connect(
        builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379"
    )
);

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();

// Configure CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins(
                    "http://localhost:5173",
                    "http://100.89.137.3:5173",
                    "http://192.168.100.100:5173"
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

var app = builder.Build();

// Configure pipeline
// Enable Swagger in all environments for development/testing
app.UseStaticFiles(); // Add static files middleware
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ECommerce HUIT API v1");
    c.RoutePrefix = "swagger"; // Swagger UI at /swagger
});

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Global exception handler
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            error = "InternalServerError",
            message = "An unexpected error occurred"
        });
    });
});

// Initialize DB using EF Core
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // Delete and recreate database to ensure clean state with EF schema
    await context.Database.EnsureDeletedAsync();
    await context.Database.EnsureCreatedAsync();

    // Seed minimal data if empty
    if (!await context.Users.AnyAsync())
    {
        // Add roles/permissions? Not needed for basic functionality
        // Add sample users
        var adminUser = new ECommerce.Huit.Domain.Entities.User
        {
            FullName = "Nguyễn Minh Trí",
            Email = "admin@huit.edu.vn",
            Phone = "0909000001",
            PasswordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes("admin123")),
            Role = ECommerce.Huit.Domain.Enums.UserRole.ADMIN,
            Status = ECommerce.Huit.Domain.Enums.UserStatus.ACTIVE,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(adminUser);
        await context.SaveChangesAsync();

        // Add sample customer
        var customer = new ECommerce.Huit.Domain.Entities.User
        {
            FullName = "Phạm Khách Hàng A",
            Email = "customerA@gmail.com",
            Phone = "0909000004",
            PasswordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes("customer123")),
            Role = ECommerce.Huit.Domain.Enums.UserRole.CUSTOMER,
            Status = ECommerce.Huit.Domain.Enums.UserStatus.ACTIVE,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(customer);
        await context.SaveChangesAsync();

        // Add categories
        var laptopCategory = new ECommerce.Huit.Domain.Entities.Category
        {
            Name = "Laptop Văn Phòng",
            Slug = "laptop-office",
            IsActive = true,
            SortOrder = 1
        };
        var phoneCategory = new ECommerce.Huit.Domain.Entities.Category
        {
            Name = "Điện Thoại",
            Slug = "smart-phone",
            IsActive = true,
            SortOrder = 2
        };
        context.Categories.Add(laptopCategory);
        context.Categories.Add(phoneCategory);
        await context.SaveChangesAsync();

        // Add brands
        var dellBrand = new ECommerce.Huit.Domain.Entities.Brand
        {
            Name = "Dell",
            Origin = "USA",
            Description = "Máy tính để bàn và laptop"
        };
        var appleBrand = new ECommerce.Huit.Domain.Entities.Brand
        {
            Name = "Apple",
            Origin = "USA",
            Description = "Thiết bị chất lượng cao"
        };
        context.Brands.Add(dellBrand);
        context.Brands.Add(appleBrand);
        await context.SaveChangesAsync();

        // Add products
        var product1 = new ECommerce.Huit.Domain.Entities.Product
        {
            Name = "Dell XPS 13 Plus",
            Slug = "dell-xps-13-plus",
            BrandId = dellBrand.Id,
            CategoryId = laptopCategory.Id,
            Description = "Laptop mỏng nhẹ, màn hình OLED, chip Intel Core i7",
            Specifications = "{\"screen\":\"13.4 inch OLED\",\"cpu\":\"Intel Core i7 1360P\",\"ram\":\"16GB\"}",
            Status = ECommerce.Huit.Domain.Enums.ProductStatus.ACTIVE,
            IsFeatured = true,
            CreatedBy = adminUser.Id
        };
        var product2 = new ECommerce.Huit.Domain.Entities.Product
        {
            Name = "iPhone 15 Pro",
            Slug = "iphone-15-pro",
            BrandId = appleBrand.Id,
            CategoryId = phoneCategory.Id,
            Description = "iPhone với chip A17 Pro, camera 48MP",
            Specifications = "{\"screen\":\"6.1 inch\",\"chip\":\"A17 Pro\"}",
            Status = ECommerce.Huit.Domain.Enums.ProductStatus.ACTIVE,
            IsFeatured = true,
            CreatedBy = adminUser.Id
        };
        context.Products.Add(product1);
        context.Products.Add(product2);
        await context.SaveChangesAsync();

        // Add variants
        var variant1 = new ECommerce.Huit.Domain.Entities.ProductVariant
        {
            ProductId = product1.Id,
            Sku = "DELL-XPS13-16-512",
            VariantName = "i7/16GB/512GB",
            Price = 45000000,
            OriginalPrice = 48000000,
            ThumbnailUrl = "https://via.placeholder.com/300",
            DisplayOrder = 1,
            IsActive = true
        };
        var variant2 = new ECommerce.Huit.Domain.Entities.ProductVariant
        {
            ProductId = product2.Id,
            Sku = "IP15-128-BK",
            VariantName = "128GB - Đen",
            Price = 25990000,
            OriginalPrice = 29990000,
            ThumbnailUrl = "https://via.placeholder.com/300",
            DisplayOrder = 1,
            IsActive = true
        };
        context.ProductVariants.Add(variant1);
        context.ProductVariants.Add(variant2);
        await context.SaveChangesAsync();

        // Add warehouse
        var warehouse = new ECommerce.Huit.Domain.Entities.Warehouse
        {
            Name = "Kho chính",
            Address = "Hà Nội",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Warehouses.Add(warehouse);
        await context.SaveChangesAsync();

        // Add inventory for each variant
        context.Inventories.Add(new ECommerce.Huit.Domain.Entities.Inventory
        {
            WarehouseId = warehouse.Id,
            VariantId = variant1.Id,
            QuantityOnHand = 100,
            QuantityReserved = 0,
            ReorderPoint = 10,
            CreatedAt = DateTime.UtcNow
        });
        context.Inventories.Add(new ECommerce.Huit.Domain.Entities.Inventory
        {
            WarehouseId = warehouse.Id,
            VariantId = variant2.Id,
            QuantityOnHand = 100,
            QuantityReserved = 0,
            ReorderPoint = 10,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
    }
}

try
{
    Log.Information("Starting ECommerce HUIT API...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
