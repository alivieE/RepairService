namespace RepairService.Core.Domain;

/// <summary>Стан квитанції на ремонт.</summary>
public enum TicketStatus
{
    /// <summary>Пристрій прийнято, квитанцію оформлено.</summary>
    Received,

    /// <summary>Клієнт погодив вартість, триває ремонт.</summary>
    InWork,

    /// <summary>Ремонт завершено, пристрій чекає на клієнта.</summary>
    Ready,

    /// <summary>Пристрій видано клієнту.</summary>
    Issued,

    /// <summary>У ремонті відмовлено або клієнт відмовився.</summary>
    Rejected,
}
