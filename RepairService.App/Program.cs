using System.Globalization;
using RepairService.Core.Domain;

RepairRequest request = new()
{
    Id = 1,
    ClientId = 10,
    DeviceModel = "Lenovo IdeaPad 5",
    CreatedAt = DateTimeOffset.Now,
};

request.AddLine(new RepairLine
{
    Code = "WRK-001",
    Quantity = 1,
    UnitPrice = 450.00m,
});
request.AddLine(new RepairLine
{
    Code = "PRT-014",
    Quantity = 2,
    UnitPrice = 320.50m,
});

string total = request.Total()
    .ToString("F2", CultureInfo.InvariantCulture);
Console.WriteLine($"Заявка #{request.Id}");
Console.WriteLine($"Пристрій: {request.DeviceModel}");
Console.WriteLine($"Стан: {request.Status}");
Console.WriteLine($"Позицій: {request.Lines.Count}");
Console.WriteLine($"Сума: {total}");
