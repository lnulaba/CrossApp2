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
            List<BookDto> parsedItems = JsonSerializer.Deserialize(
                json,
                BookJsonContext.Default.ListBookDto) ?? [];

            var items = new List<BookDto>();
            var errors = new List<string>();
            for (int index = 0; index < parsedItems.Count; index++)
            {
                BookDto item = parsedItems[index];
                if (item.Year < 1450 || item.Year > DateTime.Now.Year)
                {
                    errors.Add(
                        $"запис {index + 1}: рік '{item.Year}' поза допустимими межами");
                    continue;
                }

                items.Add(item);
            }

            return new ImportResult<BookDto>(items, errors);
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