// Успадкований код для аудиту SOLID (ЛР4, завдання 5): навмисно порушує стандарт,
// тому аналізатори в цьому файлі вимкнено. Теку Legacy буде видалено в завданні 6.
#pragma warning disable

namespace RepairService.Core.Legacy;

public class RepairTicketManager
{
    private readonly FileRepairTicketStore _store = new FileRepairTicketStore();
    private readonly SmtpMailer _mailer = new SmtpMailer();

    public decimal Place(LegacyRepairTicket ticket, string customerType)
    {
        decimal total = 0m;
        foreach (var line in ticket.Lines)
            total += line.UnitPrice * line.Quantity;

        if (customerType == "regular")
            total = total * 0.95m;
        else if (customerType == "vip")
            total = total * 0.90m;
        else if (customerType == "staff")
            total = total * 0.70m;

        if (total > 1000m)
            total = total - 50m;

        ticket.Total = total;
        ticket.Status = 1;
        _store.SaveToFile(ticket, "tickets.json");
        _mailer.Send(ticket.Email, "Ticket " + ticket.Id, "" + total);
        return total;
    }

    public string BuildCsvReport(List<LegacyRepairTicket> tickets)
    {
        var text = "id;total;status" + Environment.NewLine;
        foreach (var t in tickets)
            text += t.Id + ";" + t.Total + ";" + t.Status;

        File.WriteAllText("report.csv", text);
        Console.WriteLine("Звіт збережено");
        return text;
    }

    public void Archive(LegacyRepairTicket ticket)
    {
        LegacyRepairTicket archived = new ArchivedRepairTicket(ticket.Id);
        foreach (var line in ticket.Lines)
            archived.AddLine(line);

        _store.SaveToFile(archived, "archive.json");
    }
}
