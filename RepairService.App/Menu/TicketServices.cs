using RepairService.Core.Abstractions;
using RepairService.Core.Services;

namespace RepairService.App.Menu;

/// <summary>Набір прикладних сервісів, з якими працює консольне меню.</summary>
public sealed class TicketServices
{
    /// <summary>Збирає сервіси в один набір.</summary>
    /// <param name="intake">Приймання заявок (UC-01).</param>
    /// <param name="workflow">Позиції, переходи станів і оплата (UC-02, UC-03, UC-06).</param>
    /// <param name="query">Пошук і звіт прострочених заявок (UC-04, UC-05).</param>
    /// <param name="clock">Джерело поточної дати для звіту.</param>
    public TicketServices(
        TicketIntakeService intake,
        TicketWorkflowService workflow,
        TicketQueryService query,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(intake);
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(clock);
        Intake = intake;
        Workflow = workflow;
        Query = query;
        Clock = clock;
    }

    /// <summary>Приймання заявок (UC-01).</summary>
    public TicketIntakeService Intake { get; }

    /// <summary>Позиції, переходи станів і оплата (UC-02, UC-03, UC-06).</summary>
    public TicketWorkflowService Workflow { get; }

    /// <summary>Пошук і звіт прострочених заявок (UC-04, UC-05).</summary>
    public TicketQueryService Query { get; }

    /// <summary>Джерело поточної дати для звіту прострочених заявок.</summary>
    public IClock Clock { get; }
}
