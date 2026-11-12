namespace RepairService.Core.Domain;

/// <summary>Позиція прейскуранта: робота або запчастина.</summary>
public sealed class ServiceItem
{
    /// <summary>Код позиції в прейскуранті.</summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>Назва роботи або запчастини.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Ціна за одиницю, грн.</summary>
    public decimal Price { get; init; }

    /// <summary>Ознака запчастини (false — робота).</summary>
    public bool IsPart { get; init; }
}
