using RepairService.Core.Abstractions;
using RepairService.Core.Domain;

namespace RepairService.Core.Services;

/// <summary>
/// Приймання пристрою в ремонт (UC-01): оформлює нову заявку з датою
/// прийняття, пріоритетом і дедлайном та зберігає її.
/// </summary>
public sealed class TicketIntakeService
{
    private readonly IRepairTicketRepository _repository;
    private readonly IClock _clock;

    /// <summary>Створює сервіс приймання заявок.</summary>
    /// <param name="repository">Сховище заявок.</param>
    /// <param name="clock">Джерело поточної дати.</param>
    public TicketIntakeService(IRepairTicketRepository repository, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(clock);
        _repository = repository;
        _clock = clock;
    }

    /// <summary>Приймає пристрій у ремонт і повертає нову заявку в стані Received.</summary>
    /// <param name="customerId">Ідентифікатор клієнта, більший за нуль.</param>
    /// <param name="deviceModel">Модель пристрою, не порожня.</param>
    /// <param name="priority">Пріоритет заявки.</param>
    /// <returns>Збережена заявка з наступним вільним номером.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Клієнт не додатний або пріоритет невідомий.
    /// </exception>
    /// <exception cref="ArgumentException">Модель пристрою порожня.</exception>
    public RepairTicket Accept(int customerId, string deviceModel, TicketPriority priority)
    {
        int nextId = _repository.GetAll().Count + 1;
        var ticket = new RepairTicket(nextId, customerId, deviceModel, _clock.Today, priority);
        _repository.Add(ticket);
        return ticket;
    }
}
