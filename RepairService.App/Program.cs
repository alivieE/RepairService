using System.Text;
using RepairService.Core;

Console.OutputEncoding = Encoding.UTF8;

var workOrder = new WorkOrder("R-1001", "Іваненко");
workOrder.AddLine("WRK-1", 3, 250m);
workOrder.AddLine("WRK-2", 12, 90m);

Console.WriteLine(workOrder.CalculateTotal(false));
Console.WriteLine(workOrder.CalculateTotal(true));
Console.WriteLine(workOrder.IsValid());
Console.WriteLine(workOrder.TryChangeStatus(1));
Console.Write(workOrder.BuildReport());
