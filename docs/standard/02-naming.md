# 2. Іменування ідентифікаторів

Ідентифікатори пишуть англійською, без транслітерації. Мова коментарів і
документації — українська (C-10). Кожне поняття предметної області має одне
ім’я з глосарію (1.5).

## 2.1. Правила

**N-01. Простір імен повторює шлях до теки від кореня проєкту.**
- **Рівень:** ОБОВ’ЯЗКОВО.
- **Обґрунтування:** за іменем простору одразу видно, де лежить файл; корінь
  дорівнює імені проєкту, глибина — не більше двох рівнів під коренем
  (`RepairService.Core.Domain`), бо глибші теки в рішенні такого розміру
  лише розпорошують типи.
- **Порушення:** `namespace Core.domain;` у файлі `RepairService.Core/Domain/RepairTicket.cs`
- **Дотримання:** `namespace RepairService.Core.Domain;`
- **Перевірка:** аналізатор IDE0130, рівень error.

**N-02. Клас, структура й запис — іменник або іменникова фраза в PascalCase, в однині.**
- **Рівень:** ОБОВ’ЯЗКОВО.
- **Обґрунтування:** тип описує одну сутність; суфікси `Record`, `Dto`, `Data`
  нічого не додають до змісту, а множина в назві типу плутає з колекцією.
- **Порушення:** `public class zayavka`, `public record RepairWorkRecord`
- **Дотримання:** `public sealed class RepairTicket`, `public sealed record RepairWork`
- **Перевірка:** IDE1006 (регістр), пункт R-01 чеклісту (зміст).

**N-03. Інтерфейс має префікс `I` і назву можливості або ролі.**
- **Рівень:** ОБОВ’ЯЗКОВО.
- **Обґрунтування:** префікс відрізняє контракт від реалізації в місці
  використання: `IRepairTicketRepository` у конструкторі одразу каже, що
  підставити можна будь-яке сховище.
- **Порушення:** `public interface RepairTicketRepository`
- **Дотримання:** `public interface IRepairTicketRepository`
- **Перевірка:** CA1715, error.

**N-04. Перелічення — іменник в однині; члени в PascalCase без префікса типу.**
- **Рівень:** ОБОВ’ЯЗКОВО.
- **Обґрунтування:** змінна типу перелічення містить одне значення, тому
  `TicketStatus status` читається природно; множина допустима лише для
  `[Flags]`. Префікс у члені дублює ім’я типу: `TicketStatus.StatusReceived`.
- **Порушення:** `enum RepairStatuses { StatusNew, StatusDone }`
- **Дотримання:** `enum TicketStatus { Received, InWork, Ready, Issued, Rejected }`
- **Перевірка:** CA1717, IDE1006.

**N-05. Параметр узагальненого типу — `T` або `T` + роль.**
- **Рівень:** ОБОВ’ЯЗКОВО.
- **Обґрунтування:** одна літера `T` зрозуміла, коли параметр один
  (`Result<T>`); за кількох параметрів роль має бути в імені (`TKey`, `TValue`).
- **Порушення:** `public sealed class Result<Data>`
- **Дотримання:** `public sealed class Result<T>`
- **Перевірка:** CA1715, error.

**N-06. Метод-команда — дієслово; метод-запит — іменник із прийменником `Of`/`For` або `Is`/`Can`.**
- **Рівень:** ОБОВ’ЯЗКОВО.
- **Обґрунтування:** команда змінює стан (`Pay`, `StartWork`), запит лише
  обчислює (`TotalOf`, `DaysFor`, `IsOverdueOn`). Читач із виклику бачить,
  чи має він побічний ефект, — це наш спосіб записати принцип CQS.
- **Порушення:** `public decimal ProcessData(bool f)`, `public bool Chg(int n)`
- **Дотримання:** `public decimal CalculateTotal(bool isRegularCustomer)`, `public bool TryChangeStatus(int newStatus)`
- **Перевірка:** IDE1006 (регістр), R-01 (дієслово й зміст).

