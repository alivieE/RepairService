namespace RepairService.Core.Domain;

/// <summary>Заявка клієнта на ремонт пристрою.</summary>
public sealed class RepairRequest
{
    private readonly List<RepairLine> _lines = new();

    public int Id { get; init; }

    public int ClientId { get; init; }

    public string DeviceModel { get; init; } = string.Empty;

    public RepairStatus Status { get; set; } = RepairStatus.Accepted;

    public DateTimeOffset CreatedAt { get; init; }

    public IReadOnlyList<RepairLine> Lines => _lines;

    /// <summary>Додає рядок до заявки.</summary>
    public void AddLine(RepairLine line) => _lines.Add(line);

    /// <summary>Обчислює вартість ремонту.</summary>
    public decimal Total()
    {
        decimal sum = 0m;
        foreach (RepairLine line in _lines)
        {
            sum += line.Amount;
        }

        return sum;
    }
}
