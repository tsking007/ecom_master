using EcommerceApp.Application.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EcommerceApp.Infrastructure.Persistence;

/// <summary>
/// Used exclusively by the EF Core CLI tools (Add-Migration, Update-Database).
/// Never instantiated at runtime. Creates AppDbContext without the full DI container.
///
/// HOW TO CONFIGURE BEFORE RUNNING MIGRATIONS:
///   Option A — Set environment variable:
///     set ECOMMERCE_CONNECTION_STRING=Server=...
///   Option B — Edit the fallback string below directly.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ECOMMERCE_CONNECTION_STRING")
            ?? "Server = (localdb)\\MyLocalDB; Database = TodoDb; Trusted_Connection = True; MultipleActiveResultSets = true; TrustServerCertificate = True";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.MigrationsAssembly(
                typeof(AppDbContextFactory).Assembly.FullName);
        });

        return new AppDbContext(
            optionsBuilder.Options,
            new DesignTimeCurrentUserService());
    }
}

/// <summary>
/// No-op ICurrentUserService for design-time only.
/// Returns null/false for all members — migrations don't need a real user.
/// </summary>
internal sealed class DesignTimeCurrentUserService : ICurrentUserService
{
    public Guid? UserId => null;
    public bool IsAuthenticated => false;
    public string? Email => null;
    public string? Role => null;
}