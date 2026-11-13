namespace RepairService.Core.Domain;

/// <summary>Пріоритет заявки: визначає строк ремонту й надбавку на роботи.</summary>
public enum TicketPriority
{
    /// <summary>Низький: ремонт без поспіху.</summary>
    Low,

    /// <summary>Звичайний пріоритет.</summary>
    Normal,

    /// <summary>Високий: ремонт прискорено, роботи дорожчі.</summary>
    High,

    /// <summary>Терміновий: ремонт за добу, найбільша надбавка.</summary>
    Urgent,
}
