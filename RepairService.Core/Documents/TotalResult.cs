using RepairService.Core.Domain;

namespace RepairService.Core.Documents;

/// <summary>
/// Результат розрахунку підсумку заявки.
/// </summary>
/// <param name="Total">Підсумок до сплати, грн.</param>
/// <param name="Status">Стан заявки після розрахунку.</param>
public sealed record TotalResult(
    decimal Total,
    RepairStatus Status);
