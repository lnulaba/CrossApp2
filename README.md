# CrossApp

Лабораторна робота 1 з крос-платформного програмування.

## Тема

Налаштування середовища і старт .NET-проєкту.

## Мета

Створити solution і консольний застосунок, перевірити запуск на різних
платформах та визначити предметну область для наступних лабораторних робіт.

## Предметна область

**Бібліотека**:

- `Book` — книга;
- `BookCopy` — примірник книги;
- `Reader` — читач;
- `Loan` — видача книги.

Майбутня система призначена для обліку книг, читачів, видачі та повернення
примірників.

## Структура

- `CrossApp.sln` — solution;
- `src/Cli/Cli.csproj` — консольний .NET-проєкт;
- `src/Cli/Program.cs` — код програми;

## Середовище

- macOS на Apple Silicon (`Arm64`);
- .NET SDK 8;
- Visual Studio Code;
- TFM: `net8.0`.

## Запуск

З кореня репозиторію:

```bash
dotnet build CrossApp.sln
dotnet run --project src/Cli/Cli.csproj
```

Для виведення інформації одним JSON-рядком:

```bash
dotnet run --project src/Cli/Cli.csproj -- --json
```

Програма показує операційну систему, архітектуру процесу, версію .NET,
runtime, каталоги запуску та предметну область.

## Додаткове завдання

### 1. Self-contained публікація

```bash
dotnet publish src/Cli -c Release -r osx-arm64 --self-contained true
dotnet publish src/Cli -c Release -r linux-x64 --self-contained true
```

Підготовлено публікацію для двох RID. Розмір каталогів `publish`:

| RID | Розмір |
| --- | ---: |
| `osx-arm64` | 76 MB |
| `linux-x64` | 71 MB |

### 2. JSON-режим

Прапорець `--json` виводить ту саму інформацію одним JSON-рядком:

```bash
dotnet run --project src/Cli/Cli.csproj -- --json
```

### 3. Запуск у Docker

```bash
docker run --rm -v ${PWD}:/src -w /src mcr.microsoft.com/dotnet/sdk:8.0 \
  dotnet run --project src/Cli
```

Локально `OSDescription` визначається як `Darwin`, а в Linux-контейнері — як
`Linux`. Для Docker Desktop демон має бути запущений.
