# CrossApp

Навчальний solution для лабораторних робіт 2–6 на .NET. Усі команди виконуються
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

Реалізовано бібліотеку `Core`, CLI `Cli`, залежність `Cli -> Core`,
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

Без аргументів використовується `data/sample.csv`.

## Лабораторна 4: доменна модель

Додано сутності `BookCopy`, `Loan`, `Order`, сервіс видачі книг, DTO-мапінг та
інваріанти домену.

```bash
dotnet run --project src/Cli/Cli.csproj -- --lab4
dotnet build CrossApp.slnx
```

## Лабораторна 5: сервісний шар і ручний DI

Додано контракт `ILibraryStore`, in-memory і файлове сховища, `LendingService`,
ручний DI, фабрику сховищ, кеш-декоратор і пошук через `Func`.

Схема залежностей: `Cli -> LendingService -> ILibraryStore -> InMemoryLibraryStore | FileLibraryStore`.
`Microsoft.Extensions.DependencyInjection` не використовується.

### Команди для захисту

```bash
cd /Users/marta/Desktop/cross/lab_1
dotnet restore
dotnet build CrossApp.slnx
```

#### 1. Інтерфейс, сервіс і composition root

- `src/Core/Abstractions/ILibraryStore.cs` — інтерфейс сховища та методи
  `ListCopies`, `GetCopyById`, `ListLoans`, `GetLoanById`, `AddCopy`,
  `UpdateCopy`, `AddLoan`, `UpdateLoan`.
- `src/Core/Services/LendingService.cs` — конструктор сервісу з параметром
  типу `ILibraryStore`: `LendingService(ILibraryStore store)`.
- `src/Cli/StoreFactory.cs` — composition root і тернарний вибір:
  `--file` вибирає `FileLibraryStore`, інакше `InMemoryLibraryStore`.
- `src/Cli/Program.cs` — виклик `StoreFactory.Create(args)` і передача сховища
  в `LendingService`.

#### 2. Запуск без `--file`

```bash
rm -f data/catalog.json
dotnet run --project src/Cli/Cli.csproj -- --lab5
dotnet run --project srcconst string searchText = "201";

IReadOnlyList<BookCopy> search = service.Search(item =>
    item.Isbn.Contains(searchText, StringComparison.OrdinalIgnoreCase));

Console.WriteLine($"Пошук: ISBN містить \"{searchText}\"; знайдено {search.Count}");

foreach (BookCopy match in search)
{
    Console.WriteLine($" знайдено: {match}");
}const string searchText = "201";

IReadOnlyList<BookCopy> search = service.Search(item =>
    item.Isbn.Contains(searchText, StringComparison.OrdinalIgnoreCase));

Console.WriteLine($"Пошук: ISBN містить \"{searchText}\"; знайдено {search.Count}");

foreach (BookCopy match in search)
{
    Console.WriteLine($" знайдено: {match}");
}/Cli/Cli.csproj -- --lab5
```

Очікувано: `InMemoryLibraryStore`; дані між запусками не зберігаються.

#### 3. Запуск із `--file`

```bash
rm -f data/catalog.json
dotnet run --project src/Cli/Cli.csproj -- --lab5 --file
cat data/catalog.json
dotnet run --project src/Cli/Cli.csproj -- --lab5 --file
```

Очікувано: `FileLibraryStore`, список із 4 примірників і рядок
`Всього примірників: 4`. Під час кожного наступного запуску створюється новий
вільний ID (`C-202`, `C-203`...), тому кількість примірників збільшується.
`data/catalog.json` зберігає всі записи.

#### 4. Сценарій відмови

```bash
dotnet run --project src/Cli/Cli.csproj -- --lab5
```

Виводяться зрозумілі помилки для дубля `C-201` і неіснуючого примірника
`missing`.

### Додаткові завдання

1. **Кеш-декоратор:** [CachingLibraryStore.cs](src/Core/Storage/CachingLibraryStore.cs)
   приймає `ILibraryStore` і додає кешування.

   ```bash
   dotnet run --project src/Cli/Cli.csproj -- --lab5 --cache
   ```

