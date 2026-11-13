using System.Text;
using Microsoft.Extensions.Logging;
using RepairService.App.Input;
using RepairService.Core.Abstractions;
using RepairService.Core.Domain;
using RepairService.Core.Errors;
using RepairService.Core.Notifications;
using RepairService.Core.Pricing;
using RepairService.Core.Services;
using RepairService.Core.Storage;

Console.OutputEncoding = Encoding.UTF8;

using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddSimpleConsole(options =>
    {
        options.TimestampFormat = "HH:mm:ss ";
        options.IncludeScopes = true;
    });
    builder.SetMinimumLevel(LogLevel.Debug);
});
ILogger log = loggerFactory.CreateLogger("RepairService.App");

// верхній обробник: єдине місце, де перехоплюються всі винятки;
// кожен клас помилок має свій рівень журналу й код завершення
try
{
    RunApplication(loggerFactory);
    return 0;
}
catch (DomainRuleException ex)
{
    log.LogWarning("Правило {Rule}: {Message}", ex.Rule, ex.Message);
    return 1;
}
catch (IOException ex)
{
    log.LogError(ex, "Помилка середовища");
    return 3;
}
catch (Exception ex)
{
    log.LogCritical(ex, "Непередбачений збій");
    return 2;
}

static void RunApplication(ILoggerFactory loggerFactory)
{
    // композиційний корінь: єдине місце, де створюються конкретні реалізації
    Dictionary<string, IPricingPolicy> policies = new()
    {
        ["regular"] = new DiscountPricingPolicy(0.05m),
        ["vip"] = new VipPricingPolicy(),
        ["staff"] = new DiscountPricingPolicy(0.30m),
    };
    string customerType = "regular";
    IPricingPolicy pricing = policies.TryGetValue(customerType, out IPricingPolicy? found)
        ? found
        : new StandardPricingPolicy();

    InMemoryRepairTicketRepository repository = new();
    INotifier notifier = new TextWriterNotifier(Console.Out);
    RepairTicketService service = new(
        repository,
        pricing,
        notifier,
        loggerFactory.CreateLogger<RepairTicketService>());

    // журнал аудиту володіє файлом: using закриє його навіть після винятку
    using FileAuditLog audit = new("audit.log");

    RepairTicket ticket = new(1, 100, "Lenovo IdeaPad 5", new DateOnly(2026, 11, 2));
    service.Place(ticket);
    audit.Write("ticket.placed", ticket.Id);

    RepairWork line = ReadRepairWork();
    service.AddLine(ticket.Id, line);
    ticket.StartWork();
    ticket.Complete();

    decimal total = service.TotalOf(ticket);
    Console.WriteLine($"Сума: {total:0.00}");
    service.Pay(ticket.Id, 500m);
    audit.Write("ticket.paid", ticket.Id);
    Console.WriteLine($"Квитанція № {ticket.Id}: стан {ticket.Status}");

    Directory.CreateDirectory("reports");
    service.ExportReport(Path.Combine("reports", $"ticket-{ticket.Id}.txt"), ticket);

    // відмова після видачі заборонена: Warning і Error у журналі, код 1
    service.Cancel(ticket.Id, "клієнт передумав");
}

// цикл повторного запиту: поки ввід некоректний, пояснюємо помилку й питаємо знову
static RepairWork ReadRepairWork()
{
    while (true)
    {
        Result<RepairWork> parsed = RepairWorkParser.Parse(
            Ask("Код роботи: "),
            Ask("Кількість: "),
            Ask("Ціна за одиницю, грн: "));
        if (parsed.IsSuccess)
        {
            return parsed.Value!;
        }

        Console.WriteLine($"Помилка вводу: {parsed.Error}");
    }
}

static string Ask(string prompt)
{
    Console.Write(prompt);
    return Console.ReadLine() ?? throw new EndOfStreamException("Потік введення закрито.");
}
