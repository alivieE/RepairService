using RepairService.Core.Domain;
using RepairService.Core.Errors;
using RepairService.Core.Notifications;
using RepairService.Core.Pricing;
using RepairService.Core.Services;
using RepairService.Core.Storage;

namespace RepairService.Tests;

public class ErrorHandlingTests
{
    [Fact]
    public void Pay_UnknownTicket_ThrowsDomainRuleException()
    {
        RepairTicketService service = CreateService();

        DomainRuleException ex = Assert.Throws<DomainRuleException>(
            () => service.Pay(42, 100m));

        Assert.Equal("ticket.exists", ex.Rule);
    }

    [Fact]
    public void Pay_LessThanTotal_ThrowsWithAmountRule()
    {
        RepairTicketService service = CreateService();
        RepairTicket ticket = new(1, 100, "Lenovo IdeaPad 5", new DateOnly(2026, 11, 2));
        ticket.AddLine(new RepairWork("WRK-1", 2, 250m));
        service.Place(ticket);
        ticket.StartWork();
        ticket.Complete();

        DomainRuleException ex = Assert.Throws<DomainRuleException>(
            () => service.Pay(1, 400m));

        Assert.Equal("ticket.amount", ex.Rule);
        Assert.Contains("500", ex.Message);
    }

    private static RepairTicketService CreateService()
    {
        return new RepairTicketService(
            new InMemoryRepairTicketRepository(),
            new StandardPricingPolicy(),
            new TextWriterNotifier(TextWriter.Null));
    }
}
