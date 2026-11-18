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
    /// <exception cref="InvalidOperationException">Квитанції з таким номером немає.</exception>
    public void Update(RepairTicket ticket)
    {
        ArgumentNullException.ThrowIfNull(ticket);
        if (!_items.ContainsKey(ticket.Id))
        {
            throw new InvalidOperationException($"Квитанції № {ticket.Id} немає.");
        }

        _items[ticket.Id] = ticket;
    }

    /// <inheritdoc/>
    public RepairTicket? GetById(int id) => _items.GetValueOrDefault(id);

    /// <inheritdoc/>
    public IReadOnlyList<RepairTicket> GetAll() => _items.Values.ToList();
}
