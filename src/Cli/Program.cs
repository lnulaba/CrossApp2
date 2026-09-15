using System.Text.Json;
using Core;

Console.OutputEncoding = System.Text.Encoding.UTF8;

EnvironmentReport report = EnvironmentInfo.Collect();

if (args.Contains("--json", StringComparer.OrdinalIgnoreCase))
{
    Console.WriteLine(JsonSerializer.Serialize(report));
}
else
{
    Console.WriteLine(report.Title);
    Console.WriteLine($"Студент: {report.Student}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"ОС (OSDescription) : {report.OsDescription}");
    Console.WriteLine($"ОС (Environment) : {report.EnvironmentOs}");
    Console.WriteLine($"Архітектура процесу : {report.ProcessArchitecture}");
    Console.WriteLine($"Версія .NET (CLR) : {report.ClrVersion}");
    Console.WriteLine($"Runtime : {report.FrameworkDescription}");
    Console.WriteLine($"RID (визначено) : {report.DetectedRid}");
    Console.WriteLine($"RID (від .NET) : {report.ReportedRid}");
    Console.WriteLine($"Каталог застосунку : {report.ApplicationDirectory}");
    Console.WriteLine($"Поточний каталог : {report.CurrentDirectory}");
    Console.WriteLine($"Build note : {report.BuildNote}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"Предметна область: {report.Domain}");
}
