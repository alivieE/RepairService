# 6. Машинна частина: .editorconfig і аналізатори

## 6.1. Правила

**A-01. Єдине джерело правил стилю — `.editorconfig` у корені репозиторію з `root = true`.**
- **Рівень:** ОБОВ’ЯЗКОВО.
- **Обґрунтування:** налаштування окремої IDE в кожного розробника різні, а
  `.editorconfig` однаково читають VS Code, Visual Studio, Rider, `dotnet build`
  і `dotnet format`. `root = true` не дає підхопити чужий файл із батьківської теки.
- **Перевірка:** файл у репозиторії; збірка й конвеєр застосовують його автоматично.

**A-02. Спільні властивості складання задають у `Directory.Build.props`; `.csproj` їх не дублює.**
- **Рівень:** РЕКОМЕНДОВАНО.
- **Обґрунтування:** якщо `Nullable` задано в кожному проєкті окремо, рано чи
  пізно новий проєкт його не отримає. Виняток — `src/Legacy`, який свідомо
  вимикає аналізатори у своєму `.csproj`.
- **Порушення:** `<GenerateDocumentationFile>true</GenerateDocumentationFile>` у `RepairService.Core.csproj` поряд із тим самим у props.
- **Дотримання:** властивість лише в `Directory.Build.props`.
- **Перевірка:** R-18.

**A-03. Увімкнено вбудовані аналізатори .NET (`AnalysisLevel 8.0`, `AnalysisMode Recommended`) і StyleCop.Analyzers.**
- **Рівень:** ОБОВ’ЯЗКОВО.
- **Обґрунтування:** аналізатори .NET ловлять дефекти поведінки (культура,
  `null`, складність), StyleCop — структуру файлу й документацію. Версію
  рівня зафіксовано, щоб нове SDK не додало правил без рішення команди.
- **Перевірка:** `Directory.Build.props`, пакет `StyleCop.Analyzers 1.2.0-beta.556` у Core і App.

**A-04. Правило придушують лише в `.editorconfig` з коментарем-обґрунтуванням або атрибутом `SuppressMessage` з непорожнім `Justification`; `#pragma warning disable` заборонено.**
- **Рівень:** ЗАБОРОНЕНО (`#pragma`).
- **Обґрунтування:** придушення без пояснення — прихована зміна стандарту.
  `#pragma` у тілі методу непомітна на огляді, а коментар у `.editorconfig`
  читають усі.
- **Порушення:** `#pragma warning disable CA1062`
- **Дотримання:** атрибут нижче або рядок у `.editorconfig` із коментарем над ним.
- **Перевірка:** R-17; пошук `git grep -n "pragma warning disable"`.

```csharp
// ПОГАНО
#pragma warning disable CA1062
public void Place(RepairTicket ticket) { ... }

// ДОБРЕ
[SuppressMessage(
    "Design",
    "CA1062:Validate arguments of public methods",
    Justification = "Заявку створює лише TicketFactory, яка вже перевірила null")]
public void Place(RepairTicket ticket) { ... }
```

**A-05. Перед комітом виконують три команди перевірки.**
- **Рівень:** РЕКОМЕНДОВАНО.
- **Обґрунтування:** локальна перевірка займає ~15 секунд, а червоний
  конвеєр — кілька хвилин і блокує інших.
- **Перевірка:** конвеєр (A-06) повторює ці самі команди.

```powershell
dotnet format --verify-no-changes   # код 0 — форматування й стиль відповідають .editorconfig
dotnet build -warnaserror           # 0 попереджень і 0 помилок, зокрема аналізаторів
dotnet test                         # усі тести зелені
```

**A-06. Конвеєр GitHub Actions повторює перевірки на кожен push у `main` і `feature/**`; червона збірка — найвищий пріоритет.**
- **Рівень:** ОБОВ’ЯЗКОВО.
- **Обґрунтування:** локальну перевірку можна забути, конвеєр — ні.
- **Перевірка:** `.github/workflows/ci.yml`, бейдж у README.

## 6.2. Таблиця пояснень `.editorconfig`

Діагностика `error` відповідає рівням ОБОВ’ЯЗКОВО й ЗАБОРОНЕНО,
`suggestion` — РЕКОМЕНДОВАНО, `silent` або `none` — ДОПУСКАЄТЬСЯ (ADR-002).

