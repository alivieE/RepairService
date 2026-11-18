using RepairService.Core.Domain;
using RepairService.Core.Services;

namespace RepairService.Tests;

public class TicketIntakeServiceTests
{
    private static DateOnly Today => new(2026, 11, 16);

    [Fact]
    public void Accept_ValidInput_CreatesReceivedTicket()
    {
        // Arrange
        var repository = new FakeRepairTicketRepository();
        var sut = new TicketIntakeService(repository, new FixedClock(Today));

        // Act
        RepairTicket ticket = sut.Accept(100, "Lenovo", TicketPriority.Normal);

        // Assert
        Assert.Equal(TicketStatus.Received, ticket.Status);
        Assert.Equal(Today, ticket.CreatedAt);
        Assert.Same(ticket, repository.GetById(ticket.Id));
    }

    [Fact]
    public void Accept_UrgentPriority_DeadlineNextDay()
    {
        // Arrange
        var sut = new TicketIntakeService(new FakeRepairTicketRepository(), new FixedClock(Today));

        // Act
        RepairTicket ticket = sut.Accept(100, "Lenovo", TicketPriority.Urgent);

        // Assert
        Assert.Equal(Today.AddDays(1), ticket.Deadline);
    }

    [Fact]
    public void Accept_SecondTicket_GetsNextId()
    {
        // Arrange
        var sut = new TicketIntakeService(new FakeRepairTicketRepository(), new FixedClock(Today));
        sut.Accept(100, "Lenovo", TicketPriority.Low);

        // Act
        RepairTicket second = sut.Accept(101, "Asus", TicketPriority.High);

        // Assert
        Assert.Equal(2, second.Id);
    }

    [Fact]
    public void Accept_EmptyDeviceModel_Throws()
    {
        // Arrange
        var sut = new TicketIntakeService(new FakeRepairTicketRepository(), new FixedClock(Today));

        // Act + Assert
        Assert.ThrowsAny<ArgumentException>(() => sut.Accept(100, " ", TicketPriority.Normal));
    }

    [Fact]
    public void Accept_UnknownCustomer_Throws()
    {
        // Arrange
        var repository = new FakeRepairTicketRepository();
        var sut = new TicketIntakeService(repository, new FixedClock(Today));

        // Act + Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => sut.Accept(0, "Lenovo", TicketPriority.Normal));
        Assert.Empty(repository.GetAll());
    }
}
