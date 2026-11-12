using RepairService.Core.Domain;

namespace RepairService.Core.Documents;

/// <summary>
/// Перевірки передумов для заявки на ремонт: кидають виняток,
/// якщо вхідні дані порушують контракт розрахунку.
/// </summary>
public static class DocumentGuards
{
    /// <summary>
    /// Перевіряє заголовок заявки: номер, ім’я та пошту клієнта.
    /// </summary>
    /// <param name="documentId">Номер заявки, більший за нуль.</param>
    /// <param name="clientName">Ім’я клієнта, не порожнє.</param>
    /// <param name="clientEmail">Пошта клієнта, що містить символ @.</param>
    /// <exception cref="ArgumentOutOfRangeException">Номер заявки не додатний.</exception>
    /// <exception cref="ArgumentException">Ім’я чи пошта порожні або пошта без @.</exception>
    public static void EnsureHeaderValid(
        int documentId,
        string clientName,
        string clientEmail)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(documentId);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientName);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientEmail);
        if (!clientEmail.Contains('@'))
        {
            throw new ArgumentException(
                "Пошта має містити символ @.",
                nameof(clientEmail));
        }
    }

    /// <summary>
    /// Перевіряє один рядок заявки: код позиції, кількість і ціну.
    /// </summary>
    /// <param name="line">Рядок заявки.</param>
    /// <exception cref="ArgumentNullException">Рядок відсутній.</exception>
    /// <exception cref="ArgumentException">Код позиції порожній.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Кількість не додатна або ціна від’ємна.
    /// </exception>
    public static void EnsureLineValid(RepairLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        ArgumentException.ThrowIfNullOrWhiteSpace(line.Code);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
            line.Quantity,
            nameof(line.Quantity));
        ArgumentOutOfRangeException.ThrowIfNegative(
            line.UnitPrice,
            nameof(line.UnitPrice));
    }

    /// <summary>
    /// Перевіряє колекцію рядків заявки: вона існує, не порожня,
    /// а кожен рядок коректний.
    /// </summary>
    /// <param name="lines">Рядки заявки.</param>
    /// <exception cref="ArgumentNullException">Колекція відсутня.</exception>
    /// <exception cref="ArgumentException">Заявка не містить жодного рядка.</exception>
    public static void EnsureLinesValid(IReadOnlyList<RepairLine> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        if (lines.Count == 0)
        {
            throw new ArgumentException(
                "Заявка не містить жодної позиції.",
                nameof(lines));
        }

        foreach (RepairLine line in lines)
        {
            EnsureLineValid(line);
        }
    }

    /// <summary>
    /// Перевіряє весь запит на розрахунок: заголовок, клієнта, рядки
    /// й вартість доставки.
    /// </summary>
    /// <param name="request">Запит на розрахунок.</param>
    /// <exception cref="ArgumentNullException">Запит або клієнт відсутні.</exception>
    /// <exception cref="ArgumentException">Дані заявки порушують контракт.</exception>
    public static void EnsureRequestValid(TotalRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Client);
        EnsureHeaderValid(
            request.DocumentId,
            request.Client.Name,
            request.Client.Email);
        EnsureLinesValid(request.Lines);
        ArgumentOutOfRangeException.ThrowIfNegative(request.DeliveryPrice);
    }
}
