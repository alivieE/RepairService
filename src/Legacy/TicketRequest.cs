namespace RepairService.Legacy;

public sealed record TicketRequest(
    int TicketId,
    Client Client,
    IReadOnlyList<WorkLine>? Lines,
    TicketState State,
    string Currency,
    bool SendMail);