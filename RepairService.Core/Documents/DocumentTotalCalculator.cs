using RepairService.Core.Domain;

namespace RepairService.Core.Documents;

/// <summary>
/// Підпрограми розрахунку підсумку заявки на ремонт. Кожна обчислює
/// рівно одну складову підсумку й нічого не змінює поза собою.
/// </summary>
public static class DocumentTotalCalculator
{
    /// <summary>
    /// Обчислює підсумок заявки й наступний стан за запитом.
    /// </summary>
    /// <param name="request">Запит на розрахунок.</param>
    /// <returns>Підсумок до сплати та стан заявки після розрахунку.</returns>
    public static TotalResult Calculate(TotalRequest request)
    {
        DocumentGuards.EnsureRequestValid(request);
        decimal total = SubtotalOf(request.Lines);
        total -= ClientDiscountOf(total, request.Client.IsRegular);
        total -= VolumeDiscountOf(total);
        total -= CouponDiscountOf(total, request.CouponCode);
        total -= WeekendDiscountOf(total, request.CreatedAt);
        total += DeliveryFeeOf(total, request.DeliveryPrice);
        total = request.Status == RepairStatus.Cancelled
            ? 0m
            : Math.Round(Math.Max(total, 0m), 2);
        return new TotalResult(total, NextStatusOf(request.Status, total));
    }

    /// <summary>
    /// Обчислює суму рядків заявки з урахуванням гуртової знижки.
    /// </summary>
    /// <param name="lines">Рядки заявки.</param>
    /// <returns>Сума рядків, грн.</returns>
    public static decimal SubtotalOf(IReadOnlyList<RepairLine> lines)
    {
        DocumentGuards.EnsureLinesValid(lines);
        decimal subtotal = 0m;
        foreach (RepairLine line in lines)
        {
            decimal sum = line.Quantity * line.UnitPrice;
            if (line.Quantity >= PricingRules.BulkQuantity)
            {
                sum -= sum * PricingRules.BulkRate;
            }

            subtotal += sum;
        }

        return subtotal;
    }

    /// <summary>
    /// Обчислює знижку постійному клієнту.
    /// </summary>
    /// <param name="amount">Сума, від якої рахується знижка.</param>
    /// <param name="isRegularClient">Ознака постійного клієнта.</param>
    /// <returns>Розмір знижки, грн.</returns>
    public static decimal ClientDiscountOf(decimal amount, bool isRegularClient)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        return isRegularClient
            ? amount * PricingRules.RegularClientRate
            : 0m;
    }

    /// <summary>
    /// Обчислює знижку за обсягом замовлення.
    /// </summary>
    /// <param name="amount">Сума, від якої рахується знижка.</param>
    /// <returns>Розмір знижки, грн.</returns>
    public static decimal VolumeDiscountOf(decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        if (amount > PricingRules.LargeOrderFrom)
        {
            return amount * PricingRules.LargeOrderRate;
        }

        if (amount > PricingRules.MediumOrderFrom)
        {
            return amount * PricingRules.MediumOrderRate;
        }

        return 0m;
    }

    /// <summary>
    /// Обчислює знижку за кодом купона.
    /// </summary>
    /// <param name="amount">Сума, від якої рахується знижка.</param>
    /// <param name="couponCode">Код купона або null, якщо купона немає.</param>
    /// <returns>Розмір знижки, грн.</returns>
    public static decimal CouponDiscountOf(decimal amount, string? couponCode)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        return couponCode switch
        {
            PricingRules.SaleCoupon
                => amount * PricingRules.CouponSaleRate,
            PricingRules.FixedCoupon when amount > PricingRules.CouponFixedFrom
                => PricingRules.CouponFixed,
            _ => 0m,
        };
    }

    /// <summary>
    /// Обчислює знижку за день створення заявки.
    /// </summary>
    /// <param name="amount">Сума, від якої рахується знижка.</param>
    /// <param name="createdAt">Дата створення заявки.</param>
    /// <returns>Розмір знижки, грн.</returns>
    public static decimal WeekendDiscountOf(decimal amount, DateOnly createdAt)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        return createdAt.DayOfWeek == DayOfWeek.Sunday
            ? amount * PricingRules.WeekendRate
            : 0m;
    }

    /// <summary>
    /// Обчислює плату за доставку пристрою.
    /// </summary>
    /// <param name="amount">Сума заявки після знижок.</param>
    /// <param name="deliveryPrice">Вартість доставки, грн.</param>
    /// <returns>Плата за доставку або 0, якщо доставка безкоштовна.</returns>
    public static decimal DeliveryFeeOf(decimal amount, decimal deliveryPrice)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(deliveryPrice);
        return amount < PricingRules.FreeDeliveryFrom
            ? deliveryPrice
            : 0m;
    }

    /// <summary>
    /// Визначає наступний стан заявки після розрахунку.
    /// </summary>
    /// <param name="current">Поточний стан заявки.</param>
    /// <param name="total">Підсумок заявки.</param>
    /// <returns>Наступний стан заявки.</returns>
    public static RepairStatus NextStatusOf(RepairStatus current, decimal total)
        => current switch
        {
            RepairStatus.Accepted when total > 0m => RepairStatus.InRepair,
            RepairStatus.InRepair => RepairStatus.Ready,
            _ => current,
        };
}