**N-07. Асинхронний метод має суфікс `Async`.**
- **Рівень:** ОБОВ’ЯЗКОВО.
- **Обґрунтування:** виклик без `await` повертає `Task` і мовчки не чекає
  результату; суфікс нагадує про `await` у місці виклику. Правило знадобиться,
  коли в СР3 з’явиться файлове сховище.
- **Порушення:** `public async Task<RepairTicket> Load(int id)`
- **Дотримання:** `public async Task<RepairTicket> LoadAsync(int id)`
- **Перевірка:** IDE1006 (правило `async_suffix`), error.

**N-08. Властивість — іменник у PascalCase без префікса `Get`.**
- **Рівень:** ОБОВ’ЯЗКОВО.
- **Обґрунтування:** властивість — це дані, а не дія; `GetUnitPrice` як
  властивість читається як виклик методу.
- **Порушення:** `public decimal GetUnitPrice { get; }`
- **Дотримання:** `public decimal UnitPrice { get; }`
- **Перевірка:** IDE1006, R-01.

**N-09. Приватне поле, зокрема статичне, — `_camelCase`.**
- **Рівень:** ОБОВ’ЯЗКОВО.
- **Обґрунтування:** підкреслення миттєво відрізняє поле від локальної
  змінної й параметра, тому зайвим стає префікс `this.`; у методі
  `RepairTicketService.Pay` на 30 рядків це економить повернення до початку
  класу. Окремого префікса `s_` для статичних полів не вводимо (див. 7.2).
- **Порушення:** `private readonly List<RepairWork> lines;`
- **Дотримання:** `private readonly List<RepairWork> _lines = new();`
- **Перевірка:** IDE1006 (правило `priv_underscore`), error.

**N-10. Префікс `this.` перед членами не пишуть.**
- **Рівень:** ОБОВ’ЯЗКОВО.
- **Обґрунтування:** з N-09 поле й так відрізняється від параметра, тому
  `this.` лише подовжує рядок.
- **Порушення:** `this._lines.Add(line);`
- **Дотримання:** `_lines.Add(line);`
- **Перевірка:** IDE0003, error.

**N-11. Константа — PascalCase; ім’я називає зміст і одиницю.**
- **Рівень:** ОБОВ’ЯЗКОВО.
- **Обґрунтування:** `UrgentDays` і `MaxUrgentSurcharge` читаються в тексті
  формули; `MAX_DAYS` — стиль C, а `Value1` не каже нічого.
- **Порушення:** `private const int DAYS = 1;`
- **Дотримання:** `private const int UrgentDays = 1;`
- **Перевірка:** IDE1006 (правило `const_pascal`), error.

**N-12. Локальна змінна й параметр — camelCase, повними словами.**
- **Рівень:** ОБОВ’ЯЗКОВО.
- **Обґрунтування:** ім’я `kolvo` чи `sum1` неможливо знайти пошуком і
  зрозуміти без читання всього методу.
- **Порушення:** `decimal sum1 = 0; int kolvo = 0;`
- **Дотримання:** `decimal total = 0m; int lineCount = 0;`
- **Перевірка:** IDE1006 (регістр), R-02 (повні слова).

**N-13. Булеве ім’я починається з `Is`, `Has` або `Can` і має ствердну форму.**
- **Рівень:** РЕКОМЕНДОВАНО.
- **Обґрунтування:** `if (line.IsPart)` читається як речення; заперечення в
  імені (`IsNotPart`) дає подвійне заперечення в `!line.IsNotPart`.
- **Порушення:** `public bool Part`, параметр `bool f`
- **Дотримання:** `public bool IsPart`, параметр `bool isRegularCustomer`
- **Перевірка:** R-02.

**N-14. Колекція — іменник у множині; словник — `ЗначенняByКлюч`.**
- **Рівень:** РЕКОМЕНДОВАНО.
- **Обґрунтування:** `Lines` — набір позицій, `PricesByCode` одразу каже, що
  ключ — код позиції.
