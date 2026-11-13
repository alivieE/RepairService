namespace RepairService.Legacy;

public sealed class DiscountPolicy
{
    // порядок важливий: перше правило, що підходить, визначає ставку
    private readonly IReadOnlyList<IDiscountRule> _rules = new IDiscountRule[]
    {
        new VipDiscountRule(),
        new StaffDiscountRule(),
        new LoyalDiscountRule(),
    };

    public decimal For(decimal subtotal, Client client)
    {
        decimal rate = RateFor(client);
        return Math.Min(subtotal * rate, PricingRules.MaxDiscount);
    }

    private decimal RateFor(Client client)
    {
        foreach (IDiscountRule rule in _rules)
        {
            decimal? rate = rule.RateFor(client);
            if (rate.HasValue)
            {
                return rate.Value;
            }
        }
        return 0m;
    }
}