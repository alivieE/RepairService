namespace RepairService.Core.Domain;

/// <summary>Клієнт сервісного центру.</summary>
public sealed class Client
{
    /// <summary>Ідентифікатор клієнта.</summary>
    public int Id { get; init; }

    /// <summary>Ім’я клієнта.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Контактний телефон.</summary>
    public string Phone { get; init; } = string.Empty;

    /// <summary>Ознака постійного клієнта.</summary>
    public bool IsRegular { get; init; }
}