2. **Пошук через `Func`:** `LendingService.Search(Func<BookCopy, bool>)` приймає
   правило пошуку ззовні. CLI виводить і умову пошуку, і знайдені примірники.

   ```bash
   grep -n -A15 'Search' src/Core/Services/LendingService.cs
   grep -n -A8 'searchText' src/Cli/Program.cs
   dotnet run --project src/Cli/Cli.csproj -- --lab5
   ```

   Очікуваний вивід:

   ```text
   Пошук: ISBN містить "201"; знайдено 1
    знайдено: C-201 [978-617-201] — доступний
   ```

3. **Фабрика:** `StoreFactory.Create(args)` централізує вибір сховища замість
   розміщення всіх `new` у `Program.cs`.

   ```bash
   sed -n '1,80p' src/Cli/StoreFactory.cs
   dotnet run --project src/Cli/Cli.csproj -- --lab5
   dotnet run --project src/Cli/Cli.csproj -- --lab5 --file
   ```

## Лабораторна 6: доменні події, делегати та лямбди

У `Core.Events` оголошено `CatalogChangeKind` і `CatalogChangedEventArgs`, а
`LendingService.Changed` має тип `EventHandler<CatalogChangedEventArgs>`. Подія
піднімається тільки після успішного додавання, видачі або повернення примірника;
`Core` не залежить від `Console` чи файлового логування.

Запуск демонстрації:

```bash
dotnet run --project src/Cli/Cli.csproj -- --lab6
```

CLI підписує локальний обробник для консолі та `logs/app.log`, додає другий
підписник-лямбду із замиканням над лічильником, а потім відписує перший через
`-=`. Після відписки повернення змінює дані, але не записується в лог; лямбда
лічильника продовжує отримувати події. Невдала операція не створює подію.

Метод `FindAll(Func<BookCopy, bool>)` використовує `Func`, бо повертає значення
пошуку, тоді як подія лише повідомляє зовнішній код про вже виконану зміну.

Поточні підписники — CLI та лічильник; у наступних роботах такими підписниками
можуть стати API-логер і Blazor-компонент. Подія зменшує зв'язність: зміну
публікує `Core`, а формат реакції визначає зовнішній код.

### Захист лабораторної 6

#### 1. Оголошення події в `Core`

Подія оголошена в `LendingService`, тому що саме сервіс координує успішну зміну
стану сховища. Зовнішній код не повинен знати, коли саме викликаються
`AddCopy`, `IssueCopy` або `ReturnCopy`, і не повинен самостійно визначати,
чи завершилася операція успішно.

```csharp
public event EventHandler<CatalogChangedEventArgs>? Changed;

private void OnChanged(BookCopy copy, CatalogChangeKind kind, Loan? loan = null) =>
   Changed?.Invoke(this, new CatalogChangedEventArgs(
      copy.Id, kind, loan?.Id, loan?.ReaderId, copy.IsIssued));
```

`Changed?.Invoke` знаходиться в приватному методі сервісу, а не в CLI, тому
тільки клас-видавець вирішує, коли подія відбулася. Модифікатор `event` дозволяє
зовнішньому коду лише `+=` і `-=`, але не дозволяє сторонньому коду очистити
всіх підписників або викликати подію вручну. Оператор `?.` потрібен для випадку,
коли підписників немає: тоді `Changed` дорівнює `null`, і виклик просто не
виконується.

Подія піднімається після зміни стану:

```csharp
_store.UpdateCopy(copy);
_store.AddLoan(loan);
OnChanged(copy, CatalogChangeKind.CopyIssued, loan);
```

Якщо до `OnChanged` виникає виняток, наприклад примірник уже виданий або
читача перевищено ліміт видач, рядок `OnChanged` не виконується. Тому зовнішній
код не отримує повідомлення про операцію, якої фактично не сталося.

