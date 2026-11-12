// Успадкований код для аудиту SOLID (ЛР4, завдання 5): навмисно порушує стандарт,
// тому аналізатори в цьому файлі вимкнено. Теку Legacy буде видалено в завданні 6.
#pragma warning disable

namespace RepairService.Core.Legacy;

public class LegacyRepairWork
{
    public string Code { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class LegacyRepairTicket
{
    public int Id { get; set; }
    public int Status { get; set; }
    public decimal Total { get; set; }
    public string Email { get; set; } = "";
    public List<LegacyRepairWork> Lines { get; set; } = new();

    public virtual void AddLine(LegacyRepairWork line) => Lines.Add(line);
}

public class ArchivedRepairTicket : LegacyRepairTicket
{
    public ArchivedRepairTicket(int id) => Id = id;

    public override void AddLine(LegacyRepairWork line)
    {
        throw new NotSupportedException("Архів змінювати не можна");
    }
}
