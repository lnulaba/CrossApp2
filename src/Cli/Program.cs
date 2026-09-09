using System.Runtime.InteropServices;
using System.Text.Json;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var environmentInfo = new
{
    Title = "CrossApp – практикум з крос-платформного програмування",
    Student = "Марта, група ФЕІ-34",
    OsDescription = RuntimeInformation.OSDescription,
    EnvironmentOs = Environment.OSVersion.ToString(),
    ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
    ClrVersion = Environment.Version.ToString(),
    Runtime = RuntimeInformation.FrameworkDescription,
    ApplicationDirectory = AppContext.BaseDirectory,
    CurrentDirectory = Environment.CurrentDirectory,
    Domain = "Бібліотека (книги, примірники, читачі, видачі)"
};

if (args.Contains("--json", StringComparer.OrdinalIgnoreCase))
{
    Console.WriteLine(JsonSerializer.Serialize(environmentInfo));
}
else
{
    Console.WriteLine(environmentInfo.Title);
    Console.WriteLine($"Студент: {environmentInfo.Student}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"ОС (OSDescription) : {environmentInfo.OsDescription}");
    Console.WriteLine($"ОС (Environment) : {environmentInfo.EnvironmentOs}");
    Console.WriteLine($"Архітектура процесу : {environmentInfo.ProcessArchitecture}");
    Console.WriteLine($"Версія .NET (CLR) : {environmentInfo.ClrVersion}");
    Console.WriteLine($"Runtime : {environmentInfo.Runtime}");
    Console.WriteLine($"Каталог застосунку : {environmentInfo.ApplicationDirectory}");
    Console.WriteLine($"Поточний каталог : {environmentInfo.CurrentDirectory}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"Предметна область: {environmentInfo.Domain}");
}
