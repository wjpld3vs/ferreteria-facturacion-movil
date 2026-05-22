using FerreteriaInventario.Api.Data;
using FerreteriaInventario.Api.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;

namespace FerreteriaInventario.Tests;

public class TestConfig
{
    public static DbContextOptions<AppDbContext> CreateInMemoryDbContextOptions()
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
    }

    public static JwtSettings CreateTestJwtSettings()
    {
        return new JwtSettings
        {
            Key = "test-key-that-is-at-least-32-characters-long-for-hmacsha256",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationMinutes = 60
        };
    }

    public static IOptions<JwtSettings> CreateTestJwtOptions()
    {
        return Options.Create(CreateTestJwtSettings());
    }
}
