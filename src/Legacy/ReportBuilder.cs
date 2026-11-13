using System.Text;

namespace RepairService.Legacy;

public sealed class ReportBuilder
{
    public string Build(TicketRequest req, decimal discount, decimal shipping, decimal total)
    {
        var sb = new StringBuilder();
        sb.Append("Документ #").Append(req.TicketId).Append('\n');
        sb.Append("Клієнт: ").Append(req.Client.Name).Append('\n');
        foreach (WorkLine line in req.Lines!)
        {
            sb.Append(Line(line, req.Currency));
        }
        sb.Append(Money("Знижка", discount, req.Currency));
        sb.Append(Money("Доставка", shipping, req.Currency));
        sb.Append(Money("Разом", total, req.Currency));
        return sb.ToString();
    }

    private static string Line(WorkLine line, string currency) =>
        line.WorkCode + " x" + line.Quantity + " = " +
        (line.Quantity * line.UnitPrice).ToString("0.00") + " " + currency + "\n";

    private static string Money(string label, decimal value, string currency) =>
        label + ": " + value.ToString("0.00") + " " + currency + "\n";
}