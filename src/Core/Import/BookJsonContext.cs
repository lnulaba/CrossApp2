using System.Text.Json.Serialization;
using Core.Dto;

namespace Core.Import;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(List<BookDto>))]
internal partial class BookJsonContext : JsonSerializerContext;
