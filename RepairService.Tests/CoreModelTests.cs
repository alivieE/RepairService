using RepairService.Core.Domain;
using RepairService.Core.Errors;
using RepairService.Core.Pricing;
using RepairService.Core.Reporting;
using RepairService.Core.Storage;

namespace RepairService.Tests;

public class CoreModelTests
{
    [Fact]
    public void RepairRequest_TwoLines_TotalSumsAmounts()
    {
        // Arrange
        RepairRequest request = new() { Id = 1, ClientId = 2, DeviceModel = "Lenovo" };
        request.AddLine(new RepairLine { Code = "WRK-1", Quantity = 2, UnitPrice = 250m });
        request.AddLine(new RepairLine { Code = "PRT-2", Quantity = 1, UnitPrice = 400m });

        // Act
        decimal total = request.Total();

        // Assert
        Assert.Equal(900m, total);
        Assert.Equal(RepairStatus.Accepted, request.Status);
    }

    [Fact]
    public void ClientAndServiceItem_InitProperties_KeepValues()
    {
        // Arrange
        Client client = new() { Id = 7, Name = "Іван", Phone = "+380501112233", IsRegular = true };

        // Act
        ServiceItem item = new() { Code = "PRT-2", Title = "Матриця", Price = 400m, IsPart = true };

        // Assert
        Assert.Equal("Іван", client.Name);
        Assert.True(client.IsRegular);
        Assert.Equal(400m, item.Price);
        Assert.True(item.IsPart);
    }

    [Fact]
    public void Customer_EmailWithoutAt_Throws()
    {
        // Act
        void Act() => _ = new Customer(1, "Іван Петренко", "ivan.mail.com");

        // Assert
        Assert.Throws<ArgumentException>(Act);
    }

    [Fact]
    public void Customer_ValidData_KeepsValues()
    {
        // Act
        Customer customer = new(1, "Іван Петренко", "ivan@mail.com") { IsRegular = true };

        // Assert
        Assert.Equal("ivan@mail.com", customer.Email);
        Assert.True(customer.IsRegular);
    }

    [Fact]
    public void Result_OkAndFail_CarryValueOrError()
    {
        // Act
        Result<int> ok = Result.Ok(5);
        Result<int> fail = Result.Fail<int>("помилка");

        // Assert
        Assert.True(ok.IsSuccess);
        Assert.Equal(5, ok.Value);
        Assert.False(fail.IsSuccess);
        Assert.Equal("помилка", fail.Error);
    }

    [Fact]
    public void Result_FailWithEmptyError_Throws()
    {
        // Act
        void Act() => Result.Fail<int>(" ");

        // Assert
        Assert.ThrowsAny<ArgumentException>(Act);
    }

    [Fact]
    public void DomainRuleException_WithInner_KeepsRuleAndInner()
    {
        // Arrange
        InvalidOperationException inner = new("стан");

        // Act
        DomainRuleException ex = new("ticket.cancellable", "Не можна.", inner);

        // Assert
        Assert.Equal("ticket.cancellable", ex.Rule);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void VipPricingPolicy_Line_TakesTenPercentOff()
    {
        // Arrange
        VipPricingPolicy policy = new();

        // Act
        decimal price = policy.PriceOf(new RepairWork("WRK-1", 2, 250m));

        // Assert
        Assert.Equal(450m, price);
    }

    [Fact]
    public void CsvReport_TwoTickets_WritesHeaderAndRows()
    {
        // Arrange
        RepairTicket first = new(1, 100, "Lenovo", new DateOnly(2026, 11, 2));
        RepairTicket second = new(2, 100, "Asus", new DateOnly(2026, 11, 3));

        // Act
        string csv = RepairTicketCsvReport.BuildCsv(new[] { first, second }, _ => 475m);

        // Assert
        Assert.Contains("id;total;status", csv);
        Assert.Contains("2;475.00;Received", csv);
    }

    [Fact]
    public void InMemoryRepository_DuplicateId_Throws()
    {
        // Arrange
        InMemoryRepairTicketRepository repository = new();
        repository.Add(new RepairTicket(1, 100, "Lenovo", new DateOnly(2026, 11, 2)));

        // Act
        void Act() => repository.Add(new RepairTicket(1, 100, "Asus", new DateOnly(2026, 11, 2)));

        // Assert
        Assert.Throws<InvalidOperationException>(Act);
        Assert.Single(repository.GetAll());
    }

    [Fact]
    public void FileAuditLog_DisposedTwice_WriteThrows()
    {
        // Arrange
        string path = Path.GetTempFileName();
        FileAuditLog audit = new(path);
        audit.Dispose();
        audit.Dispose();

        // Act
        void Act() => audit.Write("ticket.paid", 1);

        // Assert
        Assert.Throws<ObjectDisposedException>(Act);
        File.Delete(path);
    }
}
