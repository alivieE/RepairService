namespace RepairService.Core.Domain;

/// <summary>Позиція прейскуранта: робота або запчастина.</summary>
public sealed class ServiceItem
{
    public string Code { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public decimal Price { get; init; }

    public bool IsPart { get; init; }
}
