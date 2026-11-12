using RepairService.Core.Domain;

namespace RepairService.Core.Abstractions;

/// <summary>
/// Політика ціноутворення: визначає вартість однієї позиції квитанції.
/// </summary>
public interface IPricingPolicy
{
    /// <summary>Обчислює вартість позиції.</summary>
    /// <param name="line">Позиція квитанції.</param>
    /// <returns>Вартість позиції, грн.</returns>
    decimal PriceOf(RepairWork line);
}
