# CrossApp

Лабораторна робота 2: бібліотека `Core`, консольний застосунок `Cli`,
multi-targeting та публікація.

## Предметна область

**Бібліотека**: книги, примірники, читачі та видачі.

Майбутня структура `Core`:

- `Core/Dto/` — record-типи даних;
- `Core/Domain/` — сутності з поведінкою та інваріантами;
- `Core/Storage/` — реалізації сховищ.

## Структура solution

```text
CrossApp/
├── CrossApp.slnx
├── README.md
└── src/
  ├── Core/
  │   ├── Core.csproj
  │   └── EnvironmentInfo.cs
  └── Cli/
  ├── Cli.csproj
  └── Program.cs

```
`Core` є class library без точки входу. `Cli` має односторонню залежність
через `ProjectReference`: `Cli -> Core`. Дані середовища збираються в
`EnvironmentInfo`, а `Program.cs` лише отримує report і форматує його.

## Середовище та multi-targeting

- macOS Apple Silicon (`osx-arm64`);
- .NET SDK 8 і 10;
- Core TFM: `net8.0;net10.0`;
- Cli TFM: `net8.0`.

Команди створення залежності:

```bash
dotnet sln add src/Core/Core.csproj
dotnet add src/Cli/Cli.csproj reference src/Core/Core.csproj
```

Збірка та запуск:

```bash
dotnet build CrossApp.slnx
dotnet build src/Core/Core.csproj
dotnet run --project src/Cli/Cli.csproj
dotnet run --project src/Cli/Cli.csproj -- --json
```

У `EnvironmentInfo` використано умовну компіляцію:
`NET10_0_OR_GREATER` додає build note для `net10.0`, а для `net8.0`
виводиться відповідний note.

## Публікація

Публікації виконуються в `bin/Release/net8.0/<rid>/publish/` і не входять
до git завдяки правилу `bin/` у `.gitignore`.

```bash
# Framework-dependent: потрібен встановлений .NET runtime
dotnet publish src/Cli -c Release -f net8.0 -r osx-arm64 --self-contained false

# Self-contained: runtime .NET входить до публікації
dotnet publish src/Cli -c Release -f net8.0 -r osx-arm64 --self-contained true

# Додатково: один виконуваний файл
dotnet publish src/Cli -c Release -f net8.0 -r osx-arm64 --self-contained true \
  -p:PublishSingleFile=true

# Додатково: trimming, перевіряти попередження рефлексії
dotnet publish src/Cli -c Release -f net8.0 -r osx-arm64 --self-contained true \
  -p:PublishTrimmed=true
```

Публікацію потрібно запускати без `dotnet run`, напряму з каталогу `publish`:

```bash
./src/Cli/bin/Release/net8.0/osx-arm64/publish/Cli
```

Розмір каталогу можна перевірити так:

```bash
du -sh src/Cli/bin/Release/net8.0/osx-arm64/publish
```

## Порівняння публікацій

Фактичні розміри отримано на macOS Apple Silicon (`osx-arm64`). Для цього
середовища publish виконувався з `-f net8.0 --no-restore`, оскільки runtime
pack вже був доступний локально:

| RID | Режим | Розмір publish | Потрібен встановлений runtime |
| --- | --- | ---: | --- |
| `osx-arm64` | framework-dependent | 164 KB | так, .NET 8 |
| `osx-arm64` | self-contained | 76 MB | ні |
| `osx-arm64` | self-contained + single-file | 70 MB | ні |
| `osx-arm64` | self-contained + trimmed | 76 MB | ні |

Framework-dependent містить код застосунку та залежності, але не .NET runtime,
тому займає менше місця. Self-contained містить runtime для конкретного RID,
тому працює без попередньої інсталяції .NET, але каталог значно більший.

`PublishSingleFile` об'єднує компоненти застосунку в один виконуваний файл.
`PublishTrimmed` видаляє невикористаний код, але може спричинити проблеми з
рефлексією. У цьому простому застосунку trimming зібрався успішно, проте
розмір майже не зменшився.

## Перевірка

```bash
dotnet sln list CrossApp.slnx
git status --short
```

У `Program.cs` немає викликів `RuntimeInformation`; ця логіка належить Core.
Core можна підключити до майбутніх `Api` та Blazor-проєктів без залежності від
консольного інтерфейсу.
