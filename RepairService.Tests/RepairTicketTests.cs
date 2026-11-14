using RepairService.Core.Domain;

namespace RepairService.Tests;

public class RepairTicketTests
{
    private static DateOnly AcceptedOn => new(2026, 11, 2);

    [Fact]
    public void FullCycle_StartCompleteIssue_EndsIssued()
    {
        // Arrange
        RepairTicket ticket = NewTicketWithLine();

        // Act
        ticket.StartWork();
        ticket.Complete();
        ticket.Issue();

        // Assert
        Assert.Equal(TicketStatus.Issued, ticket.Status);
    }

    [Fact]
    public void StartWork_NoLines_Throws()
    {
        // Arrange
        RepairTicket ticket = new(1, 100, "Lenovo IdeaPad 5", AcceptedOn);

        // Act
        void Act() => ticket.StartWork();

        // Assert
        Assert.Throws<InvalidOperationException>(Act);
    }

    [Fact]
    public void Complete_FromReceived_Throws()
    {
        // Arrange
        RepairTicket ticket = NewTicketWithLine();

        // Act
        void Act() => ticket.Complete();

        // Assert
        Assert.Throws<InvalidOperationException>(Act);
    }

    [Fact]
    public void Reject_InWork_BecomesRejected()
    {
        // Arrange
        RepairTicket ticket = NewTicketWithLine();
        ticket.StartWork();

        // Act
        ticket.Reject();

        // Assert
        Assert.Equal(TicketStatus.Rejected, ticket.Status);
    }

    [Fact]
    public void Reject_Ready_Throws()
    {
        // Arrange
        RepairTicket ticket = NewTicketWithLine();
        ticket.StartWork();
        ticket.Complete();

        // Act
        void Act() => ticket.Reject();

        // Assert
        Assert.Throws<InvalidOperationException>(Act);
    }

    [Theory]
    [InlineData(0, 100, "Lenovo")]
    [InlineData(1, 0, "Lenovo")]
    [InlineData(1, 100, " ")]
    public void Constructor_InvalidArguments_Throws(int id, int customerId, string deviceModel)
    {
        // Act
        void Act() => _ = new RepairTicket(id, customerId, deviceModel, AcceptedOn);

        // Assert
        Assert.ThrowsAny<ArgumentException>(Act);
    }

    [Fact]
    public void Archived_WrapsTicket_ExposesIdStatusAndLines()
    {
        // Arrange
        RepairTicket ticket = NewTicketWithLine();
        DateOnly archivedAt = new(2026, 12, 1);

        // Act
        ArchivedRepairTicket archived = new(ticket, archivedAt);

        // Assert
        Assert.Equal(ticket.Id, archived.Id);
        Assert.Equal(TicketStatus.Received, archived.Status);
        Assert.Single(archived.Lines);
        Assert.Equal(archivedAt, archived.ArchivedAt);
    }

    private static RepairTicket NewTicketWithLine()
    {
        RepairTicket ticket = new(1, 100, "Lenovo IdeaPad 5", AcceptedOn);
        ticket.AddLine(new RepairWork("WRK-1", 2, 250m));
        return ticket;
    }
}