| № | Ключ | Значення | Що контролює |
|---|---|---|---|
| 1 | `root` | `true` | файл кореневий, батьківські не читаються (A-01) |
| 2 | `charset` | `utf-8` | кодування файлів (F-03) |
| 3 | `insert_final_newline` | `true` | порожній рядок у кінці файлу (F-03) |
| 4 | `trim_trailing_whitespace` | `true` | без пробілів у кінці рядка (F-03) |
| 5 | `indent_style` | `space` | відступ пробілами (F-01) |
| 6 | `indent_size` | `4`, для XML/JSON/YAML `2` | ширина відступу (F-01) |
| 7 | `max_line_length` | `100` | межа рядка для редактора (F-02, без машинної перевірки) |
| 8 | `csharp_new_line_before_open_brace` | `all` | дужки в стилі Allman (F-05) |
| 9 | `csharp_new_line_before_else` / `_catch` / `_finally` | `true` | `else`, `catch`, `finally` з нового рядка (F-05) |
| 10 | `csharp_prefer_braces` | `true:error` | дужки навіть для одного оператора (F-06) |
| 11 | `csharp_space_around_binary_operators` | `before_and_after` | пробіли навколо операторів (F-17) |
| 12 | `csharp_style_namespace_declarations` | `file_scoped:error` | простір імен без блоку (F-10) |
| 13 | `csharp_using_directive_placement` | `outside_namespace:error` | `using` поза простором імен (F-11) |
| 14 | `dotnet_sort_system_directives_first` | `true` | `System` першим у списку `using` (F-11) |
| 15 | `dotnet_diagnostic.IDE0005.severity` | `error` | невживані `using` (F-11) |
| 16 | `csharp_style_expression_bodied_methods` | `when_on_single_line:silent` | тіла-вирази допускаються (F-15) |
| 17 | `dotnet_style_namespace_match_folder` | `true:error` | простір імен = теки (N-01) |
| 18 | `dotnet_style_qualification_for_field` / `_property` / `_method` | `false:error` | без `this.` (N-10) |
| 19 | `dotnet_style_require_accessibility_modifiers` | `for_non_interface_members:error` | модифікатор доступу явно, крім членів інтерфейсу (N-19) |
| 20 | `dotnet_diagnostic.CA1715.severity` | `error` | префікси `I` та `T` (N-03, N-05) |
| 21 | `dotnet_diagnostic.CA1717.severity` | `error` | перелічення в однині (N-04) |
| 22 | `dotnet_naming_rule.priv_underscore.*` | `_camelCase`, `error` | приватні поля (N-09) |
| 23 | `dotnet_naming_rule.const_pascal.*` | PascalCase, `error` | константи (N-11) |
| 24 | `dotnet_naming_rule.async_suffix.*` | суфікс `Async`, `error` | асинхронні методи (N-07) |
| 25 | `dotnet_naming_rule.members_pascal.*` | PascalCase, `error` | типи, методи, властивості, події (N-02, N-06, N-08) |
| 26 | `dotnet_naming_rule.locals_camel.*` | camelCase, `error` | параметри й локальні змінні (N-12) |
| 27 | `dotnet_diagnostic.IDE1006.severity` | `error` | правила іменування перевіряються під час складання |
| 28 | `csharp_style_var_for_built_in_types` | `false:suggestion` | явний тип для `int`, `decimal`, `string` (D-03) |
| 29 | `csharp_style_var_when_type_is_apparent` | `true:suggestion` | `var`, коли тип видно праворуч (D-03) |
| 30 | `dotnet_diagnostic.IDE0059` / `IDE0060` / `IDE0051` / `IDE0052` | `error` | невикористані значення, параметри, члени (D-04) |
| 31 | `dotnet_style_readonly_field` | `true:error` | `readonly` для незмінних полів (D-07) |
| 32 | `dotnet_style_predefined_type_for_locals_parameters_members` | `true:error` | `int`, а не `Int32` (D-12) |
| 33 | `dotnet_diagnostic.CA1305` / `CA1307` | `error` | явна культура й `StringComparison` (D-19) |
| 34 | `dotnet_diagnostic.CA1062.severity` | `error` | перевірка аргументів на `null` (S-02) |
| 35 | `dotnet_diagnostic.IDE0100.severity` | `error` | без `== true` (S-08) |
| 36 | `dotnet_diagnostic.CA1502.severity` | `error` | цикломатична складність ≤ 12 (S-11) |
| 37 | `dotnet_diagnostic.CS1591` / `SA1600`–`SA1602` | `error` | XML-документація (C-07) |
| 38 | `dotnet_diagnostic.SA1611` / `SA1615` | `error` | теги `param` і `returns` (C-08) |
| 39 | `dotnet_diagnostic.SA1516` / `SA1507` | `error` | порожні рядки (F-07) |
| 40 | `dotnet_diagnostic.SA1402` / `SA1649` | `error` | один тип на файл, ім’я файлу (F-08, N-18) |
| 41 | `dotnet_diagnostic.SA1201` / `SA1202` / `SA1204` | `error` | порядок членів класу (F-09) |
| 42 | `dotnet_diagnostic.SA1116` / `SA1117` | `error` | перенесення аргументів (F-12) |
| 43 | `dotnet_diagnostic.SA1124.severity` | `error` | регіони заборонено (F-13) |
| 44 | `dotnet_diagnostic.SA1107.severity` | `error` | один оператор на рядок (F-16) |

