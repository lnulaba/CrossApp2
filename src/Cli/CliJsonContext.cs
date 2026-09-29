using System.Text.Json.Serialization;
using Core;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(EnvironmentReport))]
internal partial class CliJsonContext : JsonSerializerContext;
