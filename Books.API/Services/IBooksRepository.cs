using Books.API.Entities;

namespace Books.API.Services;

public interface IBooksRepository
{
    IEnumerable<Book> GetBooks();
    //Book? GetBook(Guid id);

    IAsyncEnumerable<Book> GetBooksAsAsyncEnumerable();

    Task<IEnumerable<Book>> GetBooksAsync(IEnumerable<Guid> bookIds);

    Task<IEnumerable<Book>> GetBooksAsync();

    Task<Book?> GetBookAsync(Guid id);

    void AddBook(Book bookToAdd);

    Task<bool> SaveChangesAsync();
}
