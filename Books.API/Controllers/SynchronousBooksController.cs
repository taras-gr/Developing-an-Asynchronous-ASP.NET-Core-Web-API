using Books.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Books.API.Controllers;

[Route("api/books/sync")]
[ApiController]
public class SynchronousBooksController(IBooksRepository booksRepository) : ControllerBase
{
    private readonly IBooksRepository _booksRepository = booksRepository;

    [HttpGet]
    public IActionResult GetBooks()
    {
        var books = _booksRepository.GetBooks();
        return Ok(books);
    }
}
