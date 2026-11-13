namespace RepairService.Core.Storage;

/// <summary>
/// Журнал аудиту у файлі: дописує рядки «час;операція;документ».
/// Володіє відкритим потоком, тому реалізує IDisposable.
/// </summary>
public sealed class FileAuditLog : IDisposable
{
    private readonly StreamWriter _writer;
    private bool _disposed;

    /// <summary>Відкриває файл журналу для дописування.</summary>
    /// <param name="path">Шлях до файлу журналу.</param>
    /// <exception cref="IOException">Файл зайнятий іншою програмою.</exception>
    public FileAuditLog(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _writer = new StreamWriter(path, append: true);
    }

    /// <summary>Дописує рядок аудиту.</summary>
    /// <param name="operation">Назва операції, наприклад ticket.paid.</param>
    /// <param name="documentId">Номер документа.</param>
    /// <exception cref="ObjectDisposedException">Журнал уже закрито.</exception>
    public void Write(string operation, int documentId)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);
        _writer.WriteLine($"{DateTimeOffset.Now:O};{operation};{documentId}");
    }

    /// <summary>Скидає буфер і закриває файл; повторний виклик нічого не робить.</summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _writer.Flush();
        _writer.Dispose();
        _disposed = true;
    }
}
