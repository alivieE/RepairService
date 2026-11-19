using RepairService.Core.Abstractions;
using RepairService.Core.Domain;

namespace RepairService.Core.Services;

/// <summary>
/// Запити до заявок без побічних ефектів: пошук (UC-04) і звіт
/// прострочених заявок (UC-05).
/// </summary>
public sealed class TicketQueryService
{
    private readonly IRepairTicketRepository _repository;

    /// <summary>Створює сервіс запитів до заявок.</summary>
    /// <param name="repository">Сховище заявок.</param>
    public TicketQueryService(IRepairTicketRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
    }

    /// <summary>
    /// Шукає заявки за станом і пріоритетом; незаданий критерій не обмежує пошук.
    /// </summary>
    /// <param name="status">Стан заявки або null, якщо стан не важливий.</param>
    /// <param name="priority">Пріоритет заявки або null, якщо пріоритет не важливий.</param>
    /// <returns>
    /// Знайдені заявки, упорядковані за номером; порожній список, якщо збігів немає.
    /// </returns>
    public IReadOnlyList<RepairTicket> Find(TicketStatus? status, TicketPriority? priority)
    {
        return _repository.GetAll()
            .Where(t => status is null || t.Status == status)
            .Where(t => priority is null || t.Priority == priority)
            .OrderBy(t => t.Id)
            .ToList();
    }

    /// <summary>
    /// Будує звіт прострочених заявок на дату звіту (X-05): заявки в стані
    /// Received або InWork, дедлайн яких минув.
    /// </summary>
    /// <param name="reportDate">Дата, на яку будують звіт.</param>
    /// <returns>
    /// Прострочені заявки: спершу з ранішим дедлайном, за однакового — за номером.
    /// </returns>
    public IReadOnlyList<RepairTicket> Overdue(DateOnly reportDate)
    {
        return _repository.GetAll()
            .Where(t => t.IsOverdueOn(reportDate))
            .OrderBy(t => t.Deadline)
            .ThenBy(t => t.Id)
            .ToList();
    }
}
