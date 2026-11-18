using RepairService.Core.Abstractions;
using RepairService.Core.Domain;
using RepairService.Core.Errors;

namespace RepairService.Core.Services;

/// <summary>
/// Робота із заявкою після приймання: позиції з прейскуранта (UC-03),
/// переходи станів (UC-02) і оплата при видачі (UC-06). Кожна зміна
/// одразу зберігається у сховищі.
/// </summary>
public sealed class TicketWorkflowService
{
    private readonly IRepairTicketRepository _repository;
    private readonly Dictionary<string, ServiceItem> _catalog;

    /// <summary>Створює сервіс роботи із заявками.</summary>
    /// <param name="repository">Сховище заявок.</param>
    /// <param name="catalog">Прейскурант робіт і запчастин.</param>
    public TicketWorkflowService(IRepairTicketRepository repository, IEnumerable<ServiceItem> catalog)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(catalog);
        _repository = repository;
        _catalog = catalog.ToDictionary(i => i.Code, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Додає до заявки позицію з прейскуранта за кодом.</summary>
    /// <param name="ticketId">Номер заявки.</param>
    /// <param name="code">Код позиції прейскуранта, регістр не важливий.</param>
    /// <param name="quantity">Кількість одиниць.</param>
    /// <returns>Додана позиція або повідомлення, чому код чи кількість не прийнято.</returns>
    /// <exception cref="DomainRuleException">Заявки немає або вона вже не в стані Received.</exception>
    public Result<RepairWork> AddLine(int ticketId, string code, int quantity)
    {
        RepairTicket ticket = Find(ticketId);
        if (!_catalog.TryGetValue(code, out ServiceItem? item))
        {
            return Result.Fail<RepairWork>($"Коду «{code}» немає в прейскуранті.");
        }

        if (quantity <= 0)
        {
            return Result.Fail<RepairWork>("Кількість має бути більшою за нуль.");
        }

        LineKind kind = item.IsPart ? LineKind.Part : LineKind.Work;
        var line = new RepairWork(item.Code, quantity, item.Price, kind);
        Apply(ticket, () => ticket.AddLine(line));
        return Result.Ok(line);
    }

    /// <summary>Переводить заявку в новий стан за таблицею переходів.</summary>
    /// <param name="ticketId">Номер заявки.</param>
    /// <param name="target">Цільовий стан: InWork, Ready або Rejected.</param>
    /// <returns>Заявка після переходу.</returns>
    /// <exception cref="DomainRuleException">Заявки немає або перехід заборонено.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Цільовий стан недосяжний цим методом (видача — лише через оплату).
    /// </exception>
    public RepairTicket MoveTo(int ticketId, TicketStatus target)
    {
        RepairTicket ticket = Find(ticketId);
        Action transition = target switch
        {
            TicketStatus.InWork => ticket.StartWork,
            TicketStatus.Ready => ticket.Complete,
            TicketStatus.Rejected => ticket.Reject,
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, "Перехід недоступний."),
        };
        Apply(ticket, transition);
        return ticket;
    }

    /// <summary>Обчислює вартість заявки за пріоритетом (таблиця 1 специфікації).</summary>
    /// <param name="ticketId">Номер заявки.</param>
    /// <returns>Вартість, грн.</returns>
    /// <exception cref="DomainRuleException">Заявки немає.</exception>
    public decimal TotalOf(int ticketId)
    {
        RepairTicket ticket = Find(ticketId);
        return PriorityRules.TotalOf(ticket.Lines, ticket.Priority);
    }

    /// <summary>Приймає оплату й видає пристрій: Ready → Issued.</summary>
    /// <param name="ticketId">Номер заявки.</param>
    /// <param name="amount">Сплачена сума, грн.</param>
    /// <exception cref="DomainRuleException">
    /// Заявки немає, сума менша за вартість (правило ticket.amount) або заявка не готова.
    /// </exception>
    public void Pay(int ticketId, decimal amount)
    {
        RepairTicket ticket = Find(ticketId);
        decimal total = PriorityRules.TotalOf(ticket.Lines, ticket.Priority);
        if (amount < total)
        {
            throw new DomainRuleException(
                "ticket.amount",
                $"Сума {amount:0.00} менша за вартість {total:0.00} грн.");
        }

        Apply(ticket, ticket.Issue);
    }

    private RepairTicket Find(int ticketId) =>
        _repository.GetById(ticketId)
        ?? throw new DomainRuleException("ticket.exists", $"Заявки № {ticketId} немає.");

    private void Apply(RepairTicket ticket, Action change)
    {
        try
        {
            change();
        }
        catch (InvalidOperationException ex)
        {
            throw new DomainRuleException("ticket.transition", ex.Message, ex);
        }

        _repository.Update(ticket);
    }
}
