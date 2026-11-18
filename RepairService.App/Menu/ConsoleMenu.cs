using System.Globalization;
using Microsoft.Extensions.Logging;
using RepairService.Core.Domain;
using RepairService.Core.Errors;

namespace RepairService.App.Menu;

/// <summary>
/// Консольне меню застосунку. Барикада: увесь ввід перевіряється тут один раз,
/// некоректний ввід — повідомлення й повторний запит, а не виняток.
/// </summary>
public sealed class ConsoleMenu
{
    private readonly TicketServices _services;
    private readonly TextReader _input;
    private readonly TextWriter _output;
    private readonly ILogger _logger;
    private readonly Dictionary<string, Action> _commands;

    /// <summary>Створює меню над прикладними сервісами.</summary>
    /// <param name="services">Прикладні сервіси.</param>
    /// <param name="input">Джерело вводу; у застосунку — консоль.</param>
    /// <param name="output">Приймач виводу; у застосунку — консоль.</param>
    /// <param name="logger">Журнал сценаріїв.</param>
    public ConsoleMenu(TicketServices services, TextReader input, TextWriter output, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(logger);
        _services = services;
        _input = input;
        _output = output;
        _logger = logger;
        _commands = new Dictionary<string, Action>
        {
            ["1"] = Accept,
            ["2"] = AddLine,
            ["3"] = Move,
            ["4"] = Pay,
            ["5"] = Find,
            ["6"] = Overdue,
        };
    }

    /// <summary>Показує меню, доки користувач не обере вихід або не закриє ввід.</summary>
    public void Run()
    {
        while (true)
        {
            _output.WriteLine();
            _output.WriteLine("1 — прийняти заявку   2 — додати позицію   3 — змінити стан");
            _output.WriteLine("4 — оплатити й видати 5 — пошук заявок     6 — прострочені   0 — вихід");
            string? choice = _input.ReadLine()?.Trim();
            if (choice is null or "0")
            {
                return;
            }

            Execute(choice);
        }
    }

