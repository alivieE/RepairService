using RepairService.Core.Pricing;

namespace RepairService.Tests;

public class RepairCostPolicyTests
{
    [Fact]
    public void LineCostFor_NegativePrice_Throws()
    {
        // Arrange
        decimal unitPrice = -0.01m;

        // Act
        void Act() => RepairCostPolicy.LineCostFor(unitPrice, 1, false);

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(Act);
    }

    [Fact]
    public void LineCostFor_ZeroQuantity_Throws()
    {
        // Arrange
        int quantity = 0;

        // Act
        void Act() => RepairCostPolicy.LineCostFor(200m, quantity, false);

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(Act);
    }

    [Fact]
    public void LineCostFor_FreeWork_ReturnsZero()
    {
        // Arrange
        decimal unitPrice = 0m;

        // Act
        decimal actual = RepairCostPolicy.LineCostFor(unitPrice, 1, true);

        // Assert
        Assert.Equal(0m, actual);
    }

    [Fact]
    public void LineCostFor_OneUnitRegular_ReturnsPrice()
    {
        // Arrange
        decimal unitPrice = 200m;

        // Act
        decimal actual = RepairCostPolicy.LineCostFor(unitPrice, 1, false);

        // Assert
        Assert.Equal(200m, actual);
    }

    [Fact]
    public void LineCostFor_OneUnitUrgent_AddsSurcharge()
    {
        // Arrange
        bool isUrgent = true;

        // Act
        decimal actual = RepairCostPolicy.LineCostFor(200m, 1, isUrgent);

        // Assert
        Assert.Equal(260m, actual);
    }

    [Fact]
    public void LineCostFor_FourUnits_NoBulkDiscount()
    {
        // Arrange
        int quantity = RepairCostPolicy.BulkFrom - 1;

        // Act
        decimal actual = RepairCostPolicy.LineCostFor(200m, quantity, false);

        // Assert
        Assert.Equal(800m, actual);
    }

    [Fact]
    public void LineCostFor_FiveUnits_AppliesBulkDiscount()
    {
        // Arrange
        int quantity = RepairCostPolicy.BulkFrom;

        // Act
        decimal actual = RepairCostPolicy.LineCostFor(200m, quantity, false);

        // Assert
        Assert.Equal(900m, actual);
    }

    [Fact]
    public void LineCostFor_FiveUnitsUrgent_CombinesBothRules()
    {
        // Arrange
        int quantity = RepairCostPolicy.BulkFrom;

        // Act
        decimal actual = RepairCostPolicy.LineCostFor(200m, quantity, true);

        // Assert
        Assert.Equal(1200m, actual);
    }

    [Fact]
    public void LineCostFor_NonRoundProduct_RoundsToKopecks()
    {
        // Arrange
        decimal unitPrice = 33.33m;

        // Act
        decimal actual = RepairCostPolicy.LineCostFor(unitPrice, 1, true);

        // Assert
        Assert.Equal(43.33m, actual);
    }

    [Fact]
    public void LineCostFor_HalfKopeck_RoundsAwayFromZero()
    {
        // Arrange
        decimal unitPrice = 0.05m;

        // Act
        decimal actual = RepairCostPolicy.LineCostFor(unitPrice, 1, true);

        // Assert
        Assert.Equal(0.07m, actual);
    }
}
