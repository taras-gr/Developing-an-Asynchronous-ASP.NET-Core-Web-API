using AutoMapper;
using Books.API.Entities;
using Books.API.Filters;
using Books.API.Models;
using Books.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Books.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BooksController(IBooksRepository booksRepository, IMapper mapper) : ControllerBase
{
    private readonly IBooksRepository _booksRepository = booksRepository;
    private readonly IMapper _mapper = mapper;

    [HttpGet]
    [TypeFilter(typeof(BooksResultFilter))]
    public async Task<IActionResult> GetBooks()
    {
        var books = await _booksRepository.GetBooksAsync();
        return Ok(books);
    }

    [HttpGet("{id}", Name = nameof(GetBook))]
    [TypeFilter(typeof(BookResultFilter))]
    public async Task<IActionResult> GetBook(Guid id)
    {
        var book = await _booksRepository.GetBookAsync(id);
        if (book == null)
        {
            return NotFound();
        }
        return Ok(book);
    }

    [HttpPost]
    [TypeFilter(typeof(BookResultFilter))]
    public async Task<IActionResult> CreateBook(
        [FromBody] BookForCreationDto bookForCreation)
    {
        var bookEntity = _mapper.Map<Book>(bookForCreation);
        _booksRepository.AddBook(bookEntity);
        await _booksRepository.SaveChangesAsync();

        await _booksRepository.GetBookAsync(bookEntity.Id);

        return CreatedAtRoute(nameof(GetBook),
            new { id = bookEntity.Id },
            bookEntity);
    }
}
