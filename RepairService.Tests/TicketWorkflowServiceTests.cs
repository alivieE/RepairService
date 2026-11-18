using RepairService.Core.Domain;
using RepairService.Core.Errors;
using RepairService.Core.Services;
using RepairService.Core.Storage;

namespace RepairService.Tests;

public class TicketWorkflowServiceTests
{
    private static DateOnly AcceptedOn => new(2026, 11, 2);

    [Fact]
    public void AddLine_KnownCode_AddsLineWithCatalogPrice()
    {
        // Arrange
        (TicketWorkflowService sut, FakeRepairTicketRepository repository) = Create(TicketPriority.Normal);

        // Act
        Result<RepairWork> result = sut.AddLine(1, "prt-001", 2);

        // Assert
        Assert.True(result.IsSuccess);
        RepairWork line = Assert.Single(repository.GetById(1)!.Lines);
        Assert.Equal(130m, line.UnitPrice);
        Assert.Equal(LineKind.Part, line.Kind);
    }

    [Fact]
    public void AddLine_UnknownCode_Fails()
    {
        // Arrange
        (TicketWorkflowService sut, _) = Create(TicketPriority.Normal);

        // Act
        Result<RepairWork> result = sut.AddLine(1, "XYZ-999", 1);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("XYZ-999", result.Error);
    }

    [Fact]
    public void AddLine_AfterStartWork_Throws()
    {
        // Arrange
        (TicketWorkflowService sut, _) = Create(TicketPriority.Normal);
        sut.AddLine(1, "WRK-002", 1);
        sut.MoveTo(1, TicketStatus.InWork);

        // Act
        DomainRuleException ex = Assert.Throws<DomainRuleException>(() => sut.AddLine(1, "WRK-002", 1));

        // Assert
        Assert.Equal("ticket.transition", ex.Rule);
    }

    [Fact]
    public void StartWork_ReceivedWithLines_BecomesInWork()
    {
        // Arrange
        (TicketWorkflowService sut, FakeRepairTicketRepository repository) = Create(TicketPriority.Normal);
        sut.AddLine(1, "WRK-002", 1);

        // Act
        sut.MoveTo(1, TicketStatus.InWork);

        // Assert
        Assert.Equal(TicketStatus.InWork, repository.GetById(1)!.Status);
    }

    [Fact]
    public void Issue_FromInWork_Throws()
    {
        // Arrange
        (TicketWorkflowService sut, FakeRepairTicketRepository repository) = Create(TicketPriority.Normal);
        sut.AddLine(1, "WRK-002", 1);
        sut.MoveTo(1, TicketStatus.InWork);

        // Act
        DomainRuleException ex = Assert.Throws<DomainRuleException>(() => sut.Pay(1, 1000m));

        // Assert
        Assert.Equal("ticket.transition", ex.Rule);
        Assert.Equal(TicketStatus.InWork, repository.GetById(1)!.Status);
    }

    [Fact]
    public void Reject_AfterIssued_Throws()
    {
        // Arrange
        (TicketWorkflowService sut, _) = ReadyTicket();
        sut.Pay(1, 300m);

        // Act + Assert
        Assert.Throws<DomainRuleException>(() => sut.MoveTo(1, TicketStatus.Rejected));
    }

    [Fact]
    public void Reject_FromReceived_BecomesRejected()
    {
        // Arrange
        (TicketWorkflowService sut, FakeRepairTicketRepository repository) = Create(TicketPriority.Normal);

        // Act
        sut.MoveTo(1, TicketStatus.Rejected);

        // Assert
        Assert.Equal(TicketStatus.Rejected, repository.GetById(1)!.Status);
    }

    [Fact]
    public void MoveTo_ReadyFromReceived_Throws()
    {
        // Arrange
        (TicketWorkflowService sut, _) = Create(TicketPriority.Normal);

        // Act + Assert
        Assert.Throws<DomainRuleException>(() => sut.MoveTo(1, TicketStatus.Ready));
    }

    [Fact]
    public void MoveTo_Issued_ThrowsBecauseOnlyPaymentIssues()
    {
        // Arrange
        (TicketWorkflowService sut, _) = Create(TicketPriority.Normal);

        // Act + Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => sut.MoveTo(1, TicketStatus.Issued));
    }

    [Fact]
    public void TotalOf_UrgentTicket_AddsSurchargeToWorksOnly()
    {
        // Arrange
        (TicketWorkflowService sut, _) = Create(TicketPriority.Urgent);
        sut.AddLine(1, "WRK-002", 1);
        sut.AddLine(1, "PRT-001", 1);

        // Act
        decimal total = sut.TotalOf(1);

        // Assert
        Assert.Equal(520m, total);
    }

    [Fact]
    public void Pay_LessThanTotal_ThrowsWithAmountRule()
    {
        // Arrange
        (TicketWorkflowService sut, _) = ReadyTicket();

        // Act
        DomainRuleException ex = Assert.Throws<DomainRuleException>(() => sut.Pay(1, 299.99m));

        // Assert
        Assert.Equal("ticket.amount", ex.Rule);
    }

    [Fact]
    public void Pay_ExactTotal_BecomesIssued()
    {
        // Arrange
        (TicketWorkflowService sut, FakeRepairTicketRepository repository) = ReadyTicket();

        // Act
        sut.Pay(1, 300m);

        // Assert
        Assert.Equal(TicketStatus.Issued, repository.GetById(1)!.Status);
    }

    [Fact]
    public void Pay_UnknownTicket_ThrowsWithExistsRule()
    {
        // Arrange
        (TicketWorkflowService sut, _) = Create(TicketPriority.Normal);

        // Act
        DomainRuleException ex = Assert.Throws<DomainRuleException>(() => sut.Pay(42, 100m));

        // Assert
        Assert.Equal("ticket.exists", ex.Rule);
    }

    private static (TicketWorkflowService Sut, FakeRepairTicketRepository Repository) Create(
        TicketPriority priority)
    {
        var repository = new FakeRepairTicketRepository();
        repository.Add(new RepairTicket(1, 100, "Lenovo", AcceptedOn, priority));
        var sut = new TicketWorkflowService(repository, ServiceCatalog.Generate(2));
        return (sut, repository);
    }

    private static (TicketWorkflowService Sut, FakeRepairTicketRepository Repository) ReadyTicket()
    {
        (TicketWorkflowService sut, FakeRepairTicketRepository repository) = Create(TicketPriority.Normal);
        sut.AddLine(1, "WRK-002", 1);
        sut.MoveTo(1, TicketStatus.InWork);
        sut.MoveTo(1, TicketStatus.Ready);
        return (sut, repository);
    }
}
