using RepairService.Core.Domain;

namespace RepairService.Core.Storage;

/// <summary>
/// Заявка у форматі файлу JSON. Стан відновлюється повторенням дозволених
/// переходів, тому файл не може обійти таблицю станів заявки.
/// </summary>
internal sealed class TicketRecord
{
    /// <summary>Номер заявки.</summary>
    public int Id { get; init; }

    /// <summary>Ідентифікатор клієнта.</summary>
    public int CustomerId { get; init; }

    /// <summary>Модель пристрою.</summary>
    public string DeviceModel { get; init; } = string.Empty;

    /// <summary>Дата прийняття.</summary>
    public DateOnly CreatedAt { get; init; }

    /// <summary>Пріоритет заявки.</summary>
    public TicketPriority Priority { get; init; }

    /// <summary>Стан заявки на момент збереження.</summary>
    public TicketStatus Status { get; init; }

    /// <summary>Позиції заявки.</summary>
    public List<LineRecord> Lines { get; init; } = new();

    /// <summary>Перетворює заявку домену на запис файлу.</summary>
    /// <param name="ticket">Заявка на ремонт.</param>
    /// <returns>Запис для збереження.</returns>
    public static TicketRecord From(RepairTicket ticket)
    {
        return new TicketRecord
        {
            Id = ticket.Id,
            CustomerId = ticket.CustomerId,
            DeviceModel = ticket.DeviceModel,
            CreatedAt = ticket.CreatedAt,
            Priority = ticket.Priority,
            Status = ticket.Status,
            Lines = ticket.Lines
                .Select(l => new LineRecord
                {
                    Code = l.Code,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice,
                    Kind = l.Kind,
                })
                .ToList(),
        };
    }

    /// <summary>Відновлює заявку домену з запису файлу.</summary>
    /// <returns>Заявка в збереженому стані.</returns>
    /// <exception cref="InvalidOperationException">
    /// Збережений стан недосяжний для збережених позицій.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">Невідомий стан у файлі.</exception>
    public RepairTicket ToTicket()
    {
        var ticket = new RepairTicket(Id, CustomerId, DeviceModel, CreatedAt, Priority);
        foreach (LineRecord line in Lines)
        {
            ticket.AddLine(new RepairWork(line.Code, line.Quantity, line.UnitPrice, line.Kind));
        }

        switch (Status)
        {
            case TicketStatus.Received:
                break;
            case TicketStatus.InWork:
                ticket.StartWork();
                break;
            case TicketStatus.Ready:
                ticket.StartWork();
                ticket.Complete();
                break;
            case TicketStatus.Issued:
                ticket.StartWork();
                ticket.Complete();
                ticket.Issue();
                break;
            case TicketStatus.Rejected:
                ticket.Reject();
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(Status),
                    Status,
                    "Невідомий стан заявки.");
        }

        return ticket;
    }
}
