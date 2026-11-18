using RepairService.Core.Domain;

namespace RepairService.Core.Storage;

/// <summary>Позиція заявки у форматі файлу JSON.</summary>
internal sealed class LineRecord
{
    /// <summary>Код позиції за прейскурантом.</summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>Кількість одиниць.</summary>
    public int Quantity { get; init; }

    /// <summary>Ціна за одиницю, грн.</summary>
    public decimal UnitPrice { get; init; }

    /// <summary>Вид позиції: робота чи запчастина.</summary>
    public LineKind Kind { get; init; }
}
