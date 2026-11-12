using System.Globalization;
using System.Text;
using RepairService.Core.Domain;

namespace RepairService.Core.Reporting;

/// <summary>
/// Форматування звіту про квитанції у CSV. Нічого не пише на диск,
/// а лише повертає текст.
/// </summary>
public static class RepairTicketCsvReport
{
    /// <summary>
    /// Формує CSV-звіт: номер, сума й стан кожної квитанції.
    /// </summary>
    /// <param name="tickets">Квитанції для звіту.</param>
    /// <param name="totalOf">Спосіб обчислення суми квитанції.</param>
    /// <returns>Текст звіту у форматі CSV.</returns>
    public static string BuildCsv(
        IEnumerable<RepairTicket> tickets,
        Func<RepairTicket, decimal> totalOf)
    {
        ArgumentNullException.ThrowIfNull(tickets);
        ArgumentNullException.ThrowIfNull(totalOf);
        CultureInfo culture = CultureInfo.InvariantCulture;
        StringBuilder text = new();
        text.AppendLine("id;total;status");
        foreach (RepairTicket ticket in tickets)
        {
            text.AppendLine(culture, $"{ticket.Id};{totalOf(ticket):F2};{ticket.Status}");
        }

        return text.ToString();
    }
}
