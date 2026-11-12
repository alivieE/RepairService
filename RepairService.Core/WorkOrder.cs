using System;
using System.Collections.Generic;
using System.Linq;

namespace RepairService.Core;

/// <summary>
/// Наряд на ремонт: перелік робіт для клієнта сервісного центру
/// з розрахунком вартості та станом виконання.
/// </summary>
public class WorkOrder
{
    private readonly List<string[]> _lines = new List<string[]>();

    // public string prim;   // примітка, поки не треба

    /// <summary>
    /// Створює новий наряд на ремонт у стані «нова».
    /// </summary>
    /// <param name="id">Номер наряду.</param>
    /// <param name="customerName">Ім’я клієнта.</param>
    public WorkOrder(string id, string customerName)
    {
        Id = id;
        CustomerName = customerName;
        CreatedAt = DateTime.Now;
    }

    /// <summary>Номер наряду.</summary>
    public string Id { get; private set; }

    /// <summary>Ім’я клієнта, який здав пристрій у ремонт.</summary>
    public string CustomerName { get; private set; }

    /// <summary>
    /// Поточний стан наряду: 0 — нова, 1 — погоджена, 2 — виконана, 3 — скасована.
    /// </summary>
    public int Status { get; private set; }

    /// <summary>Дата й час створення наряду.</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Шукає наряд у списку за номером.
    /// </summary>
    /// <param name="workOrders">Список нарядів для пошуку.</param>
    /// <param name="workOrderId">Номер наряду, який треба знайти.</param>
    /// <returns>Знайдений наряд або null, якщо наряду з таким номером немає.</returns>
    public static WorkOrder? FindById(List<WorkOrder> workOrders, string workOrderId)
    {
        for (int i = 0; i < workOrders.Count; i++)
        {
            if (workOrders[i].Id == workOrderId)
            {
                return workOrders[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Додає до наряду рядок із роботою.
    /// </summary>
    /// <param name="workCode">Код роботи за прейскурантом.</param>
    /// <param name="quantity">Кількість одиниць роботи.</param>
    /// <param name="unitPrice">Ціна за одиницю, грн.</param>
    public void AddLine(string workCode, int quantity, decimal unitPrice)
    {
        string[] line = new string[3];
        line[0] = workCode;
        line[1] = quantity.ToString();
        line[2] = unitPrice.ToString();
        _lines.Add(line);
    }

    /// <summary>
    /// Обчислює вартість робіт за нарядом з урахуванням знижок
    /// і податку на додану вартість.
    /// </summary>
    /// <param name="isRegularCustomer">Ознака постійного клієнта.</param>
    /// <returns>Сума до сплати, заокруглена до двох знаків.</returns>
    public decimal CalculateTotal(bool isRegularCustomer)
    {
        decimal total = 0;
        int lineCount = 0;
        for (int i = 0; i < _lines.Count; i++)
        {
            int quantity = int.Parse(_lines[i][1]);
            decimal unitPrice = decimal.Parse(_lines[i][2]);
            total = total + (quantity * unitPrice);
            lineCount = lineCount + 1;
        }

        // if (sum1 > 500) { sum1 = sum1 - 50; } // стара знижка
        // знижки не сумуються: знижка постійному клієнту має пріоритет
        // над знижкою за велике замовлення, гуртова знижка діє після них
        if (isRegularCustomer == true && total > 1000)
        {
            total = total * 0.9m;
        }
        else if (total > 5000)
        {
            total = total * 0.85m;
        }
        else
        {
            total = total;
        }

        if (lineCount > 10)
        {
            total = total - 100;
        }

        if (total < 0)
        {
            total = 0;
        }

        // ПДВ нараховується на суму вже після всіх знижок
        total = total + (total * 0.2m);
        return Math.Round(total, 2);
    }

    /// <summary>
    /// Змінює стан наряду, якщо перехід дозволений правилами
    /// предметної області.
    /// </summary>
    /// <param name="newStatus">Цільовий стан наряду.</param>
    /// <returns>
    /// true, якщо перехід виконано; false, якщо він заборонений.
    /// </returns>
    public bool TryChangeStatus(int newStatus)
    {
        if (Status == 0 && newStatus == 1)
        {
            Status = 1;
            return true;
        }

        if (Status == 1 && newStatus == 2)
        {
            Status = 2;
            return true;
        }

        if (Status == 0 && newStatus == 3)
        {
            Status = 3;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Перевіряє, чи наряд заповнено коректно.
    /// </summary>
    /// <returns>true, якщо всі обов’язкові дані наряду коректні.</returns>
    public bool IsValid()
    {
        if (Id != null && Id != string.Empty
            && CustomerName != null
            && CustomerName.Length > 2
            && _lines.Count > 0
            && _lines.Count < 100
            && Status >= 0
            && Status <= 3)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Формує текстовий звіт про роботи наряду та підсумкову суму.
    /// </summary>
    /// <returns>Текст звіту, по одному рядку на роботу.</returns>
    public string BuildReport()
    {
        string report = string.Empty;
        for (int i = 0; i < _lines.Count; i++)
        {
            report = report + "Робота: " + _lines[i][0]
                + "; кількість: " + _lines[i][1]
                + "; ціна: " + _lines[i][2]
                + "; сума: " + (int.Parse(_lines[i][1]) * decimal.Parse(_lines[i][2]))
                + "\n";
        }

        report = report + "Разом: " + CalculateTotal(false) + "\n";
        return report;
    }
}
