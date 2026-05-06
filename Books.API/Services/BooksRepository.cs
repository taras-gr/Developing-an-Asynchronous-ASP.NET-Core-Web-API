using Books.API.DbContexts;
using Books.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Books.API.Services;

public class BooksRepository(BooksContext context) : IBooksRepository
{
    private readonly BooksContext _context = context
            ?? throw new ArgumentNullException(nameof(context));

    public async Task<Book?> GetBookAsync(Guid id)
    {
        return await _context.Books
            .Include(b => b.Author)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public void AddBook(Book bookToAdd)
    {
        ArgumentNullException.ThrowIfNull(bookToAdd);

        _context.Books.Add(bookToAdd);
    }

    public IEnumerable<Book> GetBooks()
    {
        return _context.Books
            .Include(b => b.Author)
            .ToList();
    }

    public async Task<IEnumerable<Book>> GetBooksAsync()
    {
        return await _context.Books
            .Include(b => b.Author)
            .ToListAsync();
    }

    public IAsyncEnumerable<Book> GetBooksAsAsyncEnumerable()
    {
        return _context.Books.AsAsyncEnumerable();
    }

    public async Task<IEnumerable<Book>> GetBooksAsync(IEnumerable<Guid> bookIds)
    {
        return await _context.Books
            .Where(b => bookIds.Contains(b.Id))
            .Include(b => b.Author)
            .ToListAsync();
    }

    public async Task<bool> SaveChangesAsync()
    {
        return (await _context.SaveChangesAsync() > 0);
    }
}
