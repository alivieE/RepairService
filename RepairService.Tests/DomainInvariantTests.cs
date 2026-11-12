using RepairService.Core.Domain;
using RepairService.Core.Notifications;
using RepairService.Core.Pricing;
using RepairService.Core.Services;
using RepairService.Core.Storage;

namespace RepairService.Tests;

public class DomainInvariantTests
{
    [Fact]
    public void RepairWork_WithZeroQuantity_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new RepairWork("WRK-1", 0, 100m));
    }

    [Fact]
    public void RepairTicket_AfterStartWork_RejectsNewLines()
    {
        RepairTicket ticket = NewTicket(1);
        ticket.AddLine(new RepairWork("WRK-1", 1, 100m));
        ticket.StartWork();

        Assert.Throws<InvalidOperationException>(
            () => ticket.AddLine(new RepairWork("WRK-2", 1, 50m)));
    }

    [Fact]
    public void TotalOf_WithTenPercentDiscount_Returns180()
    {
        RepairTicketService service = new(
            new InMemoryRepairTicketRepository(),
            new DiscountPricingPolicy(0.10m),
            new TextWriterNotifier(TextWriter.Null));
        RepairTicket ticket = NewTicket(2);
        ticket.AddLine(new RepairWork("WRK-1", 2, 100m));

        Assert.Equal(180m, service.TotalOf(ticket));
    }

    private static RepairTicket NewTicket(int id) =>
        new(id, 100, "Lenovo IdeaPad 5", new DateOnly(2026, 11, 2));
}
