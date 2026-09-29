using Core.Domain;
using Core.Dto;

namespace Core.Import;

public static class DomainImportMapper
{
    public static ImportResult<BookCopy> ToBookCopies(ImportResult<BookDto> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var items = new List<BookCopy>();
        var errors = source.Errors.ToList();
        for (int index = 0; index < source.Items.Count; index++)
        {
            BookDto dto = source.Items[index];
            try
            {
                items.Add(BookCopy.Create(dto.Id, dto.Isbn));
            }
            catch (ArgumentException exception)
            {
                errors.Add($"імпорт запису {index + 1}: {exception.Message}");
            }
        }

        return new ImportResult<BookCopy>(items, errors);
    }
}
