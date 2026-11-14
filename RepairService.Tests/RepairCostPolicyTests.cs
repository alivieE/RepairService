using RepairService.Core.Pricing;

namespace RepairService.Tests;

public class RepairCostPolicyTests
{
    [Theory]
    [InlineData(0, 1, true, 0)]
    [InlineData(200, 1, false, 200)]
    [InlineData(200, 1, true, 260)]
    [InlineData(200, 4, false, 800)]
    [InlineData(200, 5, false, 900)]
    [InlineData(200, 5, true, 1200)]
    [InlineData(33.33, 1, true, 43.33)]
    [InlineData(0.05, 1, true, 0.07)]
    [InlineData(1600, 1, true, 2080)]
    [InlineData(2000, 1, true, 2500)]
    public void LineCostFor_Boundaries_MatchesTable(
        double unitPrice, int quantity, bool isUrgent, double expected)
    {
        // Act
        decimal actual = RepairCostPolicy.LineCostFor((decimal)unitPrice, quantity, isUrgent);

        // Assert
        Assert.Equal((decimal)expected, actual);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-1000)]
    public void LineCostFor_NegativePrice_ThrowsOutOfRange(double unitPrice)
    {
        // Act
        ArgumentOutOfRangeException ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => RepairCostPolicy.LineCostFor((decimal)unitPrice, 1, false));

        // Assert
        Assert.Equal("unitPrice", ex.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void LineCostFor_QuantityBelowOne_ThrowsOutOfRange(int quantity)
    {
        // Act
        ArgumentOutOfRangeException ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => RepairCostPolicy.LineCostFor(200m, quantity, false));

        // Assert
        Assert.Equal("quantity", ex.ParamName);
    }

    [Fact]
    public void LineCostFor_HugeUrgentLine_SurchargeCappedAt500()
    {
        // Arrange
        int quantity = 4;

        // Act
        decimal actual = RepairCostPolicy.LineCostFor(1000m, quantity, true);

        // Assert
        Assert.Equal(4500m, actual);
    }
}
