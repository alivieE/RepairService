using RepairService.Core.Domain;
using RepairService.Core.Storage;

namespace RepairService.Tests;

public sealed class JsonRepairTicketRepositoryTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    private static DateOnly AcceptedOn => new(2026, 11, 2);

    public void Dispose()
    {
        File.Delete(_path);
    }

    [Fact]
    public void GetAll_FileMissing_ReturnsEmpty()
    {
        // Arrange
        var sut = new JsonRepairTicketRepository(_path);

        // Act + Assert
        Assert.Empty(sut.GetAll());
    }

    [Fact]
    public void Add_ThenNewInstance_RestoresTicketWithLinesAndPriority()
    {
        // Arrange
        var ticket = new RepairTicket(1, 100, "Lenovo", AcceptedOn, TicketPriority.Urgent);
        ticket.AddLine(new RepairWork("WRK-1", 2, 150.50m));
        ticket.AddLine(new RepairWork("PRT-7", 1, 400m, LineKind.Part));
        new JsonRepairTicketRepository(_path).Add(ticket);

        // Act
        RepairTicket? restored = new JsonRepairTicketRepository(_path).GetById(1);

        // Assert
        Assert.NotNull(restored);
        Assert.Equal(TicketPriority.Urgent, restored.Priority);
        Assert.Equal(ticket.Deadline, restored.Deadline);
        Assert.Equal(2, restored.Lines.Count);
        Assert.Equal(LineKind.Part, restored.Lines[1].Kind);
        Assert.Equal(150.50m, restored.Lines[0].UnitPrice);
    }

    [Fact]
    public void Add_ReadyTicket_RestoresStatus()
    {
        // Arrange
        var ticket = new RepairTicket(1, 100, "Lenovo", AcceptedOn);
        ticket.AddLine(new RepairWork("WRK-1", 1, 300m));
        ticket.StartWork();
        ticket.Complete();
        new JsonRepairTicketRepository(_path).Add(ticket);

        // Act
        RepairTicket? restored = new JsonRepairTicketRepository(_path).GetById(1);

        // Assert
        Assert.NotNull(restored);
        Assert.Equal(TicketStatus.Ready, restored.Status);
    }

    [Fact]
    public void Add_DuplicateId_Throws()
    {
        // Arrange
        var sut = new JsonRepairTicketRepository(_path);
        sut.Add(new RepairTicket(1, 100, "Lenovo", AcceptedOn));

        // Act + Assert
        Assert.Throws<InvalidOperationException>(
            () => sut.Add(new RepairTicket(1, 101, "Asus", AcceptedOn)));
    }
}
