using RepairService.Core.Domain;

namespace RepairService.Tests;

public class TicketDeadlineTests
{
    private static DateOnly AcceptedOn => new(2026, 11, 2);

    [Fact]
    public void Create_WithoutPriority_NormalWithFiveDays()
    {
        // Act
        var ticket = new RepairTicket(1, 100, "Lenovo", AcceptedOn);

        // Assert
        Assert.Equal(TicketPriority.Normal, ticket.Priority);
        Assert.Equal(new DateOnly(2026, 11, 7), ticket.Deadline);
    }

    [Fact]
    public void Create_UrgentPriority_DeadlineNextDay()
    {
        // Act
        var ticket = new RepairTicket(1, 100, "Lenovo", AcceptedOn, TicketPriority.Urgent);

        // Assert
        Assert.Equal(new DateOnly(2026, 11, 3), ticket.Deadline);
    }

    [Fact]
    public void Overdue_TicketPastDeadline_Included()
    {
        // Arrange
        var ticket = new RepairTicket(1, 100, "Lenovo", AcceptedOn, TicketPriority.High);

        // Act
        bool overdue = ticket.IsOverdueOn(new DateOnly(2026, 11, 5));

        // Assert
        Assert.True(overdue);
    }

    [Fact]
    public void Overdue_OnDeadlineDay_NotOverdue()
    {
        // Arrange
        var ticket = new RepairTicket(1, 100, "Lenovo", AcceptedOn, TicketPriority.High);

        // Act
        bool overdue = ticket.IsOverdueOn(ticket.Deadline);

        // Assert
        Assert.False(overdue);
    }

    [Fact]
    public void Overdue_ReadyTicket_Excluded()
    {
        // Arrange
        var ticket = new RepairTicket(1, 100, "Lenovo", AcceptedOn, TicketPriority.Urgent);
        ticket.AddLine(new RepairWork("WRK-1", 1, 300m));
        ticket.StartWork();
        ticket.Complete();

        // Act
        bool overdue = ticket.IsOverdueOn(new DateOnly(2026, 11, 20));

        // Assert
        Assert.False(overdue);
    }
}
