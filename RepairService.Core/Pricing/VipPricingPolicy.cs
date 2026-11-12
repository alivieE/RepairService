using RepairService.Core.Abstractions;
using RepairService.Core.Domain;

namespace RepairService.Core.Pricing;

/// <summary>
/// Політика для VIP-клієнтів: знижка 10 % на кожну позицію.
/// </summary>
public sealed class VipPricingPolicy : IPricingPolicy
{
    private const decimal VipRate = 0.90m;

    /// <inheritdoc/>
    public decimal PriceOf(RepairWork line)
    {
        ArgumentNullException.ThrowIfNull(line);
        return Math.Round(line.Amount * VipRate, 2);
    }
}
