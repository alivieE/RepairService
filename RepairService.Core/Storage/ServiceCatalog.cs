using System.Globalization;
using RepairService.Core.Domain;

namespace RepairService.Core.Storage;

/// <summary>
/// Генератор умовного прейскуранта: роботи й запчастини з передбачуваними
/// кодами й цінами. Реальних даних у репозиторії немає.
/// </summary>
public static class ServiceCatalog
{
    /// <summary>Типовий обсяг прейскуранта для застосунку.</summary>
    public const int DefaultSize = 60;

    private const decimal WorkBasePrice = 200m;
    private const decimal WorkPriceStep = 50m;
    private const decimal PartBasePrice = 100m;
    private const decimal PartPriceStep = 30m;
    private const int PriceCycle = 10;
    private const string CodeFormat = "D3";

    /// <summary>
    /// Генерує прейскурант: позиції з парним номером — роботи, з непарним — запчастини.
    /// </summary>
    /// <param name="count">Кількість позицій, більша за нуль.</param>
    /// <returns>Позиції з унікальними кодами WRK-001, PRT-002 тощо.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Кількість не додатна.</exception>
    public static IReadOnlyList<ServiceItem> Generate(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

        var items = new List<ServiceItem>(count);
        for (int number = 1; number <= count; number++)
        {
            items.Add(ItemFor(number));
        }

        return items;
    }

    private static ServiceItem ItemFor(int number)
    {
        bool isPart = number % 2 == 1;
        int step = number % PriceCycle;
        string suffix = number.ToString(CodeFormat, CultureInfo.InvariantCulture);
        return new ServiceItem
        {
            Code = (isPart ? "PRT-" : "WRK-") + suffix,
            Title = (isPart ? "Запчастина № " : "Робота № ") + suffix,
            Price = isPart
                ? PartBasePrice + (PartPriceStep * step)
                : WorkBasePrice + (WorkPriceStep * step),
            IsPart = isPart,
        };
    }
}
