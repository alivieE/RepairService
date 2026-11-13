namespace RepairService.Core.Errors;

/// <summary>Фабрика результатів операцій, що можуть очікувано не вдатися.</summary>
public static class Result
{
    /// <summary>Створює успішний результат зі значенням.</summary>
    /// <typeparam name="T">Тип значення.</typeparam>
    /// <param name="value">Отримане значення.</param>
    /// <returns>Успішний результат.</returns>
    public static Result<T> Ok<T>(T value) => new(true, value, string.Empty);

    /// <summary>Створює невдалий результат з поясненням помилки.</summary>
    /// <typeparam name="T">Тип значення.</typeparam>
    /// <param name="error">Пояснення помилки для користувача.</param>
    /// <returns>Невдалий результат.</returns>
    public static Result<T> Fail<T>(string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);
        return new(false, default, error);
    }
}
