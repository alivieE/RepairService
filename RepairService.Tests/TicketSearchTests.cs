using RepairService.Core.Domain;
using RepairService.Core.Services;

namespace RepairService.Tests;

public class TicketSearchTests
{
    private static DateOnly AcceptedOn => new(2026, 11, 2);

    [Fact]
    public void Find_ByStatus_ReturnsMatching()
    {
        // Arrange
        var repository = new FakeRepairTicketRepository();
        repository.Add(new RepairTicket(1, 100, "Lenovo", AcceptedOn));
        repository.Add(InWork(2, TicketPriority.High));
        var sut = new TicketQueryService(repository);

        // Act
        IReadOnlyList<RepairTicket> found = sut.Find(TicketStatus.InWork, null);

        // Assert
        RepairTicket ticket = Assert.Single(found);
        Assert.Equal(2, ticket.Id);
    }

    [Fact]
    public void Find_ByStatusAndPriority_ReturnsOnlyBothMatching()
    {
        // Arrange
        var repository = new FakeRepairTicketRepository();
        repository.Add(InWork(1, TicketPriority.Low));
        repository.Add(InWork(2, TicketPriority.Urgent));
        repository.Add(new RepairTicket(3, 100, "Asus", AcceptedOn, TicketPriority.Urgent));
        var sut = new TicketQueryService(repository);

        // Act
        IReadOnlyList<RepairTicket> found = sut.Find(TicketStatus.InWork, TicketPriority.Urgent);

        // Assert
        RepairTicket ticket = Assert.Single(found);
        Assert.Equal(2, ticket.Id);
    }

    [Fact]
    public void Find_NoCriteria_ReturnsAllSortedById()
    {
        // Arrange
        var repository = new FakeRepairTicketRepository();
        repository.Add(new RepairTicket(2, 100, "Asus", AcceptedOn));
        repository.Add(new RepairTicket(1, 100, "Lenovo", AcceptedOn));
        var sut = new TicketQueryService(repository);

        // Act
        IReadOnlyList<RepairTicket> found = sut.Find(null, null);

        // Assert
        Assert.Equal(2, found.Count);
        Assert.Equal(1, found[0].Id);
        Assert.Equal(2, found[1].Id);
    }

    [Fact]
    public void Find_NoMatches_ReturnsEmpty()
    {
        // Arrange
        var repository = new FakeRepairTicketRepository();
        repository.Add(new RepairTicket(1, 100, "Lenovo", AcceptedOn));
        var sut = new TicketQueryService(repository);

        // Act
        IReadOnlyList<RepairTicket> found = sut.Find(TicketStatus.Issued, null);

        // Assert
        Assert.Empty(found);
    }

    private static RepairTicket InWork(int id, TicketPriority priority)
    {
        var ticket = new RepairTicket(id, 100, "Lenovo", AcceptedOn, priority);
        ticket.AddLine(new RepairWork("WRK-1", 1, 300m));
        ticket.StartWork();
        return ticket;
    }
}
