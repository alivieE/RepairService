using System;
using System.Collections.Generic;

namespace RepairService.Legacy;

public class Client
{
    public int Id;
    public string Name = "";
    public string Email = "";
    public string Kind = "regular";
    public int DoneCount;
    public DateTime SinceUtc;
}

public class WorkLine
{
    public string WorkCode = "";
    public int Quantity;
    public decimal UnitPrice;
    public string Category = "";
}

public class LegacyTicketProcessor
{
    private decimal _tmpSum;
    private decimal _tmpDiscount;
    private readonly List<string> _log = new();

    public string Handle(int ticketId, int clientId,
        string clientName, string? clientMail,
        string clientKind, int clientDone,
        List<WorkLine>? lines, string state,
        string currency, DateTime createdAt, bool sendMail)
    {
        if (lines != null)
        {
            if (lines.Count > 0)
            {
                if (state == "new" || state == "paid")
                {
                    if (clientMail != null &&
                        clientMail.Contains("@"))
                    {
                        _log.Add("ok " + ticketId);
                    }
                    else
                    {
                        return "ERR: mail";
                    }
                }
                else
                {
                    return "ERR: state";
                }
            }
            else
            {
                return "ERR: empty";
            }
        }
        else
        {
            return "ERR: null";
        }

        _tmpSum = 0m;
        for (int i = 0; i < lines.Count; i++)
        {
            _tmpSum += lines[i].Quantity * lines[i].UnitPrice;
        }

        _tmpDiscount = 0m;
        if (clientKind == "vip")
        {
            _tmpDiscount = _tmpSum * 0.15m;
            if (_tmpDiscount > 500m) _tmpDiscount = 500m;
        }
        else if (clientKind == "staff")
        {
            _tmpDiscount = _tmpSum * 0.30m;
            if (_tmpDiscount > 500m) _tmpDiscount = 500m;
        }
        else if (clientDone > 10)
        {
            _tmpDiscount = _tmpSum * 0.05m;
            if (_tmpDiscount > 500m) _tmpDiscount = 500m;
        }

        decimal ship = 0m;
        if (_tmpSum - _tmpDiscount < 1000m) ship = 60m;
        decimal total = _tmpSum - _tmpDiscount + ship;

        string txt = "Документ #" + ticketId + "\n";
        txt += "Клієнт: " + clientName + "\n";
        for (int i = 0; i < lines.Count; i++)
        {
            txt += lines[i].WorkCode + " x" + lines[i].Quantity + " = " +
                (lines[i].Quantity * lines[i].UnitPrice)
                    .ToString("0.00") + " " + currency + "\n";
        }
        txt += "Знижка: " + _tmpDiscount.ToString("0.00") +
            " " + currency + "\n";
        txt += "Доставка: " + ship.ToString("0.00") +
            " " + currency + "\n";
        txt += "Разом: " + total.ToString("0.00") +
            " " + currency + "\n";

        if (sendMail)
        {
            _log.Add("mail -> " + clientMail);
        }
        return txt;
    }

    public decimal Preview(string clientKind, int clientDone,
        List<WorkLine> lines)
    {
        decimal s = 0m;
        foreach (var it in lines)
        {
            s += it.Quantity * it.UnitPrice;
        }
        decimal d = 0m;
        if (clientKind == "vip")
        {
            d = s * 0.15m;
            if (d > 500m) d = 500m;
        }
        else if (clientKind == "staff")
        {
            d = s * 0.30m;
            if (d > 500m) d = 500m;
        }
        else if (clientDone > 10)
        {
            d = s * 0.05m;
            if (d > 500m) d = 500m;
        }
        decimal sh = 0m;
        if (s - d < 1000m) sh = 60m;
        return s - d + sh;
    }

    public string DescribeClient(Client c)
    {
        string s = c.Name.Trim().ToUpper();
        if (c.Kind == "vip") s += " [VIP]";
        if (c.DoneCount > 10) s += " [ЛОЯЛЬНИЙ]";
        s += " <" + c.Email.ToLower() + ">";
        int years = DateTime.Now.Year - c.SinceUtc.Year;
        s += " стаж " + years;
        return s;
    }

    public string DumpLog()
    {
        string r = "";
        foreach (var l in _log) r += l + "\n";
        return r;
    }
}