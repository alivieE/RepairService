using System.Globalization;

namespace RepairService.Core;

/// <summary>
/// Наряд на ремонт: перелік робіт для клієнта сервісного центру
/// з розрахунком вартості та станом виконання.
/// </summary>
public class WorkOrder
{
    /// <summary>Стан «нова»: наряд щойно створено.</summary>
    public const int StatusNew = 0;

    /// <summary>Стан «погоджена»: клієнт погодив вартість робіт.</summary>
    public const int StatusApproved = 1;

    /// <summary>Стан «виконана»: роботи завершено.</summary>
    public const int StatusCompleted = 2;

    /// <summary>Стан «скасована»: наряд скасовано.</summary>
    public const int StatusCancelled = 3;

    /// <summary>Ставка податку на додану вартість.</summary>
    private const decimal VatRate = 0.2m;

    /// <summary>Поріг суми для знижки постійному клієнту, грн.</summary>
    private const decimal RegularDiscountThreshold = 1000m;

    /// <summary>Частка знижки постійному клієнту.</summary>
    private const decimal RegularDiscountRate = 0.10m;

    /// <summary>Поріг суми для знижки на велике замовлення, грн.</summary>
    private const decimal LargeOrderThreshold = 5000m;

    /// <summary>Частка знижки на велике замовлення.</summary>
    private const decimal LargeOrderDiscountRate = 0.15m;

    /// <summary>Кількість рядків, після якої діє гуртова знижка.</summary>
    private const int BulkLineCount = 10;

    /// <summary>Сума гуртової знижки, грн.</summary>
    private const decimal BulkDiscountAmount = 100m;

    /// <summary>Найбільша допустима кількість рядків у наряді (не включно).</summary>
    private const int MaxLineCount = 100;

    /// <summary>Найкоротше допустиме ім’я клієнта, символів.</summary>
    private const int MinCustomerNameLength = 3;

    private readonly List<string[]> _lines = new List<string[]>();

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
    /// Поточний стан наряду: одна з констант StatusNew, StatusApproved,
    /// StatusCompleted, StatusCancelled.
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
        line[1] = quantity.ToString(CultureInfo.CurrentCulture);
        line[2] = unitPrice.ToString(CultureInfo.CurrentCulture);
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
            int quantity = int.Parse(_lines[i][1], CultureInfo.CurrentCulture);
            decimal unitPrice = decimal.Parse(_lines[i][2], CultureInfo.CurrentCulture);
            total += quantity * unitPrice;
            lineCount++;
        }

        // знижки не сумуються: знижка постійному клієнту має пріоритет
        // над знижкою за велике замовлення, гуртова знижка діє після них
        if (isRegularCustomer && total > RegularDiscountThreshold)
        {
            total *= 1m - RegularDiscountRate;
        }
        else if (total > LargeOrderThreshold)
        {
            total *= 1m - LargeOrderDiscountRate;
        }

        if (lineCount > BulkLineCount)
        {
            total -= BulkDiscountAmount;
        }

        if (total < 0)
        {
            total = 0;
        }

        // ПДВ нараховується на суму вже після всіх знижок
        total += total * VatRate;

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
        if (Status == StatusNew && newStatus == StatusApproved)
        {
            Status = StatusApproved;
            return true;
        }

        if (Status == StatusApproved && newStatus == StatusCompleted)
        {
            Status = StatusCompleted;
            return true;
        }

        if (Status == StatusNew && newStatus == StatusCancelled)
        {
            Status = StatusCancelled;
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
            && CustomerName.Length >= MinCustomerNameLength
            && _lines.Count > 0
            && _lines.Count < MaxLineCount
            && Status >= StatusNew
            && Status <= StatusCancelled)
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
            int quantity = int.Parse(_lines[i][1], CultureInfo.CurrentCulture);
            decimal unitPrice = decimal.Parse(_lines[i][2], CultureInfo.CurrentCulture);
            report = report + "Робота: " + _lines[i][0]
                + "; кількість: " + _lines[i][1]
                + "; ціна: " + _lines[i][2]
                + "; сума: " + (quantity * unitPrice)
                + "\n";
        }

        report = report + "Разом: " + CalculateTotal(false) + "\n";
        return report;
    }
}