    private static T? TryParseEnum<T>(string text)
        where T : struct, Enum
    {
        bool isName = !int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out _);
        return isName && Enum.TryParse(text.Trim(), ignoreCase: true, out T value) ? value : null;
    }

    private void Execute(string choice)
    {
        if (!_commands.TryGetValue(choice, out Action? command))
        {
            _output.WriteLine("Невідомий пункт меню.");
            return;
        }

        try
        {
            command();
        }
        catch (DomainRuleException ex)
        {
            _logger.LogWarning("Відхилено, правило {Rule}: {Message}", ex.Rule, ex.Message);
            _output.WriteLine($"Відмовлено: {ex.Message}");
        }
    }

    private void Accept()
    {
        _logger.LogInformation("UC-01: приймання заявки");
        int customerId = AskNumber("Номер клієнта: ");
        string model = AskText("Модель пристрою: ");
        TicketPriority priority = AskEnum<TicketPriority>("Пріоритет (Low, Normal, High, Urgent): ");
        RepairTicket ticket = _services.Intake.Accept(customerId, model, priority);
        _output.WriteLine($"Заявку № {ticket.Id} прийнято, дедлайн {ticket.Deadline:dd.MM.yyyy}.");
        _logger.LogInformation("UC-01: заявку {TicketId} прийнято", ticket.Id);
    }

    private void AddLine()
    {
        int ticketId = AskNumber("Номер заявки: ");
        string code = AskText("Код позиції (наприклад, WRK-002 або PRT-001): ");
        int quantity = AskNumber("Кількість: ");
        Result<RepairWork> result = _services.Workflow.AddLine(ticketId, code, quantity);
        if (!result.IsSuccess)
        {
            _output.WriteLine($"Не додано: {result.Error}");
            return;
        }

        _output.WriteLine($"Додано {result.Value!.Code} × {result.Value.Quantity}.");
        _logger.LogInformation("UC-03: до заявки {TicketId} додано {Code}", ticketId, result.Value.Code);
    }

    private void Move()
    {
        int ticketId = AskNumber("Номер заявки: ");
        TicketStatus target = AskEnum<TicketStatus>("Новий стан (InWork, Ready, Rejected): ");
        if (target is TicketStatus.Received or TicketStatus.Issued)
        {
            _output.WriteLine("Цей стан не встановлюють вручну; видача — пункт 4.");
            return;
        }

        RepairTicket ticket = _services.Workflow.MoveTo(ticketId, target);
        _output.WriteLine($"Заявка № {ticket.Id}: стан {ticket.Status}.");
        _logger.LogInformation("UC-02: заявка {TicketId} → {Status}", ticket.Id, ticket.Status);
    }

    private void Pay()
    {
        _logger.LogInformation("UC-06: оплата й видача");
        int ticketId = AskNumber("Номер заявки: ");
        _output.WriteLine($"До сплати: {_services.Workflow.TotalOf(ticketId):0.00} грн.");
        decimal amount = AskAmount("Сума оплати, грн: ");
        _services.Workflow.Pay(ticketId, amount);
        _output.WriteLine($"Заявку № {ticketId} оплачено, пристрій видано.");
        _logger.LogInformation("UC-06: заявку {TicketId} видано", ticketId);
    }

    private void Find()
    {
        TicketStatus? status = AskOptional<TicketStatus>("Стан (Enter — будь-який): ");
        TicketPriority? priority = AskOptional<TicketPriority>("Пріоритет (Enter — будь-який): ");
        Print(_services.Query.Find(status, priority));
    }

    private void Overdue()
    {
        DateOnly today = _services.Clock.Today;
        _output.WriteLine($"Прострочені заявки на {today:dd.MM.yyyy}:");
        Print(_services.Query.Overdue(today));
    }

    private void Print(IReadOnlyList<RepairTicket> tickets)
    {
        if (tickets.Count == 0)
        {
            _output.WriteLine("Заявок не знайдено.");
            return;
        }

        _output.WriteLine("   № | Пристрій             | Пріор.  | Стан      | Дедлайн");
        foreach (RepairTicket t in tickets)
        {
            _output.WriteLine(string.Create(
                CultureInfo.InvariantCulture,
                $"{t.Id,4} | {t.DeviceModel,-20} | {t.Priority,-7} | {t.Status,-9} | {t.Deadline:dd.MM.yyyy}"));
        }
    }

    private string AskText(string prompt)
    {
        while (true)
        {
            string text = Ask(prompt).Trim();
            if (text.Length > 0)
            {
                return text;
            }

            _output.WriteLine("Значення не може бути порожнім.");
        }
    }

    private int AskNumber(string prompt)
    {
        while (true)
        {
            string text = Ask(prompt);
            if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value)
                && value > 0)
            {
                return value;
            }

            _output.WriteLine("Введіть ціле число, більше за нуль.");
        }
    }

    private decimal AskAmount(string prompt)
    {
        while (true)
        {
            string text = Ask(prompt).Replace(',', '.');
            if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal value)
                && value >= 0m)
            {
                return value;
            }

            _output.WriteLine("Введіть суму, наприклад 450.50.");
        }
    }

    private T AskEnum<T>(string prompt)
        where T : struct, Enum
    {
        while (true)
        {
            T? value = TryParseEnum<T>(Ask(prompt));
            if (value is not null)
            {
                return value.Value;
            }

            _output.WriteLine($"Допустимі значення: {string.Join(", ", Enum.GetNames<T>())}.");
        }
    }

    private T? AskOptional<T>(string prompt)
        where T : struct, Enum
    {
        while (true)
        {
            string text = Ask(prompt);
            T? value = TryParseEnum<T>(text);
            if (string.IsNullOrWhiteSpace(text) || value is not null)
            {
                return value;
            }

            _output.WriteLine($"Допустимі значення: {string.Join(", ", Enum.GetNames<T>())}.");
        }
    }

    private string Ask(string prompt)
    {
        _output.Write(prompt);
        return _input.ReadLine() ?? throw new EndOfStreamException("Потік введення закрито.");
    }
}
