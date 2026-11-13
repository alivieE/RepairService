using System.Globalization;
using RepairService.Core.Domain;
using RepairService.Core.Errors;

namespace RepairService.App.Input;

/// <summary>
/// Барикада: перетворює рядки, введені в консолі, на перевірену позицію
/// квитанції. Ядро отримує лише готовий RepairWork.
/// </summary>
public static class RepairWorkParser
{
    private static CultureInfo Ukrainian => CultureInfo.GetCultureInfo("uk-UA");

    /// <summary>Розбирає код роботи, кількість і ціну за одиницю.</summary>
    /// <param name="code">Код роботи.</param>
    /// <param name="quantity">Кількість рядком, ціле число.</param>
    /// <param name="unitPrice">Ціна рядком, дробова частина через кому.</param>
    /// <returns>Готова позиція або пояснення помилки вводу.</returns>
    public static Result<RepairWork> Parse(string? code, string? quantity, string? unitPrice)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result.Fail<RepairWork>("Код роботи не може бути порожнім.");
        }

        if (!int.TryParse(quantity, NumberStyles.Integer, Ukrainian, out int count)
            || count <= 0)
        {
            return Result.Fail<RepairWork>("Кількість — ціле число більше 0, наприклад 2.");
        }

        if (!decimal.TryParse(unitPrice, NumberStyles.Number, Ukrainian, out decimal price))
        {
            return Result.Fail<RepairWork>(
                "Ціна — число, дробова частина через кому, наприклад 250,50.");
        }

        if (price < 0m)
        {
            return Result.Fail<RepairWork>("Ціна не може бути від’ємною.");
        }

        return Result.Ok(new RepairWork(code.Trim().ToUpperInvariant(), count, price));
    }
}
