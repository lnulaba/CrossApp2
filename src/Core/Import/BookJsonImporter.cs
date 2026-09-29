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
            List<BookDto> items = JsonSerializer.Deserialize(
                json,
                BookJsonContext.Default.ListBookDto) ?? [];
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