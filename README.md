# RepairService

[![CI](https://github.com/alivieE/RepairService/actions/workflows/ci.yml/badge.svg)](https://github.com/alivieE/RepairService/actions/workflows/ci.yml)

Консольний застосунок для приймальника сервісного центру: приймає заявки на
ремонт, рахує дедлайн і вартість за пріоритетом, проводить заявку станами й
показує звіт про прострочені заявки.

## Автор і варіант

Новиков Роман Максимович, група 471, варіант 11 «RepairService — пріоритети
заявок, розрахунок дедлайну». Дисципліна «Конструювання програмного
забезпечення», самостійна робота № 3.

## Швидкий старт

Потрібен .NET SDK 8.0.

```powershell
git clone https://github.com/alivieE/RepairService.git
cd RepairService
dotnet run --project RepairService.App
```

`dotnet run` сам відновлює пакети й складає рішення. Дані зберігаються в
`data/tickets.json`, журнал — у `repairservice.log` у поточній теці.

## Структура рішення

| Проєкт | Призначення |
|--------|-------------|
| `RepairService.Core` | бібліотека логіки: домен, прикладні сервіси, сховища, правила |
| `RepairService.App` | консольне меню, журнал у файл і композиційний корінь |
| `RepairService.Tests` | модульні тести бібліотеки логіки (xUnit) |
| `src/Legacy` | успадкований модуль із ЛР 7–8, залишений як зразок рефакторингу |
| `tests/Legacy.Tests` | характеризаційні тести успадкованого модуля |

Шари й правило залежностей описано в [docs/architecture.md](docs/architecture.md).

## Можливості

Сценарії описано в [docs/spec.md](docs/spec.md):

- UC-01 — прийняти заявку з дедлайном за пріоритетом;
- UC-02 — провести заявку станами Received → InWork → Ready або Rejected;
- UC-03 — додати роботу чи деталь із прейскуранта (60 позицій);
- UC-04 — знайти заявки за станом і пріоритетом;
- UC-05 — звіт «Прострочені заявки» на дату з кількістю днів прострочення;
- UC-06 — видати пристрій з оплатою; менша сума відхиляється.

Як користуватися меню — [docs/user-guide.md](docs/user-guide.md).

## Тести й покриття

```powershell
dotnet test
dotnet test /p:CollectCoverage=true /p:Include="[RepairService.Core]*"
```

Поточний стан: 165 тестів (12 + 153), усі зелені. Покриття `RepairService.Core`:
93,33 % за рядками, 92,23 % за гілками, 92,77 % за методами.

Локальний еквівалент конвеєра CI — ті самі перевірки, що й у
`.github/workflows/ci.yml`:

```powershell
dotnet format --verify-no-changes; dotnet build -c Release; dotnet test /p:CollectCoverage=true /p:Include="[RepairService.Core]*"
```

## Технології

.NET 8, C# 12, xUnit, coverlet, аналізатори Roslyn і StyleCop
(`TreatWarningsAsErrors`), `.editorconfig` зі стандартом кодування
([CODING_STANDARD.md](CODING_STANDARD.md)), GitHub Actions.

## Обмеження й відомий борг

- Довідника клієнтів немає: номер клієнта перевіряється лише як додатне число,
  тому альтернатива UC-01 «клієнта не знайдено» не реалізована.
- Пошук працює за станом і пріоритетом; пошуку за клієнтом поки немає.
- JSON-файл читається й перезаписується цілком на кожну операцію — підходить
  для сотень заявок, але не для одночасної роботи кількох приймальників.
- Наряд `WorkOrder` з ЛР 2 ще читає системний час напряму (борг TD-10).
- Застосунок консольний і однокористувацький, без розмежування прав.
