using RepairService.Core.Domain;

namespace RepairService.Tests;

public class PriorityRulesTests
{
    private static DateOnly AcceptedOn => new(2026, 11, 2);

    [Theory]
    [InlineData(TicketPriority.Low, 10)]
    [InlineData(TicketPriority.Normal, 5)]
    [InlineData(TicketPriority.High, 2)]
    [InlineData(TicketPriority.Urgent, 1)]
    public void DeadlineOf_ByPriority_AddsDays(TicketPriority priority, int days)
    {
        // Act
        DateOnly deadline = PriorityRules.DeadlineOf(AcceptedOn, priority);

        // Assert
        Assert.Equal(AcceptedOn.AddDays(days), deadline);
    }

    [Theory]
    [InlineData(TicketPriority.Low, 0)]
    [InlineData(TicketPriority.Normal, 0)]
    [InlineData(TicketPriority.High, 0.15)]
    [InlineData(TicketPriority.Urgent, 0.30)]
    public void SurchargeOf_ByPriority_MatchesDecisionTable(TicketPriority priority, double expected)
    {
        // Act
        decimal surcharge = PriorityRules.SurchargeOf(priority);

        // Assert
        Assert.Equal((decimal)expected, surcharge);
    }

    [Fact]
    public void DaysFor_UnknownPriority_Throws()
    {
        // Arrange
        var unknown = (TicketPriority)99;

        // Act + Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => PriorityRules.DaysFor(unknown));
    }
}
