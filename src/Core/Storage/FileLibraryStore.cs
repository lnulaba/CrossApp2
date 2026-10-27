using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class FileLibraryStore(string path, IEnumerable<BookCopy>? seed = null) : ILibraryStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private readonly Dictionary<string, BookCopy> _copies = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Loan> _loans = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _path = Path.GetFullPath(path);
    private readonly IReadOnlyList<BookCopy> _seed = seed?.ToList() ?? [];
    private bool _loaded;

    public IReadOnlyList<BookCopy> ListCopies()
    {
        EnsureLoaded();
        return _copies.Values.ToList();
    }

    public BookCopy? GetCopyById(string id)
    {
        EnsureLoaded();
        return _copies.GetValueOrDefault(id);
    }

    public IReadOnlyList<Loan> ListLoans()
    {
        EnsureLoaded();
        return _loans.Values.ToList();
    }

    public Loan? GetLoanById(string id)
    {
        EnsureLoaded();
        return _loans.GetValueOrDefault(id);
    }

    public void AddCopy(BookCopy copy)
    {
        ArgumentNullException.ThrowIfNull(copy);
        EnsureLoaded();
        if (!_copies.TryAdd(copy.Id, copy))
        {
            throw new InvalidOperationException($"Примірник з id={copy.Id} уже існує.");
        }

        Flush();
    }

    public void UpdateCopy(BookCopy copy)
    {
        ArgumentNullException.ThrowIfNull(copy);
        EnsureLoaded();
        _copies[copy.Id] = copy;
        Flush();
    }

    public void AddLoan(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);
        EnsureLoaded();
        if (!_loans.TryAdd(loan.Id, loan))
        {
            throw new InvalidOperationException($"Видача з id={loan.Id} уже існує.");
        }

        Flush();
    }

    public void UpdateLoan(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);
        EnsureLoaded();
        _loans[loan.Id] = loan;
        Flush();
    }

    private void EnsureLoaded()
    {
        if (_loaded)
        {
            return;
        }

        if (File.Exists(_path))
        {
            LibraryStoreDto state = JsonSerializer.Deserialize<LibraryStoreDto>(
                File.ReadAllText(_path), Options) ?? new([], []);
            foreach (BookCopyDto dto in state.Copies)
            {
                BookCopy copy = BookCopy.FromDto(dto);
                _copies[copy.Id] = copy;
            }

            foreach (LoanDto dto in state.Loans)
            {
                Loan loan = Loan.FromDto(dto);
                _loans[loan.Id] = loan;
            }
        }
        if (_copies.Count == 0)
        {
            foreach (BookCopy copy in _seed)
            {
                _copies[copy.Id] = copy;
            }
        }

        _loaded = true;
    }

    private void Flush()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        LibraryStoreDto state = new(
            _copies.Values.Select(copy => copy.ToDto()).ToList(),
            _loans.Values.Select(loan => loan.ToDto()).ToList());
        File.WriteAllText(_path, JsonSerializer.Serialize(state, Options));
    }
}