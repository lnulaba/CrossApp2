# CrossApp

Лабораторна робота 2: бібліотека `Core`, консольний застосунок `Cli`,
multi-targeting та публікація.

## Предметна область

**Бібліотека**: книги, примірники, читачі та видачі.

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

`Core` є class library без точки входу. `Cli` має залежність від `Core` через
`ProjectReference`: `Cli -> Core`.

## Середовище та multi-targeting

- macOS Apple Silicon: `osx-arm64`;
- .NET SDK: 8 і 10;
- Core TFM: `net8.0;net10.0`;
- Cli TFM: `net8.0`.

`EnvironmentInfo` використовує умовну компіляцію: для `net10.0` і `net8.0`
виводиться відповідний build note.

## Команди лабораторної роботи №2

Усі команди нижче виконуються з кореня репозиторію.

### 1. Перейти до репозиторію

```bash
cd /Users/marta/Desktop/cross/lab_1
```

### 2. Показати встановлені SDK

```bash
dotnet --list-sdks
```

### 3. Показати встановлені runtime

```bash
dotnet --list-runtimes
```

### 4. Показати повну інформацію про .NET

```bash
dotnet --info
```

### 5. Додати проєкти до solution

```bash
dotnet sln add src/Core/Core.csproj
dotnet sln add src/Cli/Cli.csproj
```

### 6. Додати залежність `Cli -> Core`

```bash
dotnet add src/Cli/Cli.csproj reference src/Core/Core.csproj
```

### 7. Відновити залежності

```bash
dotnet restore
```

### 8. Зібрати всю solution

```bash
dotnet build CrossApp.slnx
```

### 9. Зібрати `Core` під .NET 8

```bash
dotnet build src/Core/Core.csproj -f net8.0
```

### 10. Зібрати `Core` під .NET 10

```bash
dotnet build src/Core/Core.csproj -f net10.0
```

### 11. Зібрати CLI для прямого запуску

```bash
dotnet build src/Cli/Cli.csproj
```

### 12. Запустити звичайний режим без `dotnet run`

```bash
./src/Cli/bin/Debug/net8.0/Cli
```

### 13. Запустити JSON-режим без `dotnet run`

```bash
./src/Cli/bin/Debug/net8.0/Cli --json
```

## Публікація

Публікації створюються в `src/Cli/bin/Release/net8.0/osx-arm64/publish/`.

### 14. Очистити попередню публікацію

```bash
rm -rf src/Cli/bin/Release/net8.0/osx-arm64/publish
```

### 15. Створити framework-dependent публікацію

Потрібен встановлений .NET 8 runtime:

```bash
dotnet publish src/Cli -c Release -f net8.0 -r osx-arm64 --self-contained false
```

### 16. Запустити framework-dependent публікацію

```bash
./src/Cli/bin/Release/net8.0/osx-arm64/publish/Cli --json
```

### 17. Створити self-contained публікацію

Runtime .NET входить до публікації:

```bash
dotnet publish src/Cli -c Release -f net8.0 -r osx-arm64 --self-contained true
```

### 18. Запустити self-contained публікацію

```bash
./src/Cli/bin/Release/net8.0/osx-arm64/publish/Cli --json
```

### 19. Створити single-file публікацію

```bash
dotnet publish src/Cli -c Release -f net8.0 -r osx-arm64 \
  --self-contained true \
  -p:PublishSingleFile=true
```

### 20. Запустити single-file публікацію

```bash
./src/Cli/bin/Release/net8.0/osx-arm64/publish/Cli --json
```

### 21. Створити trimmed-публікацію

```bash
dotnet publish src/Cli -c Release -f net8.0 -r osx-arm64 \
  --self-contained true \
  -p:PublishTrimmed=true
```

### 22. Запустити trimmed-публікацію

```bash
./src/Cli/bin/Release/net8.0/osx-arm64/publish/Cli
```

### 23. Перевірити розмір публікації

```bash
du -sh src/Cli/bin/Release/net8.0/osx-arm64/publish
```

## Запуск у Docker

Docker image `mcr.microsoft.com/dotnet/sdk:8.0` містить тільки SDK 8, тому
для нього потрібно явно залишити target `net8.0`.

### 24. Запустити звичайний режим у Docker SDK 8

```bash
docker run --rm \
  -v "${PWD}:/src" \
  -w /src \
  mcr.microsoft.com/dotnet/sdk:8.0 \
  dotnet run \
  --project src/Cli/Cli.csproj \
  -p:TargetFrameworks=net8.0
```

### 25. Запустити JSON-режим у Docker SDK 8

