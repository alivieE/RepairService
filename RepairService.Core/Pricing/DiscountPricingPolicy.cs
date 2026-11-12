using RepairService.Core.Abstractions;
using RepairService.Core.Domain;

namespace RepairService.Core.Pricing;

/// <summary>
/// Політика зі знижкою: вартість позиції зменшується на задану частку.
/// </summary>
public sealed class DiscountPricingPolicy : IPricingPolicy
{
    private const decimal MaxRate = 0.5m;

    private readonly decimal _rate;

    /// <summary>
    /// Створює політику зі знижкою.
    /// </summary>
    /// <param name="rate">Частка знижки від 0 до 0,5.</param>
    /// <exception cref="ArgumentOutOfRangeException">Частка поза межами.</exception>
    public DiscountPricingPolicy(decimal rate)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(rate);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(rate, MaxRate);
        _rate = rate;
    }

    /// <inheritdoc/>
    public decimal PriceOf(RepairWork line)
    {
        ArgumentNullException.ThrowIfNull(line);
        return Math.Round(line.Amount * (1m - _rate), 2);
    }
}
