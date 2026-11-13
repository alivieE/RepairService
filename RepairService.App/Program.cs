using System.Text;
using RepairService.App.Input;
using RepairService.Core.Abstractions;
using RepairService.Core.Domain;
using RepairService.Core.Errors;
using RepairService.Core.Notifications;
using RepairService.Core.Pricing;
using RepairService.Core.Services;
using RepairService.Core.Storage;

Console.OutputEncoding = Encoding.UTF8;

// верхній обробник: єдине місце, де перехоплюються всі винятки;
// кожен клас помилок має свій код завершення
try
{
    RunApplication();
    return 0;
}
catch (DomainRuleException ex)
{
    Console.WriteLine($"Порушено правило {ex.Rule}: {ex.Message}");
    return 1;
}
catch (IOException ex)
{
    Console.WriteLine($"Помилка середовища: {ex.Message}");
    return 3;
}
catch (Exception ex)
{
    Console.WriteLine($"Непередбачений збій: {ex}");
    return 2;
}

static void RunApplication()
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
    RepairTicketService service = new(repository, pricing, notifier);

    RepairTicket ticket = new(1, 100, "Lenovo IdeaPad 5", new DateOnly(2026, 11, 2));
    service.Place(ticket);

    RepairWork line = ReadRepairWork();

    service.AddLine(ticket.Id, line);
    ticket.StartWork();
    ticket.Complete();

    decimal total = service.TotalOf(ticket);
    Console.WriteLine($"Сума: {total:0.00}");
    service.Pay(ticket.Id, total);
    Console.WriteLine($"Квитанція № {ticket.Id}: стан {ticket.Status}");
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
