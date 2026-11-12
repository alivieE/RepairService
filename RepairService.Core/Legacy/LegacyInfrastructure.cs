// Успадкований код для аудиту SOLID (ЛР4, завдання 5): мінімальні заглушки,
// яких немає в методичці, але без яких клас-Бог не компілюється.
#pragma warning disable

namespace RepairService.Core.Legacy;

public class FileRepairTicketStore : IRepairTicketStore
{
    public void SaveToFile(LegacyRepairTicket ticket, string path)
    {
        File.WriteAllText(path, ticket.Id + ";" + ticket.Total + ";" + ticket.Status);
    }

    public void Add(LegacyRepairTicket ticket) => throw new NotSupportedException();
    public LegacyRepairTicket GetById(int id) => throw new NotSupportedException();
    public void SendEmail(string to, string body) => throw new NotSupportedException();
    public string ExportCsv() => throw new NotSupportedException();
    public void Backup(string folder) => throw new NotSupportedException();
    public void PrintInvoice(LegacyRepairTicket ticket) => throw new NotSupportedException();
}

public class SmtpMailer
{
    public void Send(string to, string subject, string body)
    {
        // заглушка: справжнє надсилання листа через SMTP
    }
}
