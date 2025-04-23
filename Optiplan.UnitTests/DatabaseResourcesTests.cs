using Microsoft.EntityFrameworkCore;
using Optiplan.DatabaseResources;

namespace Optiplan.UnitTests;

public class DatabaseResourcesTests
{
    [Fact]
    public void DatabaseConnectionTest()
    {
        var options = new DbContextOptionsBuilder<OptiplanContext>()
        .UseSqlite($"Data Source={OptiplanContextExtensions.ResolveFilePath("Optiplan.db")}")
        .Options;

        using OptiplanContext db = new OptiplanContext(options);
        Assert.True(db.Database.CanConnect());
    }
}