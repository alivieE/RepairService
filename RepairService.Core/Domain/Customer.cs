namespace RepairService.Core.Domain;

/// <summary>
/// Клієнт сервісного центру. Незмінний тип: дані задаються один раз під час створення.
/// </summary>
public sealed record Customer
{
    /// <summary>
    /// Створює клієнта й перевіряє його інваріант.
    /// </summary>
    /// <param name="id">Ідентифікатор клієнта, більший за нуль.</param>
    /// <param name="fullName">Повне ім’я клієнта, не порожнє.</param>
    /// <param name="email">Пошта клієнта, що містить символ @.</param>
    /// <exception cref="ArgumentOutOfRangeException">Ідентифікатор не додатний.</exception>
    /// <exception cref="ArgumentException">Ім’я чи пошта порожні або пошта без @.</exception>
    public Customer(int id, string fullName, string email)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        if (!email.Contains('@'))
        {
            throw new ArgumentException("Пошта має містити символ @.", nameof(email));
        }

        Id = id;
        FullName = fullName;
        Email = email;
    }

    /// <summary>Ідентифікатор клієнта.</summary>
    public int Id { get; }

    /// <summary>Повне ім’я клієнта.</summary>
    public string FullName { get; }

    /// <summary>Пошта клієнта.</summary>
    public string Email { get; }

    /// <summary>Ознака постійного клієнта; задається лише в ініціалізаторі.</summary>
    public bool IsRegular { get; init; }
}
