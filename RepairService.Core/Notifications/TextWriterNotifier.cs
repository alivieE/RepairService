using RepairService.Core.Abstractions;

namespace RepairService.Core.Notifications;

/// <summary>
/// Сповіщувач, що пише повідомлення в переданий потік: консоль, файл чи пам’ять.
/// </summary>
public sealed class TextWriterNotifier : INotifier
{
    private readonly TextWriter _output;

    /// <summary>
    /// Створює сповіщувач, що пише в заданий потік.
    /// </summary>
    /// <param name="output">Потік виведення.</param>
    public TextWriterNotifier(TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(output);
        _output = output;
    }

    /// <inheritdoc/>
    public void Notify(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        _output.WriteLine(message);
    }
}
