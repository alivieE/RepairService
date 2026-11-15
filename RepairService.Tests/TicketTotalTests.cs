using RepairService.Core.Domain;

namespace RepairService.Tests;

public class TicketTotalTests
{
    [Theory]
    [InlineData(TicketPriority.Low, 1000, 500, 1500)]
    [InlineData(TicketPriority.Normal, 1000, 500, 1500)]
    [InlineData(TicketPriority.High, 1000, 500, 1650)]
    [InlineData(TicketPriority.Urgent, 1000, 500, 1800)]
    [InlineData(TicketPriority.Urgent, 0, 500, 500)]
    [InlineData(TicketPriority.High, 0.30, 0, 0.35)]
    public void TotalOf_ByPriority_MatchesDecisionTable(
        TicketPriority priority, double works, double parts, double expected)
    {
        // Arrange
        var lines = new[]
        {
            new RepairWork("WRK-1", 1, (decimal)works, LineKind.Work),
            new RepairWork("PRT-1", 1, (decimal)parts, LineKind.Part),
        };

        // Act
        decimal total = PriorityRules.TotalOf(lines, priority);

        // Assert
        Assert.Equal((decimal)expected, total);
    }

    [Fact]
    public void TotalOf_NoLines_ReturnsZero()
    {
        // Act
        decimal total = PriorityRules.TotalOf(Array.Empty<RepairWork>(), TicketPriority.Urgent);

        // Assert
        Assert.Equal(0m, total);
    }

    [Fact]
    public void TotalOf_NullLines_Throws()
    {
        // Act + Assert
        Assert.Throws<ArgumentNullException>(() => PriorityRules.TotalOf(null!, TicketPriority.Low));
    }

    [Fact]
    public void RepairWork_WithoutKind_IsWork()
    {
        // Act
        var line = new RepairWork("WRK-1", 1, 100m);

        // Assert
        Assert.Equal(LineKind.Work, line.Kind);
    }
}
