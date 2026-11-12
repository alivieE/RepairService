using RepairService.Core.Abstractions;
using RepairService.Core.Domain;

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
}
