using RepairService.Core.Abstractions;
using RepairService.Core.Domain;

namespace RepairService.Tests;

/// <summary>Стаб політики ціни: жодного стану, лише наперед задана відповідь.</summary>
internal sealed class StubPricingPolicy : IPricingPolicy
{
    private readonly decimal _price;

    public StubPricingPolicy(decimal price)
    {
        _price = price;
    }

    public decimal PriceOf(RepairWork line) => _price;
}
