# CrossApp

Навчальний solution для лабораторних робіт 2–4 на .NET. Усі команди виконуються
з кореня репозиторію:

```bash
cd /Users/marta/Desktop/cross/lab_1
```

## Структура

- `src/Core` — бібліотека з доменною моделлю, DTO та імпортерами;
- `src/Cli` — консольний застосунок;
- `data` — тестові CSV/JSON-файли;
- `publish` — результати публікації.

## Лабораторна 2: solution і публікація

Реалізовано class library `Core`, CLI `Cli`, залежність `Cli -> Core`,
multi-targeting `net8.0;net10.0`, умовну компіляцію та публікацію для різних
runtime.

```bash
dotnet --info
dotnet restore
dotnet build CrossApp.slnx
dotnet build src/Core/Core.csproj -f net8.0
dotnet build src/Core/Core.csproj -f net10.0
dotnet run --project src/Cli/Cli.csproj
dotnet run --project src/Cli/Cli.csproj -- --json
```

Приклади публікації:

```bash
dotnet publish src/Cli -c Release -f net8.0 -r osx-arm64 --self-contained false
dotnet publish src/Cli -c Release -f net8.0 -r linux-x64 \
  --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true
```

## Лабораторна 3: імпорт даних

Додано імпорт книг із CSV/JSON, обробку помилок рядків, різнорідний каталог
книг і читачів та статистику імпорту.

```bash
dotnet run --project src/Cli/Cli.csproj -- data/sample.csv
dotnet run --project src/Cli/Cli.csproj -- data/sample.json
dotnet run --project src/Cli/Cli.csproj -- --mixed data/sample-mixed.csv
```

Без аргументів використовується `data/sample.csv`. Підтримуються також
`--json` для звіту про середовище та повідомлення про відсутні/непідтримувані
файли.

## Лабораторна 4: доменна модель

Додано сутності `BookCopy`, `Loan`, `Order`, сервіс видачі книг, DTO-мапінг та
інваріанти: не можна повторно видати або повернути примірник, перевищити ліміт
у п’ять відкритих видач і змінити підтверджене замовлення.

```bash
dotnet run --project src/Cli/Cli.csproj -- --lab4
dotnet build CrossApp.slnx
```

## Лабораторна 5: сервісний шар і ручний DI

Для бібліотечного домену додано:

- `ILibraryStore` — контракт сховища;
- `InMemoryLibraryStore` і `FileLibraryStore` — взаємозамінні реалізації;
- `LendingService` — сервіс, який залежить лише від інтерфейсу;
- `StoreFactory` — вибір реалізації в точці входу;
- `CachingLibraryStore` — додатковий декоратор кешування;
- `Search(Func<BookCopy, bool>)` — додатковий пошук.

Схема залежностей: `Cli -> LendingService -> ILibraryStore -> InMemoryLibraryStore | FileLibraryStore`.
Конкретні сховища створюються тільки у `StoreFactory`; `Microsoft.Extensions.DependencyInjection`
не використовується.

### Команди для захисту

```bash
cd /Users/marta/Desktop/cross/lab_1
dotnet restore
dotnet build CrossApp.slnx
```

Режим пам’яті: дані існують лише під час запуску.

```bash
dotnet run --project src/Cli/Cli.csproj -- --lab5
dotnet run --project src/Cli/Cli.csproj -- --lab5
```

Файловий режим: дані зберігаються і доступні після повторного запуску.

```bash
rm -f data/catalog.json
dotnet run --project src/Cli/Cli.csproj -- --lab5 --file
cat data/catalog.json
dotnet run --project src/Cli/Cli.csproj -- --lab5 --file
```

Додаткові завдання: кеш-декоратор і пошук.

```bash
dotnet run --project src/Cli/Cli.csproj -- --lab5 --cache
```

Перевірка помилок і структури залежностей:

```bash
dotnet run --project src/Cli/Cli.csproj -- --lab5
grep -R "FileLibraryStore\|InMemoryLibraryStore" src/Core/Services src/Core/Abstractions
grep -R "Microsoft.Extensions.DependencyInjection" src || true
git status --short
```

Очікувано сценарій показує `Сховище: InMemoryLibraryStore` або
`Сховище: FileLibraryStore`, список примірників, результат пошуку та зрозумілі
помилки для дубля і неіснуючого примірника.

### Питання на захисті: покрокові відповіді

1. **Що таке composition root і де він у проєкті?**

  Composition root — єдине місце, де створюються конкретні реалізації та
  з'єднуються залежності. У цьому проєкті вибір сховища виконує
  `src/Cli/StoreFactory.cs`, а `Program.cs` запускає сценарій і передає готове
  `ILibraryStore` у `LendingService`.

