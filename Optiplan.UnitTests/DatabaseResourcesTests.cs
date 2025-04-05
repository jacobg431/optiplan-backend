using Optiplan.DatabaseResources;

namespace Optiplan.UnitTests;

public class DatabaseResourcesTests
{
    [Fact]
    public void DatabaseConnectionTest()
    {
        using OptiplanContext db = new();
        Assert.True(db.Database.CanConnect());
    }
}