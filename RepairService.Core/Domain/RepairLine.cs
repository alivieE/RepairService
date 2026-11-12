namespace RepairService.Core.Domain;

/// <summary>Рядок заявки: робота чи запчастина, кількість і ціна.</summary>
public sealed class RepairLine
{
    public string Code { get; init; } = string.Empty;

    public int Quantity { get; init; }

    public decimal UnitPrice { get; init; }

    public decimal Amount => Quantity * UnitPrice;
}
