namespace RepairService.Core.Documents;

/// <summary>
/// Дані клієнта, який платить за ремонт.
/// </summary>
/// <param name="Name">Ім’я клієнта.</param>
/// <param name="Email">Пошта клієнта.</param>
/// <param name="IsRegular">Ознака постійного клієнта.</param>
public sealed record ClientInfo(
    string Name,
    string Email,
    bool IsRegular);
