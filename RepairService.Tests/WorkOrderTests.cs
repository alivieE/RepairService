using RepairService.Core;

namespace RepairService.Tests;

public class WorkOrderTests
{
    [Fact]
    public void CalculateTotal_RegularCustomerOver1000_AppliesDiscountAndVat()
    {
        // Arrange
        WorkOrder order = TwoLineOrder();

        // Act
        decimal total = order.CalculateTotal(true);

        // Assert
        Assert.Equal(1976.40m, total);
    }

    [Fact]
    public void CalculateTotal_NewCustomer_AddsOnlyVat()
    {
        // Arrange
        WorkOrder order = TwoLineOrder();

        // Act
        decimal total = order.CalculateTotal(false);

        // Assert
        Assert.Equal(2196.00m, total);
    }

    [Fact]
    public void CalculateTotal_LargeOrder_AppliesLargeOrderDiscount()
    {
        // Arrange
        WorkOrder order = new("WO-2", "Петренко");
        order.AddLine("WRK-9", 1, 6000m);

        // Act
        decimal total = order.CalculateTotal(false);

        // Assert
        Assert.Equal(6120.00m, total);
    }

    [Fact]
    public void CalculateTotal_ElevenLines_AppliesBulkDiscount()
    {
        // Arrange
        WorkOrder order = OrderWithLines(11, 10m);

        // Act
        decimal total = order.CalculateTotal(false);

        // Assert
        Assert.Equal(12.00m, total);
    }

    [Fact]
    public void CalculateTotal_BulkDiscountAboveSum_ReturnsZero()
    {
        // Arrange
        WorkOrder order = OrderWithLines(11, 1m);

        // Act
        decimal total = order.CalculateTotal(false);

        // Assert
        Assert.Equal(0m, total);
    }

    [Theory]
    [InlineData(WorkOrder.StatusApproved, true)]
    [InlineData(WorkOrder.StatusCancelled, true)]
    [InlineData(WorkOrder.StatusCompleted, false)]
    public void TryChangeStatus_FromNew_FollowsRules(int newStatus, bool expected)
    {
        // Arrange
        WorkOrder order = TwoLineOrder();

        // Act
        bool changed = order.TryChangeStatus(newStatus);

        // Assert
        Assert.Equal(expected, changed);
    }

    [Fact]
    public void TryChangeStatus_ApprovedToCompleted_Succeeds()
    {
        // Arrange
        WorkOrder order = TwoLineOrder();
        order.TryChangeStatus(WorkOrder.StatusApproved);

        // Act
        bool changed = order.TryChangeStatus(WorkOrder.StatusCompleted);

        // Assert
        Assert.True(changed);
        Assert.Equal(WorkOrder.StatusCompleted, order.Status);
    }

    [Fact]
    public void IsValid_FilledOrder_ReturnsTrue()
    {
        // Arrange
        WorkOrder order = TwoLineOrder();

        // Act
        bool valid = order.IsValid();

        // Assert
        Assert.True(valid);
    }

    [Fact]
    public void IsValid_NoLines_ReturnsFalse()
    {
        // Arrange
        WorkOrder order = new("WO-3", "Іваненко");

        // Act
        bool valid = order.IsValid();

        // Assert
        Assert.False(valid);
    }

    [Fact]
    public void FindById_ExistingAndMissing_ReturnsOrderOrNull()
    {
        // Arrange
        WorkOrder order = TwoLineOrder();
        List<WorkOrder> orders = new() { order };

        // Act
        WorkOrder? found = WorkOrder.FindById(orders, "WO-1");
        WorkOrder? missing = WorkOrder.FindById(orders, "WO-404");

        // Assert
        Assert.Same(order, found);
        Assert.Null(missing);
    }

    [Fact]
    public void BuildReport_TwoLines_ListsWorksAndTotal()
    {
        // Arrange
        WorkOrder order = TwoLineOrder();

        // Act
        string report = order.BuildReport();

        // Assert
        Assert.Contains("Робота: WRK-1; кількість: 3", report);
        Assert.Contains("Разом: ", report);
    }

    private static WorkOrder TwoLineOrder()
    {
        WorkOrder order = new("WO-1", "Іваненко");
        order.AddLine("WRK-1", 3, 250m);
        order.AddLine("WRK-2", 12, 90m);
        return order;
    }

    private static WorkOrder OrderWithLines(int count, decimal unitPrice)
    {
        WorkOrder order = new("WO-4", "Коваленко");
        for (int i = 0; i < count; i++)
        {
            order.AddLine("WRK-" + i, 1, unitPrice);
        }

        return order;
    }
}
