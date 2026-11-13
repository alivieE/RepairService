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

    public string Describe()
    {
        string s = Name.Trim().ToUpper();
        if (Kind == ClientKind.Vip) s += " [VIP]";
        if (IsLoyal) s += " [ЛОЯЛЬНИЙ]";
        s += " <" + Email.ToLower() + ">";
        int years = DateTime.Now.Year - SinceUtc.Year;
        s += " стаж " + years;
        return s;
    }
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

    private readonly DiscountPolicy _discounts = new();
    private readonly ShippingPolicy _shipping = new();
    private readonly ReportBuilder _report = new();
    private readonly List<string> _log = new();

    public string Handle(TicketRequest req)
    {
        string? error = Validate(req);
        if (error != null) return error;

        _log.Add("ok " + req.TicketId);

        decimal subtotal = Subtotal(req.Lines!);
        decimal discount = _discounts.For(subtotal, req.Client);
        decimal ship = _shipping.For(subtotal - discount);
        decimal total = subtotal - discount + ship;

        if (req.SendMail)
        {
            _log.Add("mail -> " + req.Client.Email);
        }
        return _report.Build(req, discount, ship, total);
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

    public decimal Preview(Client client, IReadOnlyList<WorkLine> lines)
    {
        decimal subtotal = Subtotal(lines);
        decimal discount = _discounts.For(subtotal, client);
        decimal payable = subtotal - discount;
        return payable + _shipping.For(payable);
    }

    public string DumpLog()
    {
        string r = "";
        foreach (var l in _log) r += l + "\n";
        return r;
    }
}