# CrossApp

> Увага: git-репозиторій і solution розташовані в каталозі
> `/Users/marta/Desktop/cross/lab_1`, а не в його батьківському каталозі
> `/Users/marta/Desktop/cross`. Перед командами нижче один раз виконайте:
>
> ```bash
> cd /Users/marta/Desktop/cross/lab_1
> ```
>
> Якщо prompt уже закінчується на `lab_1 %`, повторно виконувати `cd lab_1`
> не потрібно.

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

## Додаткові завдання та команди

Команди 29–37 є додатковими командами лабораторної роботи: Release-збірка,
перевірка `net10.0`, Docker-публікація, різні типи publish і очищення результатів.

### 29. Зібрати solution у конфігурації Release

```bash
dotnet build /Users/marta/Desktop/cross/lab_1/CrossApp.slnx -c Release
```

### 30. Перевірити build note для `net10.0`

```bash
dotnet build /Users/marta/Desktop/cross/lab_1/src/Core/Core.csproj -f net10.0 -c Release
```

### 31. Опублікувати self-contained застосунок для Linux ARM64 у Docker

Тип публікації: `linux-arm64`, self-contained, кілька файлів. Runtime .NET
входить до публікації; Docker використовується лише для виконання команди.

```bash
mkdir -p /Users/marta/Desktop/cross/lab_1/publish/linux-arm64
docker run --rm \
  -v "/Users/marta/Desktop/cross/lab_1:/src" \
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

### 32. Опублікувати framework-dependent застосунок для Windows x64

Потрібен встановлений .NET 10 runtime на Windows. Публікація не містить
.NET runtime і складається з файлів застосунку.

```bash
mkdir -p /Users/marta/Desktop/cross/lab_1/publish/win-x64
dotnet publish /Users/marta/Desktop/cross/lab_1/src/Cli/Cli.csproj \
  -c Release \
  -f net10.0 \
  -r win-x64 \
  --self-contained false \
  -o /Users/marta/Desktop/cross/lab_1/publish/win-x64
```

### 33. Запустити Linux-публікацію в Docker

```bash
docker run --rm \
  -v "/Users/marta/Desktop/cross/lab_1/publish/linux-arm64:/app" \
  mcr.microsoft.com/dotnet/runtime:8.0 \
  /app/Cli --json
```

### 34. Опублікувати self-contained single-file trimmed для Linux x64

Тип публікації: `linux-x64`, self-contained, single-file, trimmed. Runtime .NET
входить до одного виконуваного файла `Cli`; Docker-контейнер не створюється.

```bash
dotnet publish /Users/marta/Desktop/cross/lab_1/src/Cli -c Release -f net8.0 -r linux-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:PublishTrimmed=true \
  -o /Users/marta/Desktop/cross/lab_1/publish/linux-x64-trimmed
