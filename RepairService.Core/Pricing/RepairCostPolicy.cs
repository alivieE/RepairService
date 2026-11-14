namespace RepairService.Core.Pricing;

/// <summary>
/// Вартість позиції заявки: ціна × кількість з надбавкою за терміновість
/// і знижкою за кількість одиниць.
/// </summary>
public static class RepairCostPolicy
{
    /// <summary>Надбавка за терміновий ремонт.</summary>
    public const decimal UrgentRate = 0.30m;

    /// <summary>Знижка за велику кількість одиниць.</summary>
    public const decimal BulkRate = 0.10m;

    /// <summary>Кількість одиниць, з якої діє знижка.</summary>
    public const int BulkFrom = 5;

    /// <summary>Обчислює вартість позиції, грн.</summary>
    /// <param name="unitPrice">Ціна за одиницю, грн; 0 — безкоштовна робота.</param>
    /// <param name="quantity">Кількість одиниць, не менше 1.</param>
    /// <param name="isUrgent">Ознака термінового ремонту.</param>
    /// <returns>Вартість, округлена до копійок.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Ціна від’ємна або кількість менша за 1.</exception>
    public static decimal LineCostFor(decimal unitPrice, int quantity, bool isUrgent)
    {
        if (unitPrice < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "Ціна не може бути від’ємною.");
        }

        if (quantity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Кількість має бути не менше 1.");
        }

        decimal factor = 1m;
        if (isUrgent)
        {
            factor += UrgentRate;
        }

        if (quantity >= BulkFrom)
        {
            factor -= BulkRate;
        }

        return decimal.Round(unitPrice * quantity * factor, 2, MidpointRounding.AwayFromZero);
    }
}
