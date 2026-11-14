using System.Globalization;
using RepairService.Core.Domain;

namespace RepairService.Core.Input;

/// <summary>
/// Межа між «брудними» рядками імпорту та рядком заявки.
/// Формат рядка: КОД;КІЛЬКІСТЬ;ЦІНА, ціна з крапкою, наприклад «WKR-1;2;250.50».
/// </summary>
public static class RepairLineParser
{
    /// <summary>Найдовший допустимий код позиції, символів.</summary>
    public const int MaxCodeLength = 20;

    /// <summary>Найбільша допустима кількість одиниць в одному рядку.</summary>
    public const int MaxQuantity = 1000;

    /// <summary>Найбільша допустима ціна за одиницю, грн.</summary>
    public const decimal MaxUnitPrice = 100_000m;

    private const char FieldSeparator = ';';

    private const int FieldCount = 3;

    /// <summary>Перетворює рядок імпорту на рядок заявки.</summary>
    /// <param name="raw">Рядок у форматі КОД;КІЛЬКІСТЬ;ЦІНА.</param>
    /// <param name="line">Розібраний рядок заявки або null, якщо ввід некоректний.</param>
    /// <returns>true, якщо рядок коректний і всі значення в межах.</returns>
    public static bool TryParse(string? raw, out RepairLine? line)
    {
        line = null;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        string[] parts = raw.Split(FieldSeparator);
        if (parts.Length != FieldCount)
        {
            return false;
        }

        string code = parts[0].Trim();
        if (code.Length is 0 or > MaxCodeLength)
        {
            return false;
        }

        CultureInfo invariant = CultureInfo.InvariantCulture;
        if (!int.TryParse(parts[1], NumberStyles.Integer, invariant, out int quantity)
            || quantity is < 1 or > MaxQuantity)
        {
            return false;
        }

        if (!decimal.TryParse(parts[2], NumberStyles.Number, invariant, out decimal price)
            || price < 0m
            || price > MaxUnitPrice)
        {
            return false;
        }

        line = new RepairLine
        {
            Code = code.ToUpperInvariant(),
            Quantity = quantity,
            UnitPrice = price,
        };
        return true;
    }
}
