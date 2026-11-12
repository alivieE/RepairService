using System.Globalization;
using System.Text;
using RepairService.Core.Documents;
using RepairService.Core.Domain;

namespace RepairService.Core.Reporting;

/// <summary>
/// Формування текстового звіту про розрахунок заявки на ремонт.
/// </summary>
public static class DocumentReport
{
    /// <summary>
    /// Формує текст звіту за запитом і результатом розрахунку. Чиста функція:
    /// результат залежить лише від аргументів, бо культура форматування задана явно.
    /// </summary>
    /// <param name="request">Запит на розрахунок.</param>
    /// <param name="result">Результат розрахунку.</param>
    /// <returns>Текст звіту, по одному рядку на позицію.</returns>
    public static string TextOf(TotalRequest request, TotalResult result)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(result);
        CultureInfo culture = CultureInfo.InvariantCulture;
        StringBuilder text = new();
        text.AppendLine(culture, $"Заявка № {request.DocumentId}");
        text.AppendLine(culture, $"Клієнт: {request.Client.Name}");
        foreach (RepairLine line in request.Lines)
        {
            text.AppendLine(culture, $"{line.Code} x {line.Quantity}");
        }

        text.AppendLine(culture, $"Разом: {result.Total:F2}");
        text.AppendLine(culture, $"Стан: {result.Status}");
        return text.ToString();
    }
}