- **Порушення:** `List<RepairTicket> L`, `Dictionary<string, decimal> dict`
- **Дотримання:** `List<RepairTicket> tickets`, `Dictionary<string, decimal> pricesByCode`
- **Перевірка:** R-02.

**N-15. Подія — дієприкметник (`TicketIssued`), обробник — `On` + подія, делегат — суфікс `Handler`.**
- **Рівень:** РЕКОМЕНДОВАНО.
- **Обґрунтування:** подія повідомляє про те, що вже сталося; однакова схема
  іменування дає змогу знайти всіх підписників пошуком `On`.
- **Порушення:** `public event Action? Issue;`
- **Дотримання:** `public event EventHandler<TicketIssuedEventArgs>? TicketIssued;`
- **Перевірка:** R-02.

**N-16. Тестовий клас — `<Клас>Tests`, тестовий метод — `Метод_Сценарій_Результат`.**
- **Рівень:** РЕКОМЕНДОВАНО.
- **Обґрунтування:** у звіті про невдалий тест ім’я одразу каже, що
  перевіряли й що мало статися, без відкриття файлу. Підкреслення в іменах
  тестів дозволено (CA1707 придушено в тестових проєктах).
- **Порушення:** `public void Test1()`
- **Дотримання:** `public void Pay_LessThanTotal_ThrowsWithAmountRule()`
- **Перевірка:** R-14.

**N-17. Поняття предметної області називають лише іменем із глосарію.**
- **Рівень:** ОБОВ’ЯЗКОВО.
- **Обґрунтування:** коли та сама заявка має імена `RepairTicket`,
  `RepairRequest` і `WorkOrder`, пошук по коду знаходить лише третину
  випадків, а правило змінюють не всюди (ADR-001).
- **Порушення:** новий метод `ArchiveRequest(RepairRequest request)`
- **Дотримання:** `Archive(RepairTicket ticket)`
- **Перевірка:** R-03.

**N-18. Ім’я файлу дорівнює імені єдиного типу в ньому.**
- **Рівень:** ОБОВ’ЯЗКОВО.
- **Обґрунтування:** пошук файлу за іменем типу працює без IDE; узагальнений
  тип пишуть у фігурних дужках: `Result{T}.cs`.
- **Порушення:** файл `Models.cs` із типами `RepairWork` і `Customer`
- **Дотримання:** `RepairWork.cs`, `Customer.cs`
- **Перевірка:** SA1649 і SA1402, error.

**N-19. Модифікатор доступу вказують явно.**
- **Рівень:** ОБОВ’ЯЗКОВО.
- **Обґрунтування:** типовий доступ різний для класу (`internal`) і члена
  (`private`), тому без модифікатора читач мусить це пам’ятати.
- **Порушення:** `class InMemoryRepairTicketRepository`, `decimal Subtotal()`
- **Дотримання:** `public sealed class InMemoryRepairTicketRepository`, `private decimal Subtotal()`
- **Перевірка:** IDE0040, error.

**N-20. Абревіатура з двох літер — великими (`IO`), довша — як слово (`Csv`, `Json`, `Id`).**
- **Рівень:** РЕКОМЕНДОВАНО.
- **Обґрунтування:** так роблять у бібліотеці .NET (`System.IO`, `JsonSerializer`),
  а `RepairTicketCSVReport` ламає межі слів.
- **Порушення:** `RepairTicketCSVReport`, `TicketID`
- **Дотримання:** `RepairTicketCsvReport`, `TicketId`
- **Перевірка:** R-02.

**N-21. Параметр короткої лямбди може мати однолітерне ім’я.**
- **Рівень:** ДОПУСКАЄТЬСЯ.
- **Обґрунтування:** у виразі на один рядок тип і зміст видно з контексту:
  `tickets.Where(t => t.Status == TicketStatus.InWork)`.
