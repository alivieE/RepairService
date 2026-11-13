namespace RepairService.Legacy;

public interface IDiscountRule
{
    decimal? RateFor(Client client);
}

public sealed class VipDiscountRule : IDiscountRule
{
    public decimal? RateFor(Client client) =>
        client.Kind == ClientKind.Vip ? PricingRules.VipRate : null;
}

public sealed class StaffDiscountRule : IDiscountRule
{
    public decimal? RateFor(Client client) =>
        client.Kind == ClientKind.Staff ? PricingRules.StaffRate : null;
}

public sealed class LoyalDiscountRule : IDiscountRule
{
    public decimal? RateFor(Client client) =>
        client.IsLoyal ? PricingRules.LoyalRate : null;
}