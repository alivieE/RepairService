namespace RepairService.Core.Domain;

/// <summary>Заявка клієнта на ремонт пристрою.</summary>
public sealed class RepairRequest
{
    private readonly List<RepairLine> _lines = new();

    /// <summary>Номер заявки.</summary>
    public int Id { get; init; }

    /// <summary>Ідентифікатор клієнта.</summary>
    public int ClientId { get; init; }

    /// <summary>Модель пристрою, зданого в ремонт.</summary>
    public string DeviceModel { get; init; } = string.Empty;

    /// <summary>Поточний стан заявки.</summary>
    public RepairStatus Status { get; set; } = RepairStatus.Accepted;

    /// <summary>Момент створення заявки.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Рядки заявки лише для читання.</summary>
    public IReadOnlyList<RepairLine> Lines => _lines;

    /// <summary>Додає рядок до заявки.</summary>
    /// <param name="line">Рядок із роботою або запчастиною.</param>
    public void AddLine(RepairLine line) => _lines.Add(line);

    /// <summary>Обчислює вартість ремонту.</summary>
    /// <returns>Сума за всіма рядками заявки.</returns>
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
