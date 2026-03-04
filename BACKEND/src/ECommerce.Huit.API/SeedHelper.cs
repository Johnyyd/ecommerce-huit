using ECommerce.Huit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Huit.API;

public static class SeedHelper
{
    public static async Task InitializeDatabaseAsync(ApplicationDbContext context)
    {
        // Drop and recreate database to ensure clean state
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        // Read init.sql to set up permissions, stored procedures, etc.
        var initSqlPath = Path.Combine(Directory.GetCurrentDirectory(), "DATABASE", "init.sql");
        if (File.Exists(initSqlPath))
        {
            var initSql = await File.ReadAllTextAsync(initSqlPath);
            var commands = initSql.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var command in commands)
            {
                if (!string.IsNullOrWhiteSpace(command))
                {
                    await context.Database.ExecuteSqlRawAsync(command);
                }
            }
        }

        // Read seed.sql to insert sample data
        var seedSqlPath = Path.Combine(Directory.GetCurrentDirectory(), "DATABASE", "seed.sql");
        if (File.Exists(seedSqlPath))
        {
            var seedSql = await File.ReadAllTextAsync(seedSqlPath);
            var commands = seedSql.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var command in commands)
            {
                if (!string.IsNullOrWhiteSpace(command))
                {
                    await context.Database.ExecuteSqlRawAsync(command);
                }
            }
        }
    }
}
