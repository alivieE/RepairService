using Microsoft.Extensions.Logging.Abstractions;
using RepairService.Core.Abstractions;
using RepairService.Core.Domain;
using RepairService.Core.Errors;
using RepairService.Core.Notifications;
using RepairService.Core.Services;

namespace RepairService.Tests;

public class RepairTicketServiceTests
{
    private static DateOnly AcceptedOn => new(2026, 11, 2);

    [Fact]
    public void TotalOf_TwoLines_SumsPricesFromPolicy()
    {
        // Arrange
        RepairTicketService service = NewService(new FakeRepairTicketRepository(), new StubPricingPolicy(25m));
        RepairTicket ticket = new(7, 100, "Lenovo IdeaPad 5", AcceptedOn);
        ticket.AddLine(new RepairWork("WRK-1", 2, 10m));
        ticket.AddLine(new RepairWork("PRT-2", 1, 30m));

        // Act
        decimal actual = service.TotalOf(ticket);

        // Assert
        Assert.Equal(50m, actual);
    }

    [Fact]
    public void Place_Ticket_StoresAndAddsOnce()
    {
        // Arrange
        FakeRepairTicketRepository repository = new();
        RepairTicketService service = NewService(repository, new StubPricingPolicy(1m));
        RepairTicket ticket = new(1, 100, "Lenovo IdeaPad 5", AcceptedOn);

        // Act
        service.Place(ticket);

        // Assert
        Assert.NotNull(repository.GetById(1));
        Assert.Equal(1, repository.AddCalls);
    }

    [Fact]
    public void Load_UnknownId_ThrowsDomainRule()
    {
        // Arrange
        RepairTicketService service = NewService(new FakeRepairTicketRepository(), new StubPricingPolicy(1m));

        // Act
        DomainRuleException ex = Assert.Throws<DomainRuleException>(() => service.Load(404));

        // Assert
        Assert.Equal("ticket.exists", ex.Rule);
    }

    [Fact]
    public void Pay_InWorkTicket_ThrowsPayable()
    {
        // Arrange
        FakeRepairTicketRepository repository = new();
        RepairTicketService service = NewService(repository, new StubPricingPolicy(100m));
        RepairTicket ticket = NewTicketWithLine(1);
        ticket.StartWork();
        repository.Add(ticket);

        // Act
        DomainRuleException ex = Assert.Throws<DomainRuleException>(() => service.Pay(1, 100m));

        // Assert
        Assert.Equal("ticket.payable", ex.Rule);
    }
    private static RepairTicketService NewService(IRepairTicketRepository repository, IPricingPolicy pricing)
    {
        return new RepairTicketService(
            repository,
            pricing,
            new TextWriterNotifier(TextWriter.Null),
            NullLogger<RepairTicketService>.Instance);
    }
    private static RepairTicket NewTicketWithLine(int id)
    {
        RepairTicket ticket = new(id, 100, "Lenovo IdeaPad 5", AcceptedOn);
        ticket.AddLine(new RepairWork("WRK-1", 1, 100m));
        return ticket;
    }
}
