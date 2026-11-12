using System.Text;
using RepairService.Core;

Console.OutputEncoding = Encoding.UTF8;

var z = new zayavka("R-1001", "Іваненко");
z.Add("WRK-1", 3, 250m);
z.Add("WRK-2", 12, 90m);

Console.WriteLine(z.ProcessData(false));
Console.WriteLine(z.ProcessData(true));
Console.WriteLine(z.Ck());
Console.WriteLine(z.Chg(1));
Console.Write(z.Rep());
