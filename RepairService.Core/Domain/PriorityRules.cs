namespace RepairService.Core.Domain;

/// <summary>
/// Профільне правило варіанта: строк ремонту й надбавка на роботи
/// за пріоритетом заявки (таблиця 1 специфікації docs/spec.md).
/// </summary>
public static class PriorityRules
{
    private const int LowDays = 10;
    private const int NormalDays = 5;
    private const int HighDays = 2;
    private const int UrgentDays = 1;
    private const decimal HighSurcharge = 0.15m;
    private const decimal UrgentSurcharge = 0.30m;

    /// <summary>Повертає строк ремонту в календарних днях.</summary>
    /// <param name="priority">Пріоритет заявки.</param>
    /// <returns>Кількість днів від дати прийняття до дедлайну.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Невідоме значення пріоритету.</exception>
    public static int DaysFor(TicketPriority priority) => priority switch
    {
        TicketPriority.Low => LowDays,
        TicketPriority.Normal => NormalDays,
        TicketPriority.High => HighDays,
        TicketPriority.Urgent => UrgentDays,
        _ => throw new ArgumentOutOfRangeException(nameof(priority), priority, "Невідомий пріоритет."),
    };

    /// <summary>Повертає частку, на яку зростає вартість робіт (деталі не дорожчають).</summary>
    /// <param name="priority">Пріоритет заявки.</param>
    /// <returns>Надбавка від 0 до 0,30.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Невідоме значення пріоритету.</exception>
    public static decimal SurchargeOf(TicketPriority priority) => priority switch
    {
        TicketPriority.Low => 0m,
        TicketPriority.Normal => 0m,
        TicketPriority.High => HighSurcharge,
        TicketPriority.Urgent => UrgentSurcharge,
        _ => throw new ArgumentOutOfRangeException(nameof(priority), priority, "Невідомий пріоритет."),
    };

    /// <summary>Обчислює дедлайн: дата прийняття плюс строк ремонту.</summary>
    /// <param name="createdAt">Дата прийняття пристрою в ремонт.</param>
    /// <param name="priority">Пріоритет заявки.</param>
    /// <returns>Дата, до якої ремонт має бути завершено.</returns>
    public static DateOnly DeadlineOf(DateOnly createdAt, TicketPriority priority) =>
        createdAt.AddDays(DaysFor(priority));

    /// <summary>
    /// Обчислює вартість заявки: роботи з надбавкою за пріоритет плюс запчастини
    /// без надбавки; результат заокруглено до копійок «половина — від нуля».
    /// </summary>
    /// <param name="lines">Позиції заявки.</param>
    /// <param name="priority">Пріоритет заявки.</param>
    /// <returns>Вартість, грн; для заявки без позицій — 0,00.</returns>
    /// <exception cref="ArgumentNullException">Позиції не передано.</exception>
    public static decimal TotalOf(IEnumerable<RepairWork> lines, TicketPriority priority)
    {
        ArgumentNullException.ThrowIfNull(lines);

        decimal works = 0m;
        decimal parts = 0m;
        foreach (RepairWork line in lines)
        {
            if (line.Kind == LineKind.Part)
            {
                parts += line.Amount;
            }
            else
            {
                works += line.Amount;
            }
        }

        decimal total = (works * (1m + SurchargeOf(priority))) + parts;
        return Math.Round(total, 2, MidpointRounding.AwayFromZero);
    }
}