```

### 35. Опублікувати self-contained single-file для Linux x64

```bash
dotnet publish /Users/marta/Desktop/cross/lab_1/src/Cli -c Release -f net8.0 -r linux-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -o /Users/marta/Desktop/cross/lab_1/publish/linux-x64
```

### 36. Перевірити створені publish-каталоги

```bash
find /Users/marta/Desktop/cross/lab_1/publish -maxdepth 2 -type f -perm -111 -print
du -sh /Users/marta/Desktop/cross/lab_1/publish/*
```

### 37. Очистити результати додаткових публікацій

```bash
rm -rf /Users/marta/Desktop/cross/lab_1/publish
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

## 4. Лабораторна робота 4

Усі команди цього розділу виконуються з каталогу `/Users/marta/Desktop/cross/lab_1`.
Якщо термінал відкрито в `/Users/marta/Desktop/cross`, спочатку виконайте
`cd /Users/marta/Desktop/cross/lab_1`. У системах без `rg` для пошуку нижче
використовується стандартний `grep`.

Предметна область — бібліотека. Домен містить `BookCopy` (примірник книги),
`Loan` (видача), `LibraryLendingService` (правило максимум п'ять відкритих
видач на читача) та `Order` з явними станами `Draft`, `Confirmed`, `Cancelled`.
DTO тижня 3 не замінюються доменними сутностями.

### 4.1. Інваріанти

- `BookCopy.Id` та `BookCopy.Isbn` не можуть бути порожніми — `ArgumentException`
  у `BookCopy.Create`.
- Видати вже виданий примірник або повернути доступний примірник не можна —
  `InvalidOperationException` у `Issue`/`Return`.
- `Loan.Id`, `Loan.BookCopyId` та `Loan.ReaderId` обов'язкові — `ArgumentException`
  у `Loan.Open`.
- Дата повернення не може бути раніше дати видачі — `ArgumentOutOfRangeException`
  у `Loan.Close`.
- Читач не може мати більше п'яти відкритих видач — `InvalidOperationException`
  у `LibraryLendingService.Issue`.
- Підтвердити порожнє замовлення не можна; після `Confirmed` рядки не додаються —
  `InvalidOperationException` у `Order.Confirm`/`Order.AddLine`.
- Ціна не може бути від'ємною, а кількість у рядку має бути більшою за нуль —
  `ArgumentOutOfRangeException` у `Order.AddLine`.

### 4.2. Установити початковий стан

```bash
cd /Users/marta/Desktop/cross/lab_1
pwd
dotnet --version
dotnet --list-sdks
dotnet restore
```

### 4.3. Зібрати лабораторну під усі target framework

```bash
dotnet clean /Users/marta/Desktop/cross/lab_1/CrossApp.slnx
dotnet build /Users/marta/Desktop/cross/lab_1/CrossApp.slnx
dotnet build /Users/marta/Desktop/cross/lab_1/src/Core/Core.csproj -f net8.0
dotnet build /Users/marta/Desktop/cross/lab_1/src/Core/Core.csproj -f net10.0
dotnet build /Users/marta/Desktop/cross/lab_1/src/Cli/Cli.csproj -f net8.0
```

### 4.4. Запустити демонстрацію лабораторної

```bash
dotnet run --project /Users/marta/Desktop/cross/lab_1/src/Cli/Cli.csproj -- --lab4
```

У демонстрації показано успішну видачу й повернення, `ToDto`/`FromDto`,
конвертацію `ImportResult<BookDto>` у `ImportResult<BookCopy>`, порожній ISBN,
повторне повернення, неправильну дату, ліміт п'яти видач і заборону зміни
підтвердженого замовлення.

### 4.5. Запустити всі наявні CLI-сценарії після лабораторної

Це додаткові команди для перевірки всіх режимів CLI після виконання лабораторної.

Ці команди можна запускати навіть із `/Users/marta/Desktop/cross`:

```bash
dotnet run --project /Users/marta/Desktop/cross/lab_1/src/Cli/Cli.csproj
dotnet run --project /Users/marta/Desktop/cross/lab_1/src/Cli/Cli.csproj -- --lab4
dotnet run --project /Users/marta/Desktop/cross/lab_1/src/Cli/Cli.csproj -- --json
dotnet run --project /Users/marta/Desktop/cross/lab_1/src/Cli/Cli.csproj -- /Users/marta/Desktop/cross/lab_1/data/sample.csv
dotnet run --project /Users/marta/Desktop/cross/lab_1/src/Cli/Cli.csproj -- /Users/marta/Desktop/cross/lab_1/data/sample.json
dotnet run --project /Users/marta/Desktop/cross/lab_1/src/Cli/Cli.csproj -- --mixed /Users/marta/Desktop/cross/lab_1/data/sample-mixed.csv
```

### 4.6. Запустити зібраний CLI напряму

```bash
/Users/marta/Desktop/cross/lab_1/src/Cli/bin/Debug/net8.0/Cli --lab4
/Users/marta/Desktop/cross/lab_1/src/Cli/bin/Debug/net8.0/Cli /Users/marta/Desktop/cross/lab_1/data/sample.csv
/Users/marta/Desktop/cross/lab_1/src/Cli/bin/Debug/net8.0/Cli /Users/marta/Desktop/cross/lab_1/data/sample.json
/Users/marta/Desktop/cross/lab_1/src/Cli/bin/Debug/net8.0/Cli --mixed /Users/marta/Desktop/cross/lab_1/data/sample-mixed.csv
```

### 4.7. Перевірити інкапсуляцію та межі домену

Додаткова перевірка структури домену:

```bash
grep -REn "public .*\\{ get; set; \\}|public List<|Console\\.|File\\." /Users/marta/Desktop/cross/lab_1/src/Core/Domain || true
grep -REn "private .*\\(|private .* _|IReadOnlyList|FromDto|ToDto" /Users/marta/Desktop/cross/lab_1/src/Core/Domain /Users/marta/Desktop/cross/lab_1/src/Core/Import
find /Users/marta/Desktop/cross/lab_1/src/Core/Domain -maxdepth 1 -type f -print | sort
```

Перший пошук не повинен знайти `Console.`, `File.` або публічні сетери критичного
стану в `src/Core/Domain`. Другий показує фабрики, мапінг і захищену колекцію.

### 4.8. Перевірити, що CLI повертає успішний код

Додаткова поведінкова перевірка:

```bash
dotnet run --project /Users/marta/Desktop/cross/lab_1/src/Cli/Cli.csproj -- --lab4 >/tmp/lab04-output.txt
echo $?
cat /tmp/lab04-output.txt
grep -En "InvalidOperationException|ArgumentException|ArgumentOutOfRangeException|Фінальний стан" /tmp/lab04-output.txt
rm -f /tmp/lab04-output.txt
```

### 4.9. Запустити лабораторну в Docker SDK 8

Додаткова Docker-перевірка:

```bash
docker run --rm \
  -v "/Users/marta/Desktop/cross/lab_1:/src" \
  -w /src \
  mcr.microsoft.com/dotnet/sdk:8.0 \
  dotnet run \
  --project src/Cli/Cli.csproj \
  -p:TargetFrameworks=net8.0 \
  -- --lab4
```

### 4.10. Перевірити Docker-збірку

Додаткова Docker-збірка:

```bash
docker run --rm \
  -v "/Users/marta/Desktop/cross/lab_1:/src" \
  -w /src \
  mcr.microsoft.com/dotnet/sdk:8.0 \
  dotnet build src/Core/Core.csproj \
  -p:TargetFrameworks=net8.0
```

### 4.11. Опублікувати CLI для macOS Apple Silicon

Додаткова публікація:

```bash
rm -rf /Users/marta/Desktop/cross/lab_1/publish/lab04-osx-arm64
dotnet publish /Users/marta/Desktop/cross/lab_1/src/Cli/Cli.csproj \
  -c Release \
  -f net8.0 \
  -r osx-arm64 \
  --self-contained true \
  -o /Users/marta/Desktop/cross/lab_1/publish/lab04-osx-arm64
/Users/marta/Desktop/cross/lab_1/publish/lab04-osx-arm64/Cli --lab4
du -sh /Users/marta/Desktop/cross/lab_1/publish/lab04-osx-arm64
```

### 4.12. Перевірити зміни та очистити результати

Додаткові команди перевірки й очищення:

```bash
git -C /Users/marta/Desktop/cross/lab_1 diff --check
git -C /Users/marta/Desktop/cross/lab_1 diff --stat
git -C /Users/marta/Desktop/cross/lab_1 status --short
rm -rf /Users/marta/Desktop/cross/lab_1/publish/lab04-osx-arm64
dotnet clean /Users/marta/Desktop/cross/lab_1/CrossApp.slnx
```

### 4.13. Підготувати коміт лабораторної

Перевірити staged-файли перед комітом; автоматично додавати сторонні зміни з
worktree не потрібно.

```bash
git -C /Users/marta/Desktop/cross/lab_1 status --short
git -C /Users/marta/Desktop/cross/lab_1 diff -- src/Core/Domain src/Core/Dto/DomainDtos.cs src/Core/Import/DomainImportMapper.cs src/Cli/Program.cs README.md
git -C /Users/marta/Desktop/cross/lab_1 add README.md src/Core/Domain src/Core/Dto/DomainDtos.cs src/Core/Import/DomainImportMapper.cs src/Cli/Program.cs
git -C /Users/marta/Desktop/cross/lab_1 diff --cached --check
git -C /Users/marta/Desktop/cross/lab_1 diff --cached --stat
git -C /Users/marta/Desktop/cross/lab_1 commit -m "lab04: add domain model invariants"
git -C /Users/marta/Desktop/cross/lab_1 status --short
```
