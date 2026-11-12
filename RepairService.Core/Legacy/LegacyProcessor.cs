using RepairService.Core.Documents;
using RepairService.Core.Domain;
using static RepairService.Core.Documents.DocumentTotalCalculator;

namespace RepairService.Core.Legacy;

/// <summary>
/// Розрахунок підсумку заявки на ремонт: диспетчер, що викликає
/// підпрограми розрахунку в потрібному порядку.
/// </summary>
public static class LegacyProcessor
{
    /// <summary>
    /// Перевіряє заявку, обчислює підсумок зі знижками, визначає новий стан
    /// і за потреби друкує звіт.
    /// </summary>
    /// <param name="docId">Номер заявки.</param>
    /// <param name="clientName">Ім’я клієнта.</param>
    /// <param name="clientEmail">Пошта клієнта.</param>
    /// <param name="isRegular">Ознака постійного клієнта.</param>
    /// <param name="lines">Рядки заявки: роботи й запчастини.</param>
    /// <param name="createdAt">Дата створення заявки.</param>
    /// <param name="couponCode">Код купона на знижку.</param>
    /// <param name="status">Поточний стан заявки.</param>
    /// <param name="printToConsole">Чи друкувати звіт у консоль.</param>
    /// <param name="deliveryPrice">Вартість доставки пристрою кур’єром.</param>
    /// <param name="newStatus">Стан заявки після розрахунку.</param>
    /// <returns>Підсумок до сплати.</returns>
    public static decimal Process(
        int docId,
        string clientName,
        string clientEmail,
        bool isRegular,
        IReadOnlyList<RepairLine> lines,
        DateOnly createdAt,
        string? couponCode,
        RepairStatus status,
        bool printToConsole,
        decimal deliveryPrice,
        out RepairStatus newStatus)
    {
        DocumentGuards.EnsureHeaderValid(docId, clientName, clientEmail);
        decimal total = SubtotalOf(lines);
        total -= ClientDiscountOf(total, isRegular);
        total -= VolumeDiscountOf(total);
        total -= CouponDiscountOf(total, couponCode);
        total -= WeekendDiscountOf(total, createdAt);
        total += DeliveryFeeOf(total, deliveryPrice);
        total = status == RepairStatus.Cancelled
            ? 0m
            : Math.Round(Math.Max(total, 0m), 2);
        newStatus = NextStatusOf(status, total);

        if (printToConsole)
        {
            Console.WriteLine("Заявка № " + docId);
            Console.WriteLine("Клієнт: " + clientName);
            foreach (RepairLine line in lines)
            {
                Console.WriteLine(line.Code + " x " + line.Quantity);
            }

            Console.WriteLine("Разом: " + total);
            Console.WriteLine("Стан: " + newStatus);
        }

        return total;
    }
}
