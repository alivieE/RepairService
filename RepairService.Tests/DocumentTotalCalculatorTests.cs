using RepairService.Core.Documents;
using RepairService.Core.Domain;
using RepairService.Core.Reporting;

namespace RepairService.Tests;

public class DocumentTotalCalculatorTests
{
    [Fact]
    public void Calculate_RegularClient_Returns1292()
    {
        TotalRequest request = BuildSampleRequest();

        TotalResult result = DocumentTotalCalculator.Calculate(request);

        Assert.Equal(1292.70m, result.Total);
        Assert.Equal(RepairStatus.InRepair, result.Status);
    }

    [Fact]
    public void Calculate_SmallOrder_AddsDelivery()
    {
        TotalRequest request = new(
            DocumentId: 2,
            Client: new ClientInfo("Петренко", "p@ex.com", false),
            Lines: new List<RepairLine>
            {
                new() { Code = "WRK-1", Quantity = 2, UnitPrice = 200m },
            },
            CreatedAt: new DateOnly(2026, 3, 10),
            CouponCode: "SALE10",
            Status: RepairStatus.InRepair,
            DeliveryPrice: 60m);

        TotalResult result = DocumentTotalCalculator.Calculate(request);

        Assert.Equal(420m, result.Total);
        Assert.Equal(RepairStatus.Ready, result.Status);
    }

    [Fact]
    public void Calculate_NoLines_ThrowsArgument()
    {
        Assert.Throws<ArgumentException>(() =>
            DocumentGuards.EnsureLinesValid(new List<RepairLine>()));
    }

    [Fact]
    public void Calculate_ZeroQuantity_ThrowsRange()
    {
        RepairLine line = new() { Code = "WRK-1", Quantity = 0, UnitPrice = 10m };
        Assert.Throws<ArgumentOutOfRangeException>(
            () => DocumentGuards.EnsureLineValid(line));
    }

    [Fact]
    public void Calculate_CancelledRequest_ReturnsZero()
    {
        TotalRequest request = BuildSampleRequest() with { Status = RepairStatus.Cancelled };

        TotalResult result = DocumentTotalCalculator.Calculate(request);

        Assert.Equal(0m, result.Total);
        Assert.Equal(RepairStatus.Cancelled, result.Status);
    }

    [Fact]
    public void CouponDiscountOf_FixedCouponAboveThreshold_Returns200()
    {
        decimal discount = DocumentTotalCalculator.CouponDiscountOf(1500m, "MINUS200");

        Assert.Equal(200m, discount);
    }

    [Fact]
    public void TextOf_ContainsTotalLine()
    {
        TotalRequest request = BuildSampleRequest();
        TotalResult result = DocumentTotalCalculator.Calculate(request);

        string text = DocumentReport.TextOf(request, result);

        Assert.Contains("Разом: 1292.70", text);
    }

    [Fact]
    public void Print_WritesReportToOutput()
    {
        using StringWriter writer = new();
        DocumentReportPrinter printer = new(writer);

        printer.Print("Разом: 10.00");

        Assert.Contains("10.00", writer.ToString());
    }

    private static TotalRequest BuildSampleRequest()
    {
        return new TotalRequest(
            DocumentId: 1,
            Client: new ClientInfo("Іваненко", "i@ex.com", true),
            Lines: new List<RepairLine>
            {
                new() { Code = "WRK-1", Quantity = 12, UnitPrice = 100m },
                new() { Code = "PRT-2", Quantity = 1, UnitPrice = 250m },
            },
            CreatedAt: new DateOnly(2026, 3, 10),
            CouponCode: null,
            Status: RepairStatus.Accepted,
            DeliveryPrice: 60m);
    }
}
