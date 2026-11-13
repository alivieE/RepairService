namespace RepairService.Core.Errors;

/// <summary>
/// Результат операції: або значення, або пояснення очікуваної помилки.
/// </summary>
/// <typeparam name="T">Тип значення.</typeparam>
public sealed class Result<T>
{
    /// <summary>Створює результат; викликається лише з фабрики Result.</summary>
    /// <param name="isSuccess">Ознака успіху.</param>
    /// <param name="value">Значення при успіху.</param>
    /// <param name="error">Пояснення помилки при невдачі.</param>
    internal Result(bool isSuccess, T? value, string error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    /// <summary>Чи вдалася операція.</summary>
    public bool IsSuccess { get; }

    /// <summary>Значення; читати лише після перевірки IsSuccess.</summary>
    public T? Value { get; }

    /// <summary>Пояснення помилки; порожнє при успіху.</summary>
    public string Error { get; }
}
