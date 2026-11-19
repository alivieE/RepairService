namespace RepairService.Core.Domain;

/// <summary>Вид позиції заявки: від нього залежить, чи діє надбавка за пріоритет.</summary>
public enum LineKind
{
    /// <summary>Робота майстра; дорожчає за високого й термінового пріоритету.</summary>
    Work,

    /// <summary>Запчастина; ціна не залежить від пріоритету.</summary>
    Part,
}
