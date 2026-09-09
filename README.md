# CrossApp

Це мій проєкт з лабораторних робіт з крос-платформного програмування.

## Предметна область

Я обрала тему **Бібліотека**. У майбутньому в програмі будуть такі основні
сутності:

- `Book` — книга;
- `BookCopy` — примірник книги;
- `Reader` — читач;
- `Loan` — видача книги.

Програма буде потрібна для обліку книг, читачів, видачі та повернення
примірників.

## Що є в проєкті

```text
CrossApp.sln
README.md
REPORT.md
src/Cli/Cli.csproj
src/Cli/Program.cs
```

Консольна програма показує інформацію про операційну систему, архітектуру
процесу, версію .NET і робочі каталоги.

## Як запустити

Потрібен встановлений .NET SDK 8 або новіший.

```bash
dotnet build
dotnet run --project src/Cli
```

Для додаткового режиму з JSON:

```bash
dotnet run --project src/Cli -- --json
```

## Self-contained публікація

| RID | Розмір каталогу publish |
| --- | ---: |
| `osx-arm64` | 76 MB |
| `linux-x64` | 71 MB |

```bash
dotnet publish src/Cli -c Release -r osx-arm64 --self-contained true
dotnet publish src/Cli -c Release -r linux-x64 --self-contained true
```

Для запуску в Docker:

```bash
docker run --rm -v ${PWD}:/src -w /src mcr.microsoft.com/dotnet/sdk:8.0 \
  dotnet run --project src/Cli
```

Локально програма показує `Darwin`, а в контейнері має показувати `Linux`.

## Середовище

- macOS;
- Apple Silicon (Arm64);
- Visual Studio Code;
- .NET SDK 8;
- група ФЕІ-34.
