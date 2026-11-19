# Архітектура RepairService

## 1. Шари

Рішення має три проєкти; шари бібліотеки логіки оформлено теками й
просторами імен `RepairService.Core.*`.

| Шар | Де лежить | Відповідальність | Що заборонено |
|-----|-----------|------------------|---------------|
| Домен | `Core/Domain`, `Core/Errors` | заявка, позиції, пріоритети, стани, таблиця правил `PriorityRules`, виняток `DomainRuleException` | посилатися на сервіси, сховища, консоль, файли й системний час |
| Прикладний | `Core/Services`, `Core/Abstractions` | сценарії UC-01…UC-06, інтерфейси `IRepairTicketRepository`, `IClock` | створювати реалізації сховищ і читати файли напряму |
| Інфраструктура | `Core/Storage` | JSON-сховище, сховище в пам'яті, прейскурант | містити бізнес-правила; її знає лише композиційний корінь |
| Застосунок | `RepairService.App` | меню, ввід і вивід, журнал у файл, `SystemClock`, композиційний корінь | рахувати вартість чи дедлайн і змінювати стани в обхід сервісів |
| Тести | `RepairService.Tests` | модульні тести, тестові дублери `FakeRepairTicketRepository`, `FixedClock` | — |

## 2. Схема залежностей

```
RepairService.App ──► Services (прикладний) ──► Domain
        │                    ▲
        │                    │ реалізує IRepairTicketRepository
        └──────────────► Storage (інфраструктура) ──► Domain

RepairService.Tests ──► RepairService.Core
```

Рисунок 1 — Схема залежностей проєктів і шарів

- **App → Services.** Меню викликає лише прикладні сервіси й нічого не знає про
  формат файлу.
- **App → Storage.** Тільки композиційний корінь у `Program.cs` створює
  `JsonRepairTicketRepository`; меню отримує його вже як інтерфейс.
- **Services → Domain.** Сервіси керують заявкою через її методи-переходи і
  правило `PriorityRules`.
- **Storage → Services (Abstractions).** Сховище реалізує інтерфейс,
  оголошений у прикладному шарі, а не навпаки, — залежність спрямована
  всередину.
- **Domain → нікуди.** Домен не має жодного `using` на інші шари.

Правило закріплено тестом `ArchitectureTests.DomainTypes_DoNotReferenceOuterLayers`:
він рефлексією перевіряє поля, властивості й сигнатури всіх типів домену і
падає, якщо бодай один із них використовує тип зі `Services`, `Storage` або
`App`.

## 3. Композиційний корінь

Усі конкретні реалізації створюються в одному місці — функції
`RunApplication` у `RepairService.App/Program.cs`:

```csharp
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
    services, Console.In, Console.Out, loggerFactory.CreateLogger<ConsoleMenu>());
menu.Run();
```

Лістинг 1 — Композиційний корінь

Чому саме тут: замінити JSON на іншу реалізацію сховища, годинник на
фіксований чи консоль на рядок у тесті можна, змінивши один рядок у корені,
— жоден сервіс при цьому не змінюється. Сервіси й меню отримують залежності
через конструктор і не викликають `new` для інфраструктури.

## 4. Точки розширення

| Зміна | Механізм | Що змінюється |
|-------|----------|---------------|
| інше сховище (CSV, база даних) | новий клас, що реалізує `IRepairTicketRepository` | один рядок у композиційному корені |
| новий вид знижки для клієнта | нова реалізація `IPricingPolicy` (як `VipPricingPolicy`, `DiscountPricingPolicy`) | лише новий клас і рядок у корені |
| новий пункт меню | новий запис у таблиці команд `_commands` у `ConsoleMenu` | один метод і один рядок таблиці; наявні пункти не змінюються |

Обмеження: новий пріоритет (наприклад, `Critical`) вимагає зміни переліку
`TicketPriority` і таблиці `PriorityRules` — це свідомий компроміс
(див. docs/decisions.md, D-01): таблиця рішень лишається в одному місці.

## 5. Заміна зовнішніх залежностей у тестах

| Залежність | Інтерфейс або параметр | Дублер у тестах |
|------------|------------------------|-----------------|
| системний час | `IClock` | `FixedClock` |
| сховище заявок | `IRepairTicketRepository` | `FakeRepairTicketRepository` (з лічильником викликів `AddCalls`) |
| файл JSON | шлях у конструкторі `JsonRepairTicketRepository` | тимчасовий файл, що видаляється в `Dispose` |
| консоль | `TextReader` / `TextWriter` у конструкторі `ConsoleMenu` | `StringReader` / `StringWriter` |
| журнал | `ILogger` | `NullLogger` або власний провайдер |
