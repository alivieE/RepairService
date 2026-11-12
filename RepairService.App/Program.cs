using System.Text;
using RepairService.Core;
using RepairService.Core.Abstractions;
using RepairService.Core.Documents;
using RepairService.Core.Domain;
using RepairService.Core.Pricing;
using RepairService.Core.Reporting;
using RepairService.Core.Services;
using RepairService.Core.Storage;

Console.OutputEncoding = Encoding.UTF8;

var workOrder = new WorkOrder("R-1001", "Іваненко");
workOrder.AddLine("WRK-1", 3, 250m);
workOrder.AddLine("WRK-2", 12, 90m);

Console.WriteLine(workOrder.CalculateTotal(false));
Console.WriteLine(workOrder.CalculateTotal(true));
Console.WriteLine(workOrder.IsValid());
Console.WriteLine(workOrder.TryChangeStatus(1));
Console.Write(workOrder.BuildReport());

TotalRequest request = BuildRequestFromInput();
TotalResult result = DocumentTotalCalculator.Calculate(request);
string report = DocumentReport.TextOf(request, result);
new DocumentReportPrinter(Console.Out).Print(report);

// композиційний корінь: єдине місце, де створюються конкретні реалізації
InMemoryRepairTicketRepository repository = new();
IPricingPolicy pricing = new DiscountPricingPolicy(0.05m);
RepairTicketService service = new(repository, pricing);

RepairTicket ticket = new(1, 100, "Lenovo IdeaPad 5", new DateOnly(2026, 11, 2));
ticket.AddLine(new RepairWork("WRK-1", 2, 250m));
service.Place(ticket);

Console.WriteLine($"Сума: {service.TotalOf(ticket):0.00}");

static TotalRequest BuildRequestFromInput()
{
    return new TotalRequest(
        DocumentId: 1,
        Client: new ClientInfo("Іваненко", "i@ex.com", true),
        Lines: new List<RepairLine>
        {
            new() { Code = "WRK-1", Quantity = 12, UnitPrice = 100m },
            new() { Code = "PRT-2", Quantity = 1, UnitPrice = 250m },
        },
        CreatedAt: new DateOnly(2026, 3, 10),
        CouponCode: null,
        Status: RepairStatus.Accepted,
        DeliveryPrice: 60m);
}
