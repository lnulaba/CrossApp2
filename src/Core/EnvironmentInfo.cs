using System.Runtime.InteropServices;

namespace Core;

public sealed record EnvironmentReport(
    string Title,
    string Student,
    string OsDescription,
    string EnvironmentOs,
    string ProcessArchitecture,
    string ClrVersion,
    string FrameworkDescription,
    string DetectedRid,
    string ReportedRid,
    string ApplicationDirectory,
    string CurrentDirectory,
    string Domain,
    string BuildNote);

public static class EnvironmentInfo
{
    public static EnvironmentReport Collect() => new(
        "CrossApp – практикум з крос-платформного програмування",
        "Марта, група ФЕІ-34",
        RuntimeInformation.OSDescription,
        Environment.OSVersion.ToString(),
        RuntimeInformation.ProcessArchitecture.ToString(),
        Environment.Version.ToString(),
        RuntimeInformation.FrameworkDescription,
        DetectRid(),
        RuntimeInformation.RuntimeIdentifier,
        AppContext.BaseDirectory,
        Environment.CurrentDirectory,
        "Бібліотека (книги, примірники, читачі, видачі)",
        BuildNote);

#if NET10_0_OR_GREATER
    private const string BuildNote = "збірка під net10.0";
#else
    private const string BuildNote = "збірка під net8.0";
#endif

    private static string DetectRid()
    {
        string os =
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win" :
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "linux" :
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "osx" : "unknown";

        string arch = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            Architecture.Arm64 => "arm64",
            Architecture.Arm => "arm",
            _ => "unknown"
        };

        return $"{os}-{arch}";
    }
}