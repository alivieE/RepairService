using RepairService.Core.Domain;

namespace RepairService.Core.Legacy;

/// <summary>
/// Успадкований розрахунок підсумку заявки на ремонт: перевірка входу,
/// знижки, зміна стану й друк в одному методі.
/// </summary>
public static class LegacyProcessor
{
    /// <summary>
    /// Перевіряє заявку, обчислює підсумок зі знижками, змінює стан
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
    /// <returns>Підсумок до сплати або -1, якщо вхідні дані некоректні.</returns>
    public static decimal Process(
        int docId,
        string clientName,
        string clientEmail,
        bool isRegular,
        List<RepairLine> lines,
        DateOnly createdAt,
        string couponCode,
        string status,
        bool printToConsole,
        decimal deliveryPrice,
        out string newStatus)
    {
        newStatus = status;
        decimal total = 0;
        if (docId <= 0)
        {
            Console.WriteLine("Помилка: номер заявки");
            return -1;
        }

        if (clientName == null || clientName == string.Empty)
        {
            Console.WriteLine("Помилка: немає клієнта");
            return -1;
        }

        if (clientEmail == null || !clientEmail.Contains('@'))
        {
            Console.WriteLine("Помилка: пошта клієнта");
            return -1;
        }

        if (lines == null || lines.Count == 0)
        {
            Console.WriteLine("Помилка: немає позицій");
            return -1;
        }

        if (status != "Accepted" && status != "InRepair"
            && status != "Ready" && status != "Cancelled")
        {
            Console.WriteLine("Помилка: невідомий стан");
            return -1;
        }

        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].Quantity <= 0)
            {
                Console.WriteLine("Помилка: кількість");
                return -1;
            }

            if (lines[i].UnitPrice < 0)
            {
                Console.WriteLine("Помилка: ціна");
                return -1;
            }

            decimal sum = lines[i].Quantity * lines[i].UnitPrice;
            if (lines[i].Quantity >= 10)
            {
                sum = sum * 0.95m;
            }

            total = total + sum;
        }

        if (isRegular)
        {
            total = total * 0.93m;
        }

        if (total > 5000)
        {
            total = total * 0.9m;
        }
        else if (total > 2000)
        {
            total = total * 0.95m;
        }

        if (couponCode == "SALE10")
        {
            total = total - (total * 0.1m);
        }
        else if (couponCode == "MINUS200" && total > 1000)
        {
            total = total - 200;
        }

        if (createdAt.DayOfWeek == DayOfWeek.Sunday)
        {
            total = total * 0.98m;
        }

        if (total < 1000)
        {
            total = total + deliveryPrice;
        }

        if (total < 0)
        {
            total = 0;
        }

        total = Math.Round(total, 2);
        if (status == "Accepted" && total > 0)
        {
            newStatus = "InRepair";
        }
        else if (status == "InRepair")
        {
            newStatus = "Ready";
        }
        else if (status == "Cancelled")
        {
            newStatus = "Cancelled";
            total = 0;
        }

        if (printToConsole)
        {
            Console.WriteLine("Заявка № " + docId);
            Console.WriteLine("Клієнт: " + clientName);
            Console.WriteLine("Пошта: " + clientEmail);
            for (int i = 0; i < lines.Count; i++)
            {
                Console.WriteLine(lines[i].Code + " x " + lines[i].Quantity);
            }

            Console.WriteLine("Разом: " + total);
            Console.WriteLine("Стан: " + newStatus);
        }

        return total;
    }
}
