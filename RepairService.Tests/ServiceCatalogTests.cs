using RepairService.Core.Domain;
using RepairService.Core.Storage;

namespace RepairService.Tests;

public class ServiceCatalogTests
{
    [Fact]
    public void Generate_DefaultSize_ReturnsSixtyItemsWithUniqueCodes()
    {
        // Act
        IReadOnlyList<ServiceItem> items = ServiceCatalog.Generate(ServiceCatalog.DefaultSize);

        // Assert
        Assert.Equal(60, items.Count);
        Assert.Equal(60, items.Select(i => i.Code).Distinct().Count());
    }

    [Fact]
    public void Generate_HalfOfItems_AreParts()
    {
        // Act
        IReadOnlyList<ServiceItem> items = ServiceCatalog.Generate(ServiceCatalog.DefaultSize);

        // Assert
        Assert.Equal(30, items.Count(i => i.IsPart));
        Assert.True(items.Where(i => i.IsPart).All(i => i.Code.StartsWith("PRT-", StringComparison.Ordinal)));
    }

    [Fact]
    public void Generate_FirstItems_HaveExpectedCodesAndPrices()
    {
        // Act
        IReadOnlyList<ServiceItem> items = ServiceCatalog.Generate(2);

        // Assert
        Assert.Equal("PRT-001", items[0].Code);
        Assert.Equal(130m, items[0].Price);
        Assert.Equal("WRK-002", items[1].Code);
        Assert.Equal(300m, items[1].Price);
    }

    [Fact]
    public void Generate_ZeroCount_Throws()
    {
        // Act + Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => ServiceCatalog.Generate(0));
    }
}
