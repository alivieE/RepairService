using RepairService.Core.Abstractions;

namespace RepairService.App;

/// <summary>Годинник застосунку: поточна дата з системного часу.</summary>
public sealed class SystemClock : IClock
{
    /// <inheritdoc/>
    public DateOnly Today => DateOnly.FromDateTime(DateTime.Now);
}
