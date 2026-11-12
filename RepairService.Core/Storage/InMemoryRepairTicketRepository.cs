using RepairService.Core.Abstractions;
using RepairService.Core.Domain;

namespace RepairService.Core.Storage;

/// <summary>
/// Сховище квитанцій у пам’яті на основі словника за номером квитанції.
/// </summary>
public sealed class InMemoryRepairTicketRepository : IRepairTicketRepository
{
    private readonly Dictionary<int, RepairTicket> _items = new();

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Квитанція з таким номером уже є.</exception>
    public void Add(RepairTicket ticket)
    {
        ArgumentNullException.ThrowIfNull(ticket);
        if (!_items.TryAdd(ticket.Id, ticket))
        {
            throw new InvalidOperationException($"Квитанція № {ticket.Id} уже існує.");
        }
    }

    /// <inheritdoc/>
    public RepairTicket? GetById(int id) => _items.GetValueOrDefault(id);

    /// <inheritdoc/>
    public IReadOnlyList<RepairTicket> GetAll() => _items.Values.ToList();
}