#### 2. Підписники в `Cli`

Перший підписник оголошений локальним методом. Це дає йому ім'я, необхідне для
подальшої відписки:

```csharp
service.Changed += OnCatalogChanged;
service.Changed += (_, _) => changes++;
```

`OnCatalogChanged` формує один рядок, друкує його в консоль і додає до файлу.
Другий підписник є лямбда-виразом. Він захоплює локальну змінну `changes`,
тобто утворює замикання, і збільшує лічильник при кожній події. Підписники
викликаються в порядку підписки: спочатку логер, потім лямбда-лічильник.

#### 3. Запуск і лог

Команда демонстрації:

```bash
dotnet run --project src/Cli/Cli.csproj -- --lab6
```

Приклад виводу подій у консолі:

```text
22:08:08 BookAdded    L6-dd6c07f - issued=False
22:08:08 CopyIssued   L6-dd6c07f reader-006 issued=True
Подій отримано: 2
```

Обробник створює шлях через `Path.Combine`, створює каталог через
`Directory.CreateDirectory`, а потім дописує рядки у `app.log`. У поточному
запуску файл знаходиться за шляхом, надрукованим програмою:

```text
src/Cli/bin/Debug/net8.0/logs/app.log
```

Його можна відкрити командою:

```bash
cat src/Cli/bin/Debug/net8.0/logs/app.log
```

У ньому залишаються перші два рядки подій. Повернення примірника після `-=` до
цього файлу не потрапляє.

#### 4. Що демонструє `-=`

Після двох операцій лічильник має значення `2`. Далі CLI виконує:

```csharp
service.Changed -= OnCatalogChanged;
service.ReturnCopy(loan.Id, DateTime.Today);
Console.WriteLine($"Після -= подій отримано: {changes}");
```

Операція повернення успішно змінює стан сховища, але обробник
`OnCatalogChanged` більше не викликається, тому немає рядка в консолі й файлі.
Лямбда-лічильник не відписувався, тому він все ще отримує подію. Результат:

```text
Після -= подій отримано: 3
```

Це показує, що `-=` видаляє конкретний обробник, а не вимикає саму подію і не
видаляє інших підписників.

#### 5. Сценарій відмови

CLI намагається видати неіснуючий примірник:

```csharp
TryDo("видача неіснуючого примірника", () =>
   service.IssueCopy("missing", "L6-missing", "reader-006", DateTime.Today));
```

Сервіс кидає `InvalidOperationException`, а `OnChanged` не викликається,
оскільки до нього виконання не доходить:

```text
видача неіснуючого примірника: InvalidOperationException — Примірник missing не знайдено.
Після відмови подій отримано: 3
```

Отже, лічильник не збільшився, у консолі немає рядка події, а в `app.log` не
з'явився запис про невдалу видачу. Це важливо: подія описує факт успішної зміни,
а не спробу виконати операцію.

#### 6. `FindAll` і чому тут `Func`, а не подія

Пошук викликається так:

```csharp
IReadOnlyList<BookCopy> available = service.FindAll(copy => !copy.IsIssued);
```

Сигнатура методу приймає `Func<BookCopy, bool>`:

```csharp
public IReadOnlyList<BookCopy> FindAll(Func<BookCopy, bool> predicate) =>
   _store.ListCopies().Where(predicate).ToList();
```

`Func<BookCopy, bool>` є правилом, яке для кожного примірника повертає
`true` або `false`. У цьому прикладі лямбда `copy => !copy.IsIssued` вибирає
доступні примірники, але викликаючий код міг би передати інше правило, наприклад
`copy => copy.Isbn.Contains("201")`.

Тут потрібен саме `Func`, бо метод має синхронно повернути результат пошуку —
список примірників. Подія для цього не підходить: подія повідомляє, що щось
сталося, і може мати багато підписників, але не призначена для повернення
значення викликаючому коду. Тому правило пошуку передається через `Func`, а
результат повертається через `IReadOnlyList<BookCopy>`. Для повідомлення про
додану, видану або повернуту книгу використовується `event`, а для отримання
результату за критерієм — метод із `Func`.
### Опційні завдання на вищий бал

#### `INotificationSink`, `ConsoleSink` і `FileSink`

Окрім події, у проєкті реалізовано варіант ручного DI через контракт
`INotificationSink` у `Core.Abstractions`. Сервіс не знає, куди буде доставлено
повідомлення: він отримує набір sink-ів через конструктор:

```csharp
public LendingService(
      ILibraryStore store,
      IEnumerable<INotificationSink>? notificationSinks = null)
```

У `Cli` створюються конкретні реалізації та передаються сервісу:

```csharp
INotificationSink[] sinks = [new ConsoleSink(), new FileSink(logPath)];
var service = new LendingService(store, sinks);
```

`ConsoleSink` пише повідомлення в консоль, а `FileSink` — у файл. `Core`
залежить лише від маленького інтерфейсу, але не залежить від `System.Console`
або конкретного способу роботи з файлами.

Порівняння з подією:

- подія сповіщає про зміну, а підписники підключаються через `+=` і можуть
   відписатися через `-=`, навіть після створення сервісу;
- `INotificationSink` є явною залежністю конструктора: склад sink-ів задається
   під час створення сервісу, його легко побачити в composition root і замінити
   тестовою реалізацією;
- подія краще підходить для багатьох незалежних одержувачів і майбутніх
   компонентів API/Blazor, а sink зручний для стабільного конвеєра логування,
   яким сервіс володіє через DI.

Демонстрація sink-ів запускається так:

```bash
dotnet run --project src/Cli/Cli.csproj -- --lab6-sinks
```

#### `Action<string>` для звіту імпорту

`BookCsvImporter.Load` приймає необов'язковий callback:

```csharp
public static ImportResult<BookDto> Load(
      string path,
      Action<string>? reportError = null)
```

Після виявлення помилки імпортер зберігає її в `ImportResult.Errors` і додатково
викликає callback:

```csharp
reportError?.Invoke(message);
```

CLI може одразу показувати пропущені рядки, не змушуючи імпортер залежати від
консолі:

```csharp
ImportResult<BookDto> import = BookCsvImporter.Load(
      path,
      error => Console.WriteLine($" ! {error}"));
```

`Action<string>` підходить, бо callback отримує повідомлення і нічого не
повертає. Сам результат імпорту все одно повертається як `ImportResult`, тому
callback є додатковим каналом звіту, а не заміною результату.

#### Чому async-обробник події має тип `async void`

Стандартний `EventHandler<T>` повертає `void`. Тому обробник події, який містить
`await`, за звичайним підходом має сигнатуру `async void`:

```csharp
private async void OnChangedAsync(object? sender, CatalogChangedEventArgs e)
{
      await WriteToRemoteLoggerAsync(e);
}
```

Це небезпечно: видавець не може дочекатися завершення такого обробника і не
отримує `Task`. Виняток після першого `await` не повертається звичайному
виклику через `Task`, а передається в контекст синхронізації або може завершити
процес. Також складніше тестувати момент завершення обробки та контролювати
порядок паралельних викликів.

Для звичайних асинхронних операцій краще використовувати метод, що повертає
`Task`, наприклад `Task HandleAsync(...)`, і явно очікувати його. Для подій
`async void` допустимий лише через обмеження стандартної сигнатури; такий
обробник повинен мати власний `try/catch`, а довгу роботу краще передавати в
окрему чергу або сервіс із керованим `Task`.

### Питання на захисті

1. **Що таке composition root і де він у проєкті?**

   Composition root — це єдине місце, де створюються конкретні об'єкти та
   з'єднуються їхні залежності. У проєкті це `StoreFactory`: він вибирає
   `FileLibraryStore` або `InMemoryLibraryStore`. У `Program.cs` фабрика
   викликається, а готове сховище передається в `LendingService`.

