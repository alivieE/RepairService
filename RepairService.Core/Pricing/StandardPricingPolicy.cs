using RepairService.Core.Abstractions;
using RepairService.Core.Domain;

namespace RepairService.Core.Pricing;

/// <summary>
/// Звичайна політика: вартість позиції без знижки.
/// </summary>
public sealed class StandardPricingPolicy : IPricingPolicy
{
    /// <inheritdoc/>
    public decimal PriceOf(RepairWork line)
    {
        ArgumentNullException.ThrowIfNull(line);
        return line.Amount;
    }
}