```bash
docker run --rm \
  -v "${PWD}:/src" \
  -w /src \
  mcr.microsoft.com/dotnet/sdk:8.0 \
  dotnet run \
  --project src/Cli/Cli.csproj \
  -p:TargetFrameworks=net8.0 \
  -- --json
```

### 26. Зібрати обидва target framework у Docker SDK 10

```bash
docker run --rm \
  -v "${PWD}:/src" \
  -w /src \
  mcr.microsoft.com/dotnet/sdk:10.0 \
  dotnet build CrossApp.slnx
```

## Додаткові завдання

### 29. Зібрати solution у конфігурації Release

```bash
dotnet build CrossApp.slnx -c Release
```

### 30. Перевірити build note для `net10.0`

```bash
dotnet build src/Core/Core.csproj -f net10.0 -c Release
```

### 31. Опублікувати застосунок для Linux ARM64 у Docker

```bash
mkdir -p publish/linux-arm64
docker run --rm \
  -v "${PWD}:/src" \
  -w /src \
  mcr.microsoft.com/dotnet/sdk:8.0 \
  dotnet publish src/Cli/Cli.csproj \
  -c Release \
  -f net8.0 \
  -r linux-arm64 \
  --self-contained true \
  -p:TargetFrameworks=net8.0 \
  -o /src/publish/linux-arm64
```

### 32. Запустити Linux-публікацію в Docker

```bash
docker run --rm \
  -v "${PWD}/publish/linux-arm64:/app" \
  mcr.microsoft.com/dotnet/runtime:8.0 \
  /app/Cli --json
```

### 33. Опублікувати single-file для Linux x64

```bash
dotnet publish src/Cli -c Release -f net8.0 -r linux-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -o publish/linux-x64
```

### 34. Перевірити створені publish-каталоги

```bash
find publish -maxdepth 2 -type f -perm -111 -print
du -sh publish/*
```

### 35. Очистити результати додаткових публікацій

```bash
rm -rf publish
```

## Фінальна перевірка

### 27. Показати проєкти solution

```bash
dotnet sln list CrossApp.slnx
```

### 28. Перевірити зміни в git

```bash
git status --short
```

## 3. Лабораторна робота 3

Предметна область — бібліотека. `BookDto` і `ReaderDto` лежать у `src/Core/Dto`,
а імпортери — у `src/Core/Import`. Усі команди виконуються з кореня репозиторію.

### 3.1. Перейти до репозиторію

```bash
cd /Users/marta/Desktop/cross/lab_1
```

### 3.2. Перевірити SDK і runtime

```bash
dotnet --list-sdks
dotnet --list-runtimes
dotnet --info
```

### 3.3. Відновити залежності

```bash
dotnet restore
```

### 3.4. Зібрати solution під .NET 8 і .NET 10

```bash
dotnet build CrossApp.slnx
dotnet build src/Core/Core.csproj -f net8.0
dotnet build src/Core/Core.csproj -f net10.0
dotnet build src/Cli/Cli.csproj
```

### 3.5. Запустити імпорт CSV

У файлі `data/sample.csv` є 10 коректних записів і 3 навмисно пошкоджені рядки.

```bash
dotnet run --project src/Cli/Cli.csproj -- data/sample.csv
```

### 3.6. Запустити імпорт за замовчуванням

Якщо шлях не передати, використовується `data/sample.csv`.

```bash
dotnet run --project src/Cli/Cli.csproj
```

### 3.7. Додаткове завдання: імпорт JSON

```bash
dotnet run --project src/Cli/Cli.csproj -- data/sample.json
```

Імпортер обирається через switch expression за розширенням `.csv` або `.json`.
CLI виводить перші п'ять записів, помилки та статистику імпорту.

### 3.8. Додаткове завдання: різнорідні записи

Префікс `B` означає книгу, а `R` — читача.

```bash
dotnet run --project src/Cli/Cli.csproj -- --mixed data/sample-mixed.csv
```

### 3.9. Перевірити неіснуючий файл

```bash
dotnet run --project src/Cli/Cli.csproj -- data/missing.csv
echo $?
```

Програма показує повний шлях до відсутнього файлу та повертає код завершення `1`.

### 3.10. Запустити зібраний файл напряму

```bash
./src/Cli/bin/Debug/net8.0/Cli data/sample.csv
./src/Cli/bin/Debug/net8.0/Cli data/sample.json
./src/Cli/bin/Debug/net8.0/Cli --mixed data/sample-mixed.csv
```

### 3.11. Перевірити фінальний стан

```bash
dotnet sln list CrossApp.slnx
git status --short
```
