using RepairService.Core.Abstractions;
using RepairService.Core.Domain;

namespace RepairService.Tests;

/// <summary>Фейк сховища: робоча реалізація на словнику в пам’яті з лічильником викликів.</summary>
internal sealed class FakeRepairTicketRepository : IRepairTicketRepository
{
    private readonly Dictionary<int, RepairTicket> _items = new();

    public int AddCalls { get; private set; }

    public void Add(RepairTicket ticket)
    {
        _items[ticket.Id] = ticket;
        AddCalls++;
    }

    public RepairTicket? GetById(int id) => _items.GetValueOrDefault(id);

    public IReadOnlyList<RepairTicket> GetAll() => _items.Values.ToList();
}
