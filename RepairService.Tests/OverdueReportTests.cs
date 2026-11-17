using RepairService.Core.Domain;
using RepairService.Core.Services;

namespace RepairService.Tests;

public class OverdueReportTests
{
    private static DateOnly AcceptedOn => new(2026, 11, 2);

    private static DateOnly ReportDate => new(2026, 11, 10);

    [Fact]
    public void Overdue_TicketPastDeadline_Included()
    {
        // Arrange
        var repository = new FakeRepairTicketRepository();
        repository.Add(new RepairTicket(1, 100, "Lenovo", AcceptedOn, TicketPriority.Urgent));
        var sut = new TicketQueryService(repository);

        // Act
        IReadOnlyList<RepairTicket> overdue = sut.Overdue(ReportDate);

        // Assert
        Assert.Single(overdue);
    }

    [Fact]
    public void Overdue_LowPriorityWithinTenDays_Excluded()
    {
        // Arrange
        var repository = new FakeRepairTicketRepository();
        repository.Add(new RepairTicket(1, 100, "Lenovo", AcceptedOn, TicketPriority.Low));
        var sut = new TicketQueryService(repository);

        // Act
        IReadOnlyList<RepairTicket> overdue = sut.Overdue(ReportDate);

        // Assert
        Assert.Empty(overdue);
    }

    [Fact]
    public void Overdue_ReadyTicket_Excluded()
    {
        // Arrange
        var ticket = new RepairTicket(1, 100, "Lenovo", AcceptedOn, TicketPriority.Urgent);
        ticket.AddLine(new RepairWork("WRK-1", 1, 300m));
        ticket.StartWork();
        ticket.Complete();
        var repository = new FakeRepairTicketRepository();
        repository.Add(ticket);
        var sut = new TicketQueryService(repository);

        // Act
        IReadOnlyList<RepairTicket> overdue = sut.Overdue(ReportDate);

        // Assert
        Assert.Empty(overdue);
    }

    [Fact]
    public void Overdue_SortedByDeadline()
    {
        // Arrange
        var repository = new FakeRepairTicketRepository();
        repository.Add(new RepairTicket(1, 100, "Lenovo", AcceptedOn, TicketPriority.Normal));
        repository.Add(new RepairTicket(2, 100, "Asus", AcceptedOn, TicketPriority.Urgent));
        repository.Add(new RepairTicket(3, 100, "Acer", AcceptedOn, TicketPriority.High));
        var sut = new TicketQueryService(repository);

        // Act
        IReadOnlyList<RepairTicket> overdue = sut.Overdue(ReportDate);

        // Assert
        Assert.Equal(3, overdue.Count);
        Assert.Equal(2, overdue[0].Id);
        Assert.Equal(3, overdue[1].Id);
        Assert.Equal(1, overdue[2].Id);
    }
}
