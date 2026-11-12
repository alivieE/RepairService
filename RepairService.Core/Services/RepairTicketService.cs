using RepairService.Core.Abstractions;
using RepairService.Core.Domain;

namespace RepairService.Core.Services;

/// <summary>
/// Прикладний сервіс квитанцій: приймає квитанції й рахує їхню вартість.
/// Залежності отримує в конструкторі як інтерфейси.
/// </summary>
public sealed class RepairTicketService
{
    private readonly IRepairTicketRepository _repository;
    private readonly IPricingPolicy _pricing;

    /// <summary>
    /// Створює сервіс із переданими залежностями.
    /// </summary>
    /// <param name="repository">Сховище квитанцій.</param>
    /// <param name="pricing">Політика ціноутворення.</param>
    public RepairTicketService(IRepairTicketRepository repository, IPricingPolicy pricing)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(pricing);
        _repository = repository;
        _pricing = pricing;
    }

    /// <summary>Приймає квитанцію: зберігає її у сховищі.</summary>
    /// <param name="ticket">Квитанція на ремонт.</param>
    public void Place(RepairTicket ticket)
    {
        ArgumentNullException.ThrowIfNull(ticket);
        _repository.Add(ticket);
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
