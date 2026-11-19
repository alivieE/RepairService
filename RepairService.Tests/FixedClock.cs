using RepairService.Core.Abstractions;

namespace RepairService.Tests;

/// <summary>Годинник для тестів: завжди повертає задану дату.</summary>
internal sealed class FixedClock : IClock
{
    public FixedClock(DateOnly today)
    {
        Today = today;
    }

    public DateOnly Today { get; }
}
