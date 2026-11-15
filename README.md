# RepairService

![CI](https://github.com/alivieE/RepairService/actions/workflows/ci.yml/badge.svg)

Навчальний проєкт з дисципліни «Конструювання програмного
забезпечення». Варіант № 11: заявки на ремонт у сервісному центрі —
приймання пристрою, діагностика, ремонт, видача клієнту й розрахунок
вартості робіт і запчастин.

## Склад рішення

- RepairService.Core — логіка предметної області (бібліотека)
- RepairService.App — консольний застосунок (введення, виведення)
- RepairService.Tests — модульні тести ядра (xUnit, coverlet)
- src/Legacy — успадкований модуль із ЛР6
- tests/Legacy.Tests — характеризаційні тести успадкованого модуля

## Вимоги

.NET SDK 8.0 або новіший, Git. Версію SDK закріплено у файлі global.json.

## Складання та запуск

    dotnet build
    dotnet run --project RepairService.App

Спільні властивості складання (версія, попередження як помилки,
аудит пакетів NuGet) задано в Directory.Build.props.

## Конвеєр CI

Файл .github/workflows/ci.yml запускається на кожен push у main
і feature/** та на кожен pull request у main. Зміна потрапляє в main,
лише якщо виконано всі критерії якості:

| Критерій     | Команда                             | Поріг                 |
|--------------|-------------------------------------|-----------------------|
| Форматування | dotnet format --verify-no-changes   | без змін              |
| Складання    | dotnet build -c Release             | 0 попереджень         |
| Тести        | dotnet test -c Release              | 100 % зелених         |
| Покриття     | coverlet, /p:Threshold=60           | рядків не менше 60 %  |

Ті самі перевірки локально:

    dotnet format --verify-no-changes
    dotnet build -c Release
    dotnet test -c Release /p:CollectCoverage=true /p:Threshold=60

## Стандарт кодування

Правила описано у файлі CODING_STANDARD.md і закріплено
в .editorconfig. Перевірка: dotnet format --verify-no-changes

## Автор

Новиков Роман, група 471