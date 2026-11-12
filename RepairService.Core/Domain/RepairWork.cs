namespace RepairService.Core.Domain;

/// <summary>
/// Позиція квитанції: робота або запчастина з кількістю й ціною.
/// Незмінний тип: після створення позицію неможливо зіпсувати.
/// </summary>
public sealed record RepairWork
{
    /// <summary>
    /// Створює позицію й перевіряє її інваріант.
    /// </summary>
    /// <param name="code">Код роботи за прейскурантом, не порожній.</param>
    /// <param name="quantity">Кількість одиниць, більша за нуль.</param>
    /// <param name="unitPrice">Ціна за одиницю, не від’ємна, грн.</param>
    /// <exception cref="ArgumentException">Код порожній.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Кількість не додатна або ціна від’ємна.
    /// </exception>
    public RepairWork(string code, int quantity, decimal unitPrice)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        ArgumentOutOfRangeException.ThrowIfNegative(unitPrice);

        Code = code;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    /// <summary>Код роботи за прейскурантом.</summary>
    public string Code { get; }

    /// <summary>Кількість одиниць.</summary>
    public int Quantity { get; }

    /// <summary>Ціна за одиницю, зафіксована на момент оформлення, грн.</summary>
    public decimal UnitPrice { get; }

    /// <summary>Сума позиції; обчислюється, тому не розходиться з кількістю й ціною.</summary>
    public decimal Amount => Quantity * UnitPrice;
}
