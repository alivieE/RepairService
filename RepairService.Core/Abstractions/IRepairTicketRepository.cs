using RepairService.Core.Domain;

namespace RepairService.Core.Abstractions;

/// <summary>
/// Сховище квитанцій на ремонт. Відповідає лише за зберігання.
/// </summary>
public interface IRepairTicketRepository
{
    /// <summary>Додає квитанцію до сховища.</summary>
    /// <param name="ticket">Квитанція на ремонт.</param>
    void Add(RepairTicket ticket);

    /// <summary>Шукає квитанцію за номером.</summary>
    /// <param name="id">Номер квитанції.</param>
    /// <returns>Знайдена квитанція або null.</returns>
    RepairTicket? GetById(int id);

    /// <summary>Повертає всі квитанції.</summary>
    /// <returns>Список квитанцій лише для читання.</returns>
    IReadOnlyList<RepairTicket> GetAll();
}
