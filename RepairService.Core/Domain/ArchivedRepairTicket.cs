namespace RepairService.Core.Domain;

/// <summary>
/// Архівна квитанція. Не є квитанцією, а лише показує її дані:
/// методу додавання позицій немає, тож порушити контракт нічим.
/// </summary>
public sealed class ArchivedRepairTicket
{
    private readonly RepairTicket _ticket;

    /// <summary>
    /// Створює архівну квитанцію на основі наявної.
    /// </summary>
    /// <param name="ticket">Квитанція, що архівується.</param>
    /// <param name="archivedAt">Дата архівування.</param>
    public ArchivedRepairTicket(RepairTicket ticket, DateOnly archivedAt)
    {
        ArgumentNullException.ThrowIfNull(ticket);
        _ticket = ticket;
        ArchivedAt = archivedAt;
    }

    /// <summary>Дата архівування.</summary>
    public DateOnly ArchivedAt { get; }

    /// <summary>Номер квитанції.</summary>
    public int Id => _ticket.Id;

    /// <summary>Стан квитанції на момент архівування.</summary>
    public TicketStatus Status => _ticket.Status;

    /// <summary>Позиції квитанції лише для читання.</summary>
    public IReadOnlyList<RepairWork> Lines => _ticket.Lines;
}