- **Порушення:** —
- **Дотримання:** `t => t.Deadline` або `ticket => ticket.Deadline`
- **Перевірка:** не перевіряють.

**N-22. Імена-антипатерни з таблиці 2.3 заборонено.**
- **Рівень:** ЗАБОРОНЕНО.
- **Обґрунтування:** кожен антипатерн або ховає зміст, або бреше про нього.
- **Порушення / дотримання:** таблиця 2.3.
- **Перевірка:** R-01.

## 2.2. Пари прикладів

Приклади «погано» взято з успадкованого коду ЛР2 (`Code1.cs`, клас
`zayavka`), приклади «добре» — з його рефакторингу та з ядра RepairService.

**Пара 1. Ім’я типу (N-02).**

```csharp
// ПОГАНО
public class zayavka
```

```csharp
// ДОБРЕ
public sealed class RepairTicket
```

Транслітерацію не знайде англомовний колега, а мала літера на початку робить
тип схожим на змінну. `sealed` також говорить, що спадкування не задумане.

**Пара 2. Поля замість властивостей і число замість стану (N-04, N-08).**

```csharp
// ПОГАНО
public string cl;
public int st = 0; // 0-нова,1-погодж,2-викон,3-скасов
```

```csharp
// ДОБРЕ
public string CustomerName { get; }
public TicketStatus Status { get; private set; }
```

`cl` і `st` треба розшифровувати коментарем, а коментар уже розійшовся з
кодом: станів у моделі п’ять, а не чотири. Перелічення робить неможливим
значення 17, а `private set` забороняє змінювати стан повз методи переходу.

**Пара 3. Методи з беззмістовними іменами (N-06).**

```csharp
// ПОГАНО
public bool Chg(int n)
public bool Ck()
public string Rep()
```

```csharp
// ДОБРЕ
public bool TryChangeStatus(int newStatus)
public bool IsValid()
public string BuildReport()
```

Скорочення `Chg`, `Ck`, `Rep` змушують читати тіло методу. Префікс `Try`
повідомляє, що метод повертає ознаку успіху замість винятку.

**Пара 4. Метод-затичка з параметром-прапорцем (N-06, N-13).**

```csharp
// ПОГАНО
public decimal ProcessData(bool f)
```

```csharp
// ДОБРЕ
public decimal CalculateTotal(bool isRegularCustomer)
```

`ProcessData` підходить до будь-якого методу програми, а `f` у місці виклику
виглядає як `ProcessData(true)` — що саме «true», не видно.

**Пара 5. Метод пошуку (N-06, N-12, N-14).**

```csharp
// ПОГАНО
public static zayavka F(List<zayavka> L, string x)
```

```csharp
// ДОБРЕ
public static WorkOrder? FindById(List<WorkOrder> workOrders, string workOrderId)
```

Однолітерні імена не знаходяться пошуком, а `L` з великої літери ще й
порушує N-12. Тип результату з `?` попереджає, що наряду може не бути.

**Пара 6. Локальні змінні (N-12).**

```csharp
// ПОГАНО
decimal sum1 = 0; int kolvo = 0;
```

```csharp
// ДОБРЕ
decimal total = 0m;
int lineCount = 0;
```

Цифра в `sum1` натякає на `sum2`, якого немає; `kolvo` — транслітерація.
Дві змінні в одному рядку порушують також F-16.

**Пара 7. Приватне поле й `this.` (N-09, N-10).**

```csharp
// ПОГАНО
private readonly List<RepairWork> lines = new();
public void AddLine(RepairWork lines) => this.lines.Add(lines);
```

```csharp
// ДОБРЕ
private readonly List<RepairWork> _lines = new();
public void AddLine(RepairWork line) => _lines.Add(line);
```

Однакові імена поля й параметра змушують писати `this.`, а помилка в одному
місці (`lines.Add(lines)`) компілюється й тихо нічого не робить.

