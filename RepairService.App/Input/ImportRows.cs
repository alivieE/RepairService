using System.Globalization;

namespace RepairService.App.Input;

/// <summary>Розбір рядків імпорту позицій квитанції.</summary>
public static class ImportRows
{
    /// <summary>Рахує рядки з коректною кількістю (ціле число більше 0).</summary>
    /// <param name="rows">Рядки з кількістю.</param>
    /// <returns>Кількість коректних рядків.</returns>
    public static int CountValidQuantities(string[] rows)
    {
        var valid = 0;
        foreach (var row in rows)
        {
            if (int.TryParse(row, NumberStyles.Integer, CultureInfo.CurrentCulture, out int count)
                && count > 0)
            {
                valid++;
            }
        }

        return valid;
    }
}
