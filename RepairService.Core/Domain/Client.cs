namespace RepairService.Core.Domain;

/// <summary>Клієнт сервісного центру.</summary>
public sealed class Client
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Phone { get; init; } = string.Empty;

    public bool IsRegular { get; init; }
}
