namespace RepairService.Core.Domain;

/// <summary>Рядок заявки: робота чи запчастина, кількість і ціна.</summary>
public sealed class RepairLine
{
    /// <summary>Код позиції прейскуранта.</summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>Кількість одиниць.</summary>
    public int Quantity { get; init; }

    /// <summary>Ціна за одиницю на момент додавання, грн.</summary>
    public decimal UnitPrice { get; init; }

    /// <summary>Сума рядка: кількість × ціна.</summary>
    public decimal Amount => Quantity * UnitPrice;
}
