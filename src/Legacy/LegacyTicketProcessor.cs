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

    public string Handle(TicketRequest req)
    {
        string? error = Validate(req);
        if (error != null) return error;

        _log.Add("ok " + req.TicketId);

        _tmpSum = Subtotal(req.Lines!);
        _tmpDiscount = DiscountOf(_tmpSum, req.Client);
        decimal ship = ShippingOf(_tmpSum - _tmpDiscount);
        decimal total = _tmpSum - _tmpDiscount + ship;

        string txt = "Документ #" + req.TicketId + "\n";
        txt += "Клієнт: " + req.Client.Name + "\n";
        for (int i = 0; i < req.Lines!.Count; i++)
        {
            txt += req.Lines[i].WorkCode + " x" + req.Lines[i].Quantity + " = " +
                (req.Lines[i].Quantity * req.Lines[i].UnitPrice)
                    .ToString("0.00") + " " + req.Currency + "\n";
        }
        txt += "Знижка: " + _tmpDiscount.ToString("0.00") +
            " " + req.Currency + "\n";
        txt += "Доставка: " + ship.ToString("0.00") +
            " " + req.Currency + "\n";
        txt += "Разом: " + total.ToString("0.00") +
            " " + req.Currency + "\n";

        if (req.SendMail)
        {
            _log.Add("mail -> " + req.Client.Email);
        }
        return txt;
    }

    private static string? Validate(TicketRequest req)
    {
        if (req.Lines is null) return "ERR: null";
        if (req.Lines.Count == 0) return "ERR: empty";
        if (req.State is not (TicketState.New or TicketState.Paid)) return "ERR: state";
        if (!req.Client.HasValidEmail) return "ERR: mail";
        return null;
    }

    private static decimal Subtotal(IReadOnlyList<WorkLine> lines)
    {
        decimal sum = 0m;
        for (int i = 0; i < lines.Count; i++)
        {
            sum += lines[i].Quantity * lines[i].UnitPrice;
        }
        return sum;
    }

    private static decimal DiscountOf(decimal subtotal, Client client)
    {
        decimal discount = 0m;
        if (client.Kind == ClientKind.Vip)
        {
            discount = subtotal * PricingRules.VipRate;
            if (discount > PricingRules.MaxDiscount) discount = PricingRules.MaxDiscount;
        }
        else if (client.Kind == ClientKind.Staff)
        {
            discount = subtotal * PricingRules.StaffRate;
            if (discount > PricingRules.MaxDiscount) discount = PricingRules.MaxDiscount;
        }
        else if (client.IsLoyal)
        {
            discount = subtotal * PricingRules.LoyalRate;
            if (discount > PricingRules.MaxDiscount) discount = PricingRules.MaxDiscount;
        }
        return discount;
    }

    private static decimal ShippingOf(decimal payable) =>
        payable < PricingRules.FreeShipFrom ? PricingRules.ShipCost : 0m;

    public decimal Preview(Client client, IReadOnlyList<WorkLine> lines)
    {
        decimal subtotal = Subtotal(lines);
        decimal discount = DiscountOf(subtotal, client);
        decimal payable = subtotal - discount;
        return payable + ShippingOf(payable);
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