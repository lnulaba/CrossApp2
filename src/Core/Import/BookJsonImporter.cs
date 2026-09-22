using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class BookJsonImporter
{
    public static ImportResult<BookDto> Load(string path)
    {
        try
        {
            string json = File.ReadAllText(path, System.Text.Encoding.UTF8);
            JsonSerializerOptions options = new()
            {
                PropertyNameCaseInsensitive = true
            };

            List<BookDto> items = JsonSerializer.Deserialize<List<BookDto>>(json, options) ?? [];
            return new ImportResult<BookDto>(items, []);
        }
        catch (JsonException exception)
        {
            return new ImportResult<BookDto>([], [$"некоректний JSON: {exception.Message}"]);
        }
        catch (IOException exception)
        {
            return new ImportResult<BookDto>([], [$"помилка читання: {exception.Message}"]);
        }
    }
}