using Books.API.DbContexts;
using Books.API.Entities;
using Books.API.Models.External;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Books.API.Services;

public class BooksRepository(BooksContext context,
    IHttpClientFactory httpClientFactory) : IBooksRepository
{
    private readonly BooksContext _context = context
            ?? throw new ArgumentNullException(nameof(context));
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory
            ?? throw new ArgumentNullException(nameof(httpClientFactory));

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

    public async Task<BookCoverDto?> GetBookCoverAsync(string id)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var response = await httpClient
            .GetAsync($"http://localhost:5146/api/bookcovers/{id}");

        if (response.IsSuccessStatusCode)
        {
            return JsonSerializer.Deserialize<BookCoverDto>(
                await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }

        return null;
    }

    public async Task<IEnumerable<BookCoverDto>> GetBookCoversProcessOneByOneAsync(
        Guid bookId, CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var bookCovers = new List<BookCoverDto>();
        
        var bookCoversUrls = new[]
        {
            $"http://localhost:5146/api/bookcovers/{bookId}-dummycover1",
            $"http://localhost:5146/api/bookcovers/{bookId}-dummycover2?returnFault=true",
            $"http://localhost:5146/api/bookcovers/{bookId}-dummycover3",
            $"http://localhost:5146/api/bookcovers/{bookId}-dummycover4",
            $"http://localhost:5146/api/bookcovers/{bookId}-dummycover5"
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            using (var linkedCancellationTokenSource = 
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationTokenSource.Token, cancellationToken))
            {
                foreach (var bookCoverUrl in bookCoversUrls)
                {
                    var response = await httpClient.GetAsync(bookCoverUrl,
                        linkedCancellationTokenSource.Token);

                    if (response.IsSuccessStatusCode)
                    {
                        var bookCover = JsonSerializer.Deserialize<BookCoverDto>(
                            await response.Content.ReadAsStringAsync(linkedCancellationTokenSource.Token),
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                        if (bookCover != null)
                        {
                            bookCovers.Add(bookCover);
                        }
                    }
                    else
                    {
                        cancellationTokenSource.Cancel();
                    }
                }

            }
        }

        return bookCovers;
    }

    public async Task<IEnumerable<BookCoverDto>> GetBookCoversProcessAfterWaitForAllAsync(Guid bookId)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var bookCovers = new List<BookCoverDto>();

        var bookCoversUrls = new[]
        {
            $"http://localhost:5146/api/bookcovers/{bookId}-dummycover1",
            $"http://localhost:5146/api/bookcovers/{bookId}-dummycover2",
            $"http://localhost:5146/api/bookcovers/{bookId}-dummycover3",
            $"http://localhost:5146/api/bookcovers/{bookId}-dummycover4",
            $"http://localhost:5146/api/bookcovers/{bookId}-dummycover5"
        };

        var bookCoverTasks = new List<Task<HttpResponseMessage>>();
        foreach (var bookCoverUrl in bookCoversUrls)
        {
            bookCoverTasks.Add(httpClient.GetAsync(bookCoverUrl));
        };

        var bookCoverTasksResults = await Task.WhenAll(bookCoverTasks);

        foreach (var bookCoverTaskResult in bookCoverTasksResults.Reverse())
        {
            var bookCover = JsonSerializer.Deserialize<BookCoverDto>(
                    await bookCoverTaskResult.Content.ReadAsStringAsync(),
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (bookCover != null)
            {
                bookCovers.Add(bookCover);
            }
        }

        return bookCovers;
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
