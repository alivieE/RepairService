namespace RepairService.Core.Abstractions;

/// <summary>
/// Джерело поточної дати. Ядро не читає системний годинник напряму (D-21),
/// тому логіку з датами можна перевірити тестом на будь-який день.
/// </summary>
public interface IClock
{
    /// <summary>Поточна дата.</summary>
    DateOnly Today { get; }
}