## 6.3. Свідомі придушення

| Правило | Чому придушено |
|---|---|
| SA1633 | заголовок із ліцензією в кожному файлі навчального проєкту не потрібен |
| SA1101 | вимагає `this.` перед членами — суперечить N-10 і конвенції .NET |
| SA1200 | вимагає `using` усередині простору імен — суперечить F-11 і шаблонам .NET |
| SA1309 | забороняє `_` на початку поля — суперечить N-09 |
| SA0001 | скаржиться на відсутній файл документації; його не генерує лише `src/Legacy`, де аналізатори вимкнено |
| SA1623, SA1642 | вимагають англійських зачинів «Gets…», «Initializes a new instance…», а документацію ведемо українською (C-10) |
| SA1413 | вимагає коми після останнього елемента ініціалізатора — у стандарті це ДОПУСКАЄТЬСЯ (F-14) |
| CA1848 | вимагає генератора `LoggerMessage`; для навчального консольного застосунку достатньо іменованих заповнювачів |
| IDE0010 | вимагає перелічити в `switch` кожен член перелічення; гілка `_ => throw` (S-05) робить це надлишковим |
| CA1707 (тести) | забороняє `_` в іменах; тестові методи навмисно мають вигляд `Метод_Сценарій_Результат` (N-16) |
| CA1307 (тести) | `Assert.Contains` перевіряє фрагмент тексту, сформованого самим тестом |
| CS1591 (тести) | ім’я тесту саме описує сценарій, XML-коментар його лише повторив би |

Кожне придушення записано в `.editorconfig` з коментарем над рядком; у коді
немає жодного `#pragma warning disable` і жодного `SuppressMessage`.

## 6.4. `Directory.Build.props`

| Властивість | Значення | Призначення |
|---|---|---|
| `TargetFramework` | `net8.0` | усі проєкти на одній платформі, версію SDK додатково закріплює `global.json` |
| `Nullable` | `enable` | компілятор попереджає про можливе розіменування `null` (D-11) |
| `ImplicitUsings` | `enable` | `System`, `System.Linq` та інші базові простори імен підключено без `using` |
| `LangVersion` | `12.0` | версію мови зафіксовано, нові можливості вмикають рішенням команди |
| `EnableNETAnalyzers` | `true` | вбудовані аналізатори .NET (A-03) |
| `AnalysisLevel` | `8.0` | набір правил .NET 8, не змінюється з новим SDK |
| `AnalysisMode` | `Recommended` | рекомендований набір правил увімкнено як попередження |
| `EnforceCodeStyleInBuild` | `true` | правила стилю IDE (IDE0011, IDE1006 тощо) перевіряються під час `dotnet build` |
| `TreatWarningsAsErrors` | `true` | будь-яке попередження зупиняє збірку |
| `GenerateDocumentationFile` | `true`, крім `RepairService.Legacy` | вмикає CS1591 і перевірку XML-коментарів (C-12) |
| `VersionPrefix`, `Authors`, `Product` | `0.8.0`, автор, назва | метадані збірки (ЛР8) |
| `NuGetAudit`, `NuGetAuditMode` | `true`, `all` | вразливий пакет, зокрема транзитивний, зупиняє збірку |
| `AdditionalFiles` | `CodeMetricsConfig.txt` | поріг CA1502 = 12 (S-11) |

## 6.5. Доказ роботи механізму

1. До ядра тимчасово додано файл `StandardDemo.cs` із полем без `_`, `if`
   без дужок і порівнянням `== true`. Збірка впала з шістьма помилками:
   IDE1006 (N-09), IDE0044 (D-07), IDE0011 і SA1503 (F-06), IDE0100 (S-08),
   CS0649 (D-04). Після видалення файлу збірка чиста.
2. Тимчасовий поріг `CA1502: 2` змусив аналізатор показати складність кожного
   методу ядра; значення збіглися з ручним підрахунком (5.3).
3. Після зміни `.editorconfig` перевірка `dotnet format --verify-no-changes`
   повернула код 2: правило IDE0005 знайшло два зайві `using` у
   `src/Legacy/LegacyTicketProcessor.cs`, який збірка не аналізує. Перший
   запуск конвеєра на гілці став червоним; після виправлення — зеленим.