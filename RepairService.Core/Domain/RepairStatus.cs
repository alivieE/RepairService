namespace RepairService.Core.Domain;

/// <summary>Стан заявки на ремонт у її життєвому циклі.</summary>
public enum RepairStatus
{
    Accepted = 0,
    Diagnostics = 1,
    InRepair = 2,
    Ready = 3,
    Issued = 4,
    Cancelled = 5,
}
