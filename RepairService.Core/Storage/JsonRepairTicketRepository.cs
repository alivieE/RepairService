using System.Text.Json;
using System.Text.Json.Serialization;
using RepairService.Core.Abstractions;
using RepairService.Core.Domain;

namespace RepairService.Core.Storage;

/// <summary>
/// Сховище заявок у файлі JSON. Кожна операція читає файл заново, тому
/// дані не розходяться з диском між запусками застосунку.
/// </summary>
public sealed class JsonRepairTicketRepository : IRepairTicketRepository
{
    private readonly string _path;
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>Створює сховище для заданого файлу.</summary>
    /// <param name="path">Шлях до файлу JSON; файл може ще не існувати.</param>
    /// <exception cref="ArgumentException">Шлях порожній.</exception>
    public JsonRepairTicketRepository(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _path = path;
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Заявка з таким номером уже є.</exception>
    public void Add(RepairTicket ticket)
    {
        ArgumentNullException.ThrowIfNull(ticket);
        List<TicketRecord> records = Load();
        if (records.Exists(r => r.Id == ticket.Id))
        {
            throw new InvalidOperationException($"Заявка № {ticket.Id} уже існує.");
        }

        records.Add(TicketRecord.From(ticket));
        Save(records);
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Заявки з таким номером немає.</exception>
    public void Update(RepairTicket ticket)
    {
        ArgumentNullException.ThrowIfNull(ticket);
        List<TicketRecord> records = Load();
        int index = records.FindIndex(r => r.Id == ticket.Id);
        if (index < 0)
        {
            throw new InvalidOperationException($"Заявки № {ticket.Id} немає.");
        }

        records[index] = TicketRecord.From(ticket);
        Save(records);
    }

    /// <inheritdoc/>
    public RepairTicket? GetById(int id) =>
        Load().Find(r => r.Id == id)?.ToTicket();

    /// <inheritdoc/>
    public IReadOnlyList<RepairTicket> GetAll() =>
        Load().Select(r => r.ToTicket()).ToList();

    private List<TicketRecord> Load()
    {
        if (!File.Exists(_path))
        {
            return new List<TicketRecord>();
        }

        string json = File.ReadAllText(_path);
        return JsonSerializer.Deserialize<List<TicketRecord>>(json, _options)
            ?? new List<TicketRecord>();
    }

    private void Save(List<TicketRecord> records)
    {
        string json = JsonSerializer.Serialize(records, _options);
        File.WriteAllText(_path, json);
    }
}
