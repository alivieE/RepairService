using RepairService.Core.Domain;

namespace RepairService.Core.Documents;

/// <summary>
/// Запит на розрахунок підсумку заявки на ремонт.
/// </summary>
/// <param name="DocumentId">Номер заявки.</param>
/// <param name="Client">Клієнт, який платить за ремонт.</param>
/// <param name="Lines">Рядки заявки: роботи й запчастини.</param>
/// <param name="CreatedAt">Дата створення заявки.</param>
/// <param name="CouponCode">Код купона або null, якщо купона немає.</param>
/// <param name="Status">Поточний стан заявки.</param>
/// <param name="DeliveryPrice">Вартість доставки пристрою, грн.</param>
public sealed record TotalRequest(
    int DocumentId,
    ClientInfo Client,
    IReadOnlyList<RepairLine> Lines,
    DateOnly CreatedAt,
    string? CouponCode,
    RepairStatus Status,
    decimal DeliveryPrice);
