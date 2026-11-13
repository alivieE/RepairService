using RepairService.Core.Abstractions;
using RepairService.Core.Domain;
using RepairService.Core.Errors;

namespace RepairService.Core.Services;

/// <summary>
/// Прикладний сервіс квитанцій: приймає квитанції, рахує їхню вартість
/// і сповіщає клієнта. Залежності отримує в конструкторі як інтерфейси.
/// </summary>
public sealed class RepairTicketService
{
    private readonly IRepairTicketRepository _repository;
    private readonly IPricingPolicy _pricing;
    private readonly INotifier _notifier;

    /// <summary>
    /// Створює сервіс із переданими залежностями.
    /// </summary>
    /// <param name="repository">Сховище квитанцій.</param>
    /// <param name="pricing">Політика ціноутворення.</param>
    /// <param name="notifier">Сповіщувач клієнта.</param>
    public RepairTicketService(
        IRepairTicketRepository repository,
        IPricingPolicy pricing,
        INotifier notifier)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(pricing);
        ArgumentNullException.ThrowIfNull(notifier);
        _repository = repository;
        _pricing = pricing;
        _notifier = notifier;
    }

    /// <summary>Приймає квитанцію: зберігає її і сповіщає клієнта.</summary>
    /// <param name="ticket">Квитанція на ремонт.</param>
    public void Place(RepairTicket ticket)
    {
        ArgumentNullException.ThrowIfNull(ticket);
        _repository.Add(ticket);
        _notifier.Notify($"Квитанцію № {ticket.Id} прийнято в ремонт.");
    }

    /// <summary>Обчислює вартість квитанції за політикою ціноутворення.</summary>
    /// <param name="ticket">Квитанція на ремонт.</param>
    /// <returns>Вартість квитанції, грн.</returns>
    public decimal TotalOf(RepairTicket ticket)
    {
        ArgumentNullException.ThrowIfNull(ticket);
        decimal total = 0m;
        foreach (RepairWork line in ticket.Lines)
        {
            total += _pricing.PriceOf(line);
        }

        return total;
    }

    /// <summary>Додає перевірену позицію до квитанції.</summary>
    /// <param name="ticketId">Номер квитанції.</param>
    /// <param name="line">Позиція, вже розібрана барикадою.</param>
    /// <exception cref="DomainRuleException">
    /// Квитанцію не знайдено або вона вже не редагується.
    /// </exception>
    public void AddLine(int ticketId, RepairWork line)
    {
        ArgumentNullException.ThrowIfNull(line);
        RepairTicket ticket = Load(ticketId);
        if (ticket.Status != TicketStatus.Received)
        {
            throw new DomainRuleException(
                "ticket.editable",
                $"Квитанція № {ticketId} у стані {ticket.Status} не редагується.");
        }

        ticket.AddLine(line);
    }

    /// <summary>Приймає оплату при видачі пристрою: Ready → Issued.</summary>
    /// <param name="ticketId">Номер квитанції.</param>
    /// <param name="amount">Сплачена сума, грн.</param>
    /// <exception cref="DomainRuleException">
    /// Квитанцію не знайдено, її стан не дозволяє оплату або сплачено замало.
    /// </exception>
    public void Pay(int ticketId, decimal amount)
    {
        RepairTicket ticket = Load(ticketId);
        if (ticket.Status != TicketStatus.Ready)
        {
            throw new DomainRuleException(
                "ticket.payable",
                $"Стан {ticket.Status} не дозволяє оплату квитанції № {ticketId}.");
        }

        decimal total = TotalOf(ticket);
        if (amount < total)
        {
            throw new DomainRuleException(
                "ticket.amount",
                $"Сплачено {amount:0.00} грн, а треба {total:0.00} грн.");
        }

        ticket.Issue();
    }

    /// <summary>Завантажує квитанцію за номером.</summary>
    /// <param name="ticketId">Номер квитанції.</param>
    /// <returns>Знайдена квитанція.</returns>
    /// <exception cref="DomainRuleException">Квитанцію не знайдено.</exception>
    public RepairTicket Load(int ticketId)
    {
        return _repository.GetById(ticketId)
            ?? throw new DomainRuleException(
                "ticket.exists",
                $"Квитанцію № {ticketId} не знайдено.");
    }

    /// <summary>Відмовляє в ремонті; дозволено лише до завершення ремонту.</summary>
    /// <param name="ticketId">Номер квитанції.</param>
    /// <param name="reason">Причина відмови.</param>
    /// <exception cref="DomainRuleException">
    /// Квитанцію не знайдено або її стан не дозволяє відмову.
    /// </exception>
    public void Cancel(int ticketId, string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        RepairTicket ticket = Load(ticketId);
        try
        {
            ticket.Reject();
        }
        catch (InvalidOperationException ex)
        {
            throw new DomainRuleException(
                "ticket.cancellable",
                $"Квитанцію № {ticketId} у стані {ticket.Status} скасувати не можна.",
                ex);
        }
    }

    /// <summary>Зберігає звіт про квитанцію у файл.</summary>
    /// <param name="path">Шлях до файлу звіту.</param>
    /// <param name="ticket">Квитанція на ремонт.</param>
    public void ExportReport(string path, RepairTicket ticket)
    {
        var writer = new StreamWriter(path);
        writer.WriteLine($"Квитанція {ticket.Id}");
        writer.WriteLine(TotalOf(ticket));
        writer.Close();
    }
}
