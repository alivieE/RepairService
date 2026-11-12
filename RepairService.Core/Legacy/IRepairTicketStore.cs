// Успадкований код для аудиту SOLID (ЛР4, завдання 5): навмисно порушує стандарт,
// тому аналізатори в цьому файлі вимкнено. Теку Legacy буде видалено в завданні 6.
#pragma warning disable

namespace RepairService.Core.Legacy;

public interface IRepairTicketStore
{
    void Add(LegacyRepairTicket ticket);
    LegacyRepairTicket GetById(int id);
    void SaveToFile(LegacyRepairTicket ticket, string path);
    void SendEmail(string to, string body);
    string ExportCsv();
    void Backup(string folder);
    void PrintInvoice(LegacyRepairTicket ticket);
}
