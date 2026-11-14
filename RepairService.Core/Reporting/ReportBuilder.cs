using System.Text;

namespace RepairService.Core.Reporting;

/// <summary>
/// Формує текст простого звіту з готових рядків.
/// </summary>
public sealed class ReportBuilder
{
    /// <summary>Заголовок звіту; у тексті виводиться великими літерами.</summary>
    public string Title { get; init; } = "Звіт";

    /// <summary>Складає звіт: заголовок і рядки, кожен з нового рядка.</summary>
    /// <param name="rows">Рядки звіту.</param>
    /// <returns>Текст звіту.</returns>
    /// <exception cref="ArgumentNullException">Рядки не передано.</exception>
    public string Build(IReadOnlyList<string> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        var text = new StringBuilder();
        text.AppendLine(Title.ToUpperInvariant());
        foreach (string row in rows)
        {
            text.AppendLine(row);
        }

        return text.ToString();
    }

    /// <summary>Описує кількість позицій у звіті.</summary>
    /// <param name="count">Кількість позицій.</param>
    /// <returns>Текст опису.</returns>
    public string Describe(int count)
    {
        string note;
        return "Позицій: " + count;
    }
}
