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

    [Fact]
    public void Pay_ExactTotal_IssuesTicket()
    {
        // Arrange
        FakeRepairTicketRepository repository = new();
        RepairTicketService service = NewService(repository, new StubPricingPolicy(100m));
        RepairTicket ticket = NewReadyTicket(1);
        repository.Add(ticket);

        // Act
        service.Pay(1, 100m);

        // Assert
        Assert.Equal(TicketStatus.Issued, ticket.Status);
    }

    [Fact]
    public void Pay_Overpayment_StillIssuesTicket()
    {
        // Arrange
        FakeRepairTicketRepository repository = new();
        RepairTicketService service = NewService(repository, new StubPricingPolicy(100m));
        RepairTicket ticket = NewReadyTicket(1);
        repository.Add(ticket);

        // Act
        service.Pay(1, 150m);

        // Assert
        Assert.Equal(TicketStatus.Issued, ticket.Status);
    }

    [Fact]
    public void AddLine_ReceivedTicket_AddsLine()
    {
        // Arrange
        FakeRepairTicketRepository repository = new();
        RepairTicketService service = NewService(repository, new StubPricingPolicy(1m));
        RepairTicket ticket = new(1, 100, "Lenovo IdeaPad 5", AcceptedOn);
        repository.Add(ticket);

        // Act
        service.AddLine(1, new RepairWork("WRK-1", 2, 250m));

        // Assert
        Assert.Single(ticket.Lines);
    }

    [Fact]
    public void AddLine_TicketInWork_ThrowsEditable()
    {
        // Arrange
        FakeRepairTicketRepository repository = new();
        RepairTicketService service = NewService(repository, new StubPricingPolicy(1m));
        RepairTicket ticket = NewTicketWithLine(1);
        ticket.StartWork();
        repository.Add(ticket);

        // Act
        DomainRuleException ex = Assert.Throws<DomainRuleException>(
            () => service.AddLine(1, new RepairWork("PRT-2", 1, 50m)));

        // Assert
        Assert.Equal("ticket.editable", ex.Rule);
    }

    [Fact]
    public void Cancel_ReceivedTicket_RejectsTicket()
    {
        // Arrange
        FakeRepairTicketRepository repository = new();
        RepairTicketService service = NewService(repository, new StubPricingPolicy(1m));
        RepairTicket ticket = NewTicketWithLine(1);
        repository.Add(ticket);

        // Act
        service.Cancel(1, "клієнт передумав");

        // Assert
        Assert.Equal(TicketStatus.Rejected, ticket.Status);
    }

    [Fact]
    public void Cancel_IssuedTicket_ThrowsCancellableWithInner()
    {
        // Arrange
        FakeRepairTicketRepository repository = new();
        RepairTicketService service = NewService(repository, new StubPricingPolicy(100m));
        RepairTicket ticket = NewReadyTicket(1);
        ticket.Issue();
        repository.Add(ticket);

        // Act
        DomainRuleException ex = Assert.Throws<DomainRuleException>(
            () => service.Cancel(1, "клієнт передумав"));

        // Assert
        Assert.Equal("ticket.cancellable", ex.Rule);
        Assert.IsType<InvalidOperationException>(ex.InnerException);
    }

    [Fact]
    public void ExportReport_Ticket_WritesIdToFile()
    {
        // Arrange
        RepairTicketService service = NewService(new FakeRepairTicketRepository(), new StubPricingPolicy(100m));
        string path = Path.GetTempFileName();

        // Act
        service.ExportReport(path, NewTicketWithLine(5));

        // Assert
        string text = File.ReadAllText(path);
        File.Delete(path);
        Assert.Contains("Квитанція 5", text);
    }

    [Fact]
    public void ExportReport_MissingFolder_ThrowsIoException()
    {
        // Arrange
        RepairTicketService service = NewService(new FakeRepairTicketRepository(), new StubPricingPolicy(100m));
        string path = Path.Combine(Path.GetTempPath(), "missing-" + Path.GetRandomFileName(), "report.txt");

        // Act
        void Act() => service.ExportReport(path, NewTicketWithLine(5));

        // Assert
        Assert.ThrowsAny<IOException>(Act);
    }

    private static RepairTicketService NewService(IRepairTicketRepository repository, IPricingPolicy pricing)
    {
        return new RepairTicketService(
            repository,
            pricing,
            new TextWriterNotifier(TextWriter.Null),
            NullLogger<RepairTicketService>.Instance);
    }

    private static RepairTicket NewReadyTicket(int id)
    {
        RepairTicket ticket = NewTicketWithLine(id);
        ticket.StartWork();
        ticket.Complete();
        return ticket;
    }

    private static RepairTicket NewTicketWithLine(int id)
    {
        RepairTicket ticket = new(id, 100, "Lenovo IdeaPad 5", AcceptedOn);
        ticket.AddLine(new RepairWork("WRK-1", 1, 100m));
        return ticket;
    }
}
