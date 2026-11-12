using RepairService.Core.Domain;
using RepairService.Core.Legacy;

namespace RepairService.Tests;

public class LegacyProcessorTests
{
    [Fact]
    public void Process_RegularClient_Returns1292()
    {
        List<RepairLine> lines = new()
        {
            new RepairLine { Code = "WRK-1", Quantity = 12, UnitPrice = 100m },
            new RepairLine { Code = "PRT-2", Quantity = 1, UnitPrice = 250m },
        };
        decimal total = LegacyProcessor.Process(
            1, "Іваненко", "i@ex.com", true, lines,
            new DateOnly(2026, 3, 10), "", "Accepted", false,
            60m, out string next);
        Assert.Equal(1292.70m, total);
        Assert.Equal("InRepair", next);
    }

    [Fact]
    public void Process_SmallOrder_AddsDelivery()
    {
        List<RepairLine> lines = new()
        {
            new RepairLine { Code = "WRK-1", Quantity = 2, UnitPrice = 200m },
        };
        decimal total = LegacyProcessor.Process(
            2, "Петренко", "p@ex.com", false, lines,
            new DateOnly(2026, 3, 10), "SALE10", "InRepair",
            false, 60m, out string next);
        Assert.Equal(420m, total);
        Assert.Equal("Ready", next);
    }

    [Fact]
    public void Process_NoLines_ReturnsMinusOne()
    {
        decimal total = LegacyProcessor.Process(
            3, "Коваль", "k@ex.com", false,
            new List<RepairLine>(),
            new DateOnly(2026, 3, 10), "", "Accepted", false,
            60m, out string next);
        Assert.Equal(-1m, total);
    }
}