2. **Чому сервіс не створює сховище сам? Назвіть два наслідки.**

  `LendingService` залежить від `ILibraryStore`, тому не знає про файл або
  словник. Це дає дві переваги: можна замінити спосіб збереження без зміни
  сервісу та протестувати сервіс без файлової системи, передавши in-memory
  сховище.

3. **Різниця між Dependency Inversion і Dependency Injection.**

  Dependency Inversion — принцип: високорівневий сервіс і низькорівнева
  реалізація залежать від абстракції. Dependency Injection — техніка передачі
  конкретної залежності ззовні. Тут принцип реалізовано через `ILibraryStore`,
  а injection — через конструктор `LendingService(ILibraryStore store)`.

4. **Чому `List()` повертає `IReadOnlyList`, а не `List`?**

  Щоб код поза сховищем не міг додавати або видаляти елементи напряму й
  обходити правила `AddCopy`, `UpdateCopy` та доменні інваріанти. У бібліотеці
  аналогом `List()` є `ListCopies()`.

5. **Що зміниться, коли сховищем стане база даних?**

  У `LendingService` нічого або майже нічого: він і далі працюватиме з
  `ILibraryStore`. Потрібно буде додати, наприклад, `EfLibraryStore`, який
  реалізує той самий інтерфейс, і замінити реалізацію у `StoreFactory`.

6. **Навіщо DTO між файлом і сутністю?**

  DTO відокремлює формат JSON від доменної моделі. Приватні поля та інваріанти
  `BookCopy` і `Loan` не залежать від способу серіалізації, а формат файлу можна
  змінити без зміни правил домену. Перетворення виконують `ToDto()` і `FromDto()`.

7. **Скільки методів має бути в інтерфейсі?**

  Рівно стільки, скільки потрібно сервісу для його операцій: у цьому проєкті це
  читання, додавання та оновлення примірників і видач. Методи «про запас» не
  додаються, бо кожна нова реалізація буде змушена їх підтримувати.

8. **Як перевірити сервіс без диска?**

  Створити `InMemoryLibraryStore`, передати його у `LendingService`, виконати
  `AddCopy`, `IssueCopy`, `ReturnCopy` і перевірити результат. У CLI це можна
  побачити командою:

  ```bash
  dotnet run --project src/Cli/Cli.csproj -- --lab5
  ```

9. **Чому `FileLibraryStore` кешує дані?**

  Файл читається один раз під час першого звернення, після чого операції
  працюють зі словниками в пам'яті. Після `Add`, `Update` або повернення дані
  записуються на диск. Це зменшує кількість операцій введення-виведення і
  гарантує, що всі методи використовують узгоджений стан.

10. **Чи потрібен DI-контейнер у консольному застосунку?**

  Ні, для невеликого CLI достатньо ручного DI: `StoreFactory` створює сховище,
  а `Program.cs` передає його сервісу. Контейнер виправданий, коли залежностей
  стає багато, з'являються різні життєві цикли або кілька застосунків. У цій
  лабораторній `Microsoft.Extensions.DependencyInjection` навмисно не додано.

### Додаткові завдання

1. **Декоратор кешування.**

  `CachingLibraryStore` приймає інший `ILibraryStore`, тому має той самий
  контракт, але додає кешування результатів `ListCopies()` і `ListLoans()`.
  Після зміни даних кеш очищається. Демонстрація:

  ```bash
  dotnet run --project src/Cli/Cli.csproj -- --lab5 --cache
  ```

2. **Пошук через предикат.**

  `LendingService.Search(Func<BookCopy, bool> predicate)` приймає правило
  пошуку ззовні. Наприклад, сценарій шукає примірники за частиною ISBN:

  ```csharp
  service.Search(copy => copy.Isbn.Contains("201"));
  ```

  Сервіс не прив'язаний до одного фіксованого фільтра й готовий до складніших
  запитів у наступних лабораторних.

3. **Фабрика сховищ.**

  `StoreFactory.Create(args)` приховує умовний вибір між
  `InMemoryLibraryStore` і `FileLibraryStore`. Порівняно з розміщенням усіх
  `new` у `Program.cs`, фабрика робить точку входу коротшою та зосереджує
  конфігурацію в одному місці. Перевірка вибору:

  ```bash
  dotnet run --project src/Cli/Cli.csproj -- --lab5
  dotnet run --project src/Cli/Cli.csproj -- --lab5 --file
  ```

## Фінальна перевірка

```bash
dotnet sln list CrossApp.slnx
git status --short
```