**Пара 8. Інтерфейс і параметр типу (N-03, N-05).**

```csharp
// ПОГАНО
public interface TicketStorage { }
public sealed class Result<Data> { }
```

```csharp
// ДОБРЕ
public interface IRepairTicketRepository { }
public sealed class Result<T> { }
```

Без `I` у конструкторі сервісу не видно, що це абстракція; `Data` у
`Result<Data>` легко сплутати з реальним типом `Data`.

**Пара 9. Ім’я, що бреше (N-22).**

```csharp
// ПОГАНО
public decimal GetTotal(RepairTicket ticket)
{
    decimal total = PriorityRules.TotalOf(ticket.Lines, ticket.Priority);
    _repository.Add(ticket);
    return total;
}
```

```csharp
// ДОБРЕ
public decimal TotalOf(RepairTicket ticket) =>
    PriorityRules.TotalOf(ticket.Lines, ticket.Priority);

public void Place(RepairTicket ticket) => _repository.Add(ticket);
```

`GetTotal` обіцяє лише прочитати суму, а насправді ще й зберігає заявку:
повторний виклик у звіті створить дублікат. Розділення на запит і команду
прибирає прихований побічний ефект.

**Пара 10. Синонім замість терміна з глосарію (N-17).**

```csharp
// ПОГАНО
public IReadOnlyList<RepairRequest> FindOrders(RepairStatus state)
```

```csharp
// ДОБРЕ
public IReadOnlyList<RepairTicket> Find(TicketStatus status)
```

Один метод змішує три назви однієї заявки (`Request`, `Order`, `Status`
з іншої моделі). Пошук `RepairTicket` цей метод не знайде.

**Пара 11. Назва тесту (N-16).**

```csharp
// ПОГАНО
[Fact]
public void Test1()
```

```csharp
// ДОБРЕ
[Fact]
public void Pay_LessThanTotal_ThrowsWithAmountRule()
```

У звіті конвеєра «Test1 failed» не каже нічого; друга назва описує метод,
вхідну умову й очікуваний результат.

**Пара 12. Абревіатури (N-20).**

```csharp
// ПОГАНО
public sealed class RepairTicketCSVReport
public int TicketID { get; }
```

```csharp
// ДОБРЕ
public sealed class RepairTicketCsvReport
public int TicketId { get; }
```

`CSVReport` злипається в одне слово, і автодоповнення за великими літерами
(`RTCR`) його не знаходить.

## 2.3. Заборонені антипатерни (N-22)

| Антипатерн | Приклад порушення | Чим шкідливий | Як виправити |
|---|---|---|---|
| ім’я-затичка | `data`, `info`, `temp`, `obj`, `item1` | не каже, що лежить у змінній | `ticket`, `pricesByCode` |
| транслітерація | `zayavka`, `kolvo`, `Kilkist` | не читається англійською, різні автори транслітерують по-різному | `RepairTicket`, `quantity` |
| угорська нотація | `strName`, `iCount`, `lstLines` | тип і так знає компілятор; після зміни типу ім’я бреше | `name`, `count`, `lines` |
| нестандартне скорочення | `calcOrdTot`, `Chg`, `Ck` | кожен читач розшифровує по-своєму | `CalculateTotal`, `TryChangeStatus` |
| `And` в імені методу | `PayAndIssueAndNotify` | ознака двох і більше обов’язків | `Pay` + окрема подія `TicketIssued` |
| беззмістовний суфікс | `TicketManager`, `PriceHelper`, `Utils` | клас стає звалищем будь-яких методів | `RepairTicketService`, `PriorityRules` |
| числовий суфікс | `Calculate2`, `TotalNew`, `OrderFinal` | не пояснює різниці між версіями | назва за відмінністю: `TotalWithSurcharge` |
| ім’я, що бреше | `GetTotal`, який ще й зберігає | читач не очікує побічного ефекту | розділити на запит і команду (пара 9) |
