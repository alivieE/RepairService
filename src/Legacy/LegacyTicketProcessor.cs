using System;
using System.Collections.Generic;

namespace RepairService.Legacy;

public sealed class Client
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Email { get; init; } = "";
    public ClientKind Kind { get; init; }
    public int DoneCount { get; init; }
    public DateTime SinceUtc { get; init; }

    public bool HasValidEmail =>
        Email is not null && Email.Contains("@");

    public bool IsLoyal =>
        DoneCount > PricingRules.LoyalOrders;
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
        ClientKind clientKind, int clientDone,
        List<WorkLine>? lines, TicketState state,
        string currency, DateTime createdAt, bool sendMail)
    {
        string? error = Validate(lines, state, clientMail);
        if (error != null) return error;

        _log.Add("ok " + ticketId);

        _tmpSum = Subtotal(lines!);
        _tmpDiscount = DiscountOf(_tmpSum, clientKind, clientDone);
        decimal ship = ShippingOf(_tmpSum - _tmpDiscount);
        decimal total = _tmpSum - _tmpDiscount + ship;

        string txt = "Документ #" + ticketId + "\n";
        txt += "Клієнт: " + clientName + "\n";
        for (int i = 0; i < lines!.Count; i++)
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

    private static string? Validate(
        List<WorkLine>? lines, TicketState state, string? clientMail)
    {
        if (lines is null) return "ERR: null";
        if (lines.Count == 0) return "ERR: empty";
        if (state is not (TicketState.New or TicketState.Paid)) return "ERR: state";
        if (clientMail is null || !clientMail.Contains("@")) return "ERR: mail";
        return null;
    }

    private static decimal Subtotal(List<WorkLine> lines)
    {
        decimal sum = 0m;
        for (int i = 0; i < lines.Count; i++)
        {
            sum += lines[i].Quantity * lines[i].UnitPrice;
        }
        return sum;
    }

    private static decimal DiscountOf(decimal subtotal, ClientKind clientKind, int clientDone)
    {
        decimal discount = 0m;
        if (clientKind == ClientKind.Vip)
        {
            discount = subtotal * PricingRules.VipRate;
            if (discount > PricingRules.MaxDiscount) discount = PricingRules.MaxDiscount;
        }
        else if (clientKind == ClientKind.Staff)
        {
            discount = subtotal * PricingRules.StaffRate;
            if (discount > PricingRules.MaxDiscount) discount = PricingRules.MaxDiscount;
        }
        else if (clientDone > PricingRules.LoyalOrders)
        {
            discount = subtotal * PricingRules.LoyalRate;
            if (discount > PricingRules.MaxDiscount) discount = PricingRules.MaxDiscount;
        }
        return discount;
    }

    private static decimal ShippingOf(decimal payable) =>
        payable < PricingRules.FreeShipFrom ? PricingRules.ShipCost : 0m;

    public decimal Preview(ClientKind clientKind, int clientDone,
        List<WorkLine> lines)
    {
        decimal s = 0m;
        foreach (var it in lines)
        {
            s += it.Quantity * it.UnitPrice;
        }
        decimal d = 0m;
        if (clientKind == ClientKind.Vip)
        {
            d = s * PricingRules.VipRate;
            if (d > PricingRules.MaxDiscount) d = PricingRules.MaxDiscount;
        }
        else if (clientKind == ClientKind.Staff)
        {
            d = s * PricingRules.StaffRate;
            if (d > PricingRules.MaxDiscount) d = PricingRules.MaxDiscount;
        }
        else if (clientDone > PricingRules.LoyalOrders)
        {
            d = s * PricingRules.LoyalRate;
            if (d > PricingRules.MaxDiscount) d = PricingRules.MaxDiscount;
        }
        decimal sh = 0m;
        if (s - d < PricingRules.FreeShipFrom) sh = PricingRules.ShipCost;
        return s - d + sh;
    }

    public string DescribeClient(Client c)
    {
        string s = c.Name.Trim().ToUpper();
        if (c.Kind == ClientKind.Vip) s += " [VIP]";
        if (c.DoneCount > PricingRules.LoyalOrders) s += " [ЛОЯЛЬНИЙ]";
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