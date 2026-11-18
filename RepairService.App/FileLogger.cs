using System.Globalization;
using Microsoft.Extensions.Logging;

namespace RepairService.App;

/// <summary>Журнал однієї категорії, що пише рядки через <see cref="FileLoggerProvider"/>.</summary>
internal sealed class FileLogger : ILogger
{
    private readonly FileLoggerProvider _owner;
    private readonly string _category;

    /// <summary>Створює журнал категорії.</summary>
    /// <param name="owner">Постачальник, що володіє файлом.</param>
    /// <param name="category">Категорія — зазвичай повне ім'я класу.</param>
    public FileLogger(FileLoggerProvider owner, string category)
    {
        _owner = owner;
        _category = category;
    }

    /// <inheritdoc/>
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => null;

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

    /// <inheritdoc/>
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        if (!IsEnabled(logLevel))
        {
            return;
        }

        string time = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        string error = exception is null ? string.Empty : $" | {exception.GetType().Name}: {exception.Message}";
        _owner.WriteLine($"{time} [{logLevel}] {_category}: {formatter(state, exception)}{error}");
    }
}
