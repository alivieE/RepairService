namespace RepairService.Core.Domain;

/// <summary>Стан заявки на ремонт у її життєвому циклі.</summary>
public enum RepairStatus
{
    /// <summary>Пристрій прийнято, заявку зареєстровано.</summary>
    Accepted = 0,

    /// <summary>Майстер з’ясовує несправність.</summary>
    Diagnostics = 1,

    /// <summary>Клієнт погодив вартість, триває ремонт.</summary>
    InRepair = 2,

    /// <summary>Ремонт завершено, пристрій чекає на клієнта.</summary>
    Ready = 3,

    /// <summary>Пристрій видано, заявку закрито.</summary>
    Issued = 4,

    /// <summary>Заявку скасовано.</summary>
    Cancelled = 5,
}
