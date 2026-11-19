namespace RepairService.Core.Domain;

/// <summary>
/// Квитанція на ремонт. Агрегат: позиції приховані, а стан змінюють
/// лише методи-переходи за таблицею станів.
/// </summary>
public sealed class RepairTicket
{
    private readonly List<RepairWork> _lines = new();

    /// <summary>
    /// Створює квитанцію у стані Received і перевіряє її інваріант.
    /// </summary>
    /// <param name="id">Номер квитанції, більший за нуль.</param>
    /// <param name="customerId">Ідентифікатор клієнта, більший за нуль.</param>
    /// <param name="deviceModel">Модель пристрою, не порожня.</param>
    /// <param name="createdAt">Дата оформлення квитанції.</param>
    /// <param name="priority">Пріоритет заявки; від нього залежить дедлайн (X-02).</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Номер чи клієнт не додатні або пріоритет невідомий.
    /// </exception>
    /// <exception cref="ArgumentException">Модель пристрою порожня.</exception>
    public RepairTicket(
        int id,
        int customerId,
        string deviceModel,
        DateOnly createdAt,
        TicketPriority priority = TicketPriority.Normal)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(customerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceModel);

        Id = id;
        CustomerId = customerId;
        DeviceModel = deviceModel;
        CreatedAt = createdAt;
        Priority = priority;
        Deadline = PriorityRules.DeadlineOf(createdAt, priority);
        Status = TicketStatus.Received;
    }

    /// <summary>Номер квитанції.</summary>
    public int Id { get; }

    /// <summary>Ідентифікатор клієнта.</summary>
    public int CustomerId { get; }

    /// <summary>Модель пристрою, зданого в ремонт.</summary>
    public string DeviceModel { get; }

    /// <summary>Дата оформлення квитанції.</summary>
    public DateOnly CreatedAt { get; }

    /// <summary>Пріоритет заявки.</summary>
    public TicketPriority Priority { get; }

    /// <summary>Останній день строку ремонту включно.</summary>
    public DateOnly Deadline { get; }

    /// <summary>Поточний стан; змінюється лише методами-переходами.</summary>
    public TicketStatus Status { get; private set; }

    /// <summary>Позиції квитанції лише для читання.</summary>
    public IReadOnlyList<RepairWork> Lines => _lines.AsReadOnly();

    /// <summary>
    /// Перевіряє, чи заявка прострочена на дату звіту: ремонт ще не завершено,
    /// а дата звіту пізніша за дедлайн (X-05). Запит без побічних ефектів.
    /// </summary>
    /// <param name="reportDate">Дата, на яку будують звіт.</param>
    /// <returns>true, якщо заявка в стані Received або InWork і дедлайн минув.</returns>
    public bool IsOverdueOn(DateOnly reportDate) =>
        reportDate > Deadline && Status is TicketStatus.Received or TicketStatus.InWork;

    /// <summary>Додає позицію; дозволено лише у стані Received.</summary>
    /// <param name="line">Позиція з роботою.</param>
    /// <exception cref="InvalidOperationException">Квитанція вже не у стані Received.</exception>
    public void AddLine(RepairWork line)
    {
        ArgumentNullException.ThrowIfNull(line);
        if (Status != TicketStatus.Received)
        {
            throw new InvalidOperationException("Позиції додають лише до нової квитанції.");
        }

        _lines.Add(line);
    }

    /// <summary>Починає ремонт після погодження вартості: Received → InWork.</summary>
    /// <exception cref="InvalidOperationException">
    /// Перехід заборонено або квитанція не містить позицій.
    /// </exception>
    public void StartWork()
    {
        EnsureStatus(TicketStatus.Received);
        if (_lines.Count == 0)
        {
            throw new InvalidOperationException("Квитанцію без позицій погодити не можна.");
        }

        Status = TicketStatus.InWork;
    }

    /// <summary>Завершує ремонт: InWork → Ready.</summary>
    /// <exception cref="InvalidOperationException">Перехід заборонено.</exception>
    public void Complete()
    {
        EnsureStatus(TicketStatus.InWork);
        Status = TicketStatus.Ready;
    }

    /// <summary>Видає пристрій клієнту: Ready → Issued.</summary>
    /// <exception cref="InvalidOperationException">Перехід заборонено.</exception>
    public void Issue()
    {
        EnsureStatus(TicketStatus.Ready);
        Status = TicketStatus.Issued;
    }

    /// <summary>Відмовляє в ремонті; дозволено у станах Received і InWork.</summary>
    /// <exception cref="InvalidOperationException">Перехід заборонено.</exception>
    public void Reject()
    {
        if (Status is not (TicketStatus.Received or TicketStatus.InWork))
        {
            throw new InvalidOperationException("Відмовити можна лише до завершення ремонту.");
        }

        Status = TicketStatus.Rejected;
    }

    private void EnsureStatus(TicketStatus expected)
    {
        if (Status != expected)
        {
            throw new InvalidOperationException(
                $"Перехід зі стану {Status} заборонено, очікувався стан {expected}.");
        }
    }
}
