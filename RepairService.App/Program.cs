using System.Text;
using Microsoft.Extensions.Logging;
using RepairService.App;
using RepairService.App.Menu;
using RepairService.Core;
using RepairService.Core.Abstractions;
using RepairService.Core.Services;
using RepairService.Core.Storage;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;
Console.WriteLine($"RepairService v{VersionInfo.Current()}");

// журнал пишеться у файл, щоб не змішуватися з меню в консолі
using var fileLogger = new FileLoggerProvider("repairservice.log");
using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddProvider(fileLogger);
    builder.SetMinimumLevel(LogLevel.Information);
});
ILogger log = loggerFactory.CreateLogger("RepairService.App");

// верхній обробник: єдине місце, де перехоплюються всі винятки;
// кожен клас помилок має свій рівень журналу й код завершення
try
{
    RunApplication(loggerFactory);
    return 0;
}
catch (IOException ex)
{
    log.LogError(ex, "Помилка середовища");
    Console.Error.WriteLine($"Помилка файлу або вводу: {ex.Message}");
    return 3;
}
catch (Exception ex)
{
    log.LogCritical(ex, "Непередбачений збій");
    Console.Error.WriteLine("Непередбачений збій, деталі в repairservice.log.");
    return 2;
}

static void RunApplication(ILoggerFactory loggerFactory)
{
    // композиційний корінь: єдине місце, де створюються конкретні реалізації
    Directory.CreateDirectory("data");
    IRepairTicketRepository repository =
        new JsonRepairTicketRepository(Path.Combine("data", "tickets.json"));
    IClock clock = new SystemClock();
    var services = new TicketServices(
        new TicketIntakeService(repository, clock),
        new TicketWorkflowService(repository, ServiceCatalog.Generate(ServiceCatalog.DefaultSize)),
        new TicketQueryService(repository),
        clock);

    var menu = new ConsoleMenu(
        services,
        Console.In,
        Console.Out,
        loggerFactory.CreateLogger<ConsoleMenu>());
    menu.Run();
}
