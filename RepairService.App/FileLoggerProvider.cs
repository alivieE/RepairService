using Microsoft.Extensions.Logging;

namespace RepairService.App;

/// <summary>
/// Постачальник журналу у текстовий файл: один рядок на запис —
/// час, рівень, категорія і повідомлення зі сценарієм.
/// </summary>
public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly StreamWriter _writer;
    private readonly object _sync = new();

    /// <summary>Відкриває файл журналу на дописування.</summary>
    /// <param name="path">Шлях до файлу журналу.</param>
    public FileLoggerProvider(string path)
    {
        _writer = new StreamWriter(path, append: true) { AutoFlush = true };
    }

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName) => new FileLogger(this, categoryName);

    /// <inheritdoc/>
    public void Dispose() => _writer.Dispose();

    /// <summary>Дописує готовий рядок у файл; безпечно з кількох потоків.</summary>
    /// <param name="line">Відформатований рядок журналу.</param>
    internal void WriteLine(string line)
    {
        lock (_sync)
        {
            _writer.WriteLine(line);
        }
    }
}
