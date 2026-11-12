namespace RepairService.Core.Reporting;

/// <summary>
/// Виведення готового звіту в потік. Команда: нічого не повертає
/// й не знає, куди саме пише — у консоль, файл чи пам’ять.
/// </summary>
public sealed class DocumentReportPrinter
{
    private readonly TextWriter _output;

    /// <summary>
    /// Створює принтер, що пише в заданий потік.
    /// </summary>
    /// <param name="output">Потік виведення.</param>
    public DocumentReportPrinter(TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(output);
        _output = output;
    }

    /// <summary>
    /// Записує звіт у потік виведення.
    /// </summary>
    /// <param name="report">Текст звіту.</param>
    public void Print(string report)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(report);
        _output.Write(report);
    }
}