2. **Чому сервіс не створює сховище сам? Назвіть два наслідки.**

   `LendingService` залежить від `ILibraryStore`, а не від конкретного класу.
   По-перше, можна замінити JSON-сховище на in-memory або базу даних без зміни
   сервісу. По-друге, сервіс можна тестувати без диска, передавши тестове
   сховище. Якщо створювати `FileLibraryStore` всередині сервісу, обидві
   можливості будуть ускладнені.

3. **Яка різниця між Dependency Inversion і Dependency Injection?**

   Dependency Inversion — це принцип проєктування: високорівневий сервіс не
   залежить від деталей зберігання, а обидві частини залежать від абстракції
   `ILibraryStore`. Dependency Injection — спосіб передати залежність ззовні.
   У проєкті DI виконано через конструктор:
   `LendingService(ILibraryStore store)`.

4. **Чому `List()` повертає `IReadOnlyList`, а не `List`?**

   У нашому домені аналогом `List()` є `ListCopies()`. `IReadOnlyList` дозволяє
   читати результат, але не дає зовнішньому коду додавати чи видаляти елементи
   напряму. Зміни проходять тільки через `AddCopy`, `UpdateCopy` та доменні
   правила. Це захищає внутрішній стан сховища.

5. **Що зміниться в `CatalogService`, коли сховищем стане база даних?**

   У цьому проєкті сервіс називається `LendingService`. Його код не повинен
   змінитися: додається нова реалізація, наприклад `EfLibraryStore`, яка також
   реалізує `ILibraryStore`. У `StoreFactory` змінюється лише вибір реалізації
   та налаштування підключення до бази даних.

6. **Навіщо DTO між файлом і сутністю, якщо можна серіалізувати сутність?**

   DTO відокремлює формат збереження від доменної моделі. `BookCopy` має
   приватний стан і методи-інваріанти `Issue()` та `Return()`, тому JSON не
   повинен напряму керувати цим станом. `BookCopyDto` і `LoanDto` зберігають
   прості дані, а `ToDto()`/`FromDto()` виконують безпечне перетворення. Так
   можна змінити формат JSON, не змінюючи правила домену.

7. **Скільки методів має бути в інтерфейсі і чому не «більше про запас»?**

   Інтерфейс має містити мінімальний набір операцій, потрібний сервісу. У
   `ILibraryStore` це читання, додавання й оновлення примірників та видач.
   Методи «про запас» збільшують контракт і змушують кожну нову реалізацію
   підтримувати непотрібні операції. Це порушує принцип розділення інтерфейсів.

8. **Як перевірити сервіс тестом, не торкаючись диска?**

   Створити `InMemoryLibraryStore`, передати його в `LendingService`, виконати
   `AddCopy`, `IssueCopy`, `ReturnCopy` і перевірити список та стан примірника.
   Файлова система при цьому не використовується:

   ```csharp
   var store = new InMemoryLibraryStore();
   var service = new LendingService(store);
   BookCopy copy = service.AddCopy("C-TEST", "978-617-TEST");
   ```

9. **Чому `FileLibraryStore` кешує дані, а не читає файл на кожен виклик?**

   При першому зверненні файл завантажується у словники `_copies` і `_loans`.
   Наступні `List` та `Get` працюють у пам'яті, тому немає зайвого дискового
   читання. Після `Add` або `Update` кеш змінюється і стан записується назад у
   JSON. Це швидше та зберігає узгоджений стан протягом роботи програми.

10. **Чи потрібен контейнер DI у консольному застосунку?**

   Для цього невеликого CLI — ні. Достатньо ручного DI: `StoreFactory` створює
   сховище, а `Program.cs` передає його у `LendingService`. Контейнер потрібен,
   коли залежностей багато, треба керувати їхнім життєвим циклом або є кілька
   застосунків із великою конфігурацією. У цій лабораторній контейнер навмисно
   не підключається, щоб продемонструвати принцип вручну.

## Фінальна перевірка

```bash
dotnet sln list CrossApp.slnx
git status --short
```
