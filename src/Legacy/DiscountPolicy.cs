namespace RepairService.Legacy;

public sealed class DiscountPolicy
{
    public decimal For(decimal subtotal, Client client)
    {
        decimal rate = RateFor(client);
        return Math.Min(subtotal * rate, PricingRules.MaxDiscount);
    }

    private static decimal RateFor(Client client) => client.Kind switch
    {
        ClientKind.Vip => PricingRules.VipRate,
        ClientKind.Staff => PricingRules.StaffRate,
        _ => client.IsLoyal ? PricingRules.LoyalRate : 0m,
    };
}