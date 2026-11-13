namespace RepairService.Core.Errors;

/// <summary>
/// Порушення правила предметної області: операцію неможливо виконати
/// в поточному стані квитанції або з такими даними.
/// </summary>
public class DomainRuleException : Exception
{
    /// <summary>Створює виняток з ідентифікатором правила й поясненням.</summary>
    /// <param name="rule">Ідентифікатор порушеного правила, наприклад ticket.amount.</param>
    /// <param name="message">Пояснення для користувача.</param>
    public DomainRuleException(string rule, string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rule);
        Rule = rule;
    }

    /// <summary>Створює виняток із внутрішнім винятком-причиною.</summary>
    /// <param name="rule">Ідентифікатор порушеного правила.</param>
    /// <param name="message">Пояснення для користувача.</param>
    /// <param name="innerException">Виняток, що спричинив порушення.</param>
    public DomainRuleException(string rule, string message, Exception innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rule);
        Rule = rule;
    }

    /// <summary>Ідентифікатор порушеного правила.</summary>
    public string Rule { get; }
}
