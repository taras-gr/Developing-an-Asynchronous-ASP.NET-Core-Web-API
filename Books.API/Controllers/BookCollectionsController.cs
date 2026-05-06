using AutoMapper;
using Books.API.Filters;
using Books.API.Helpers;
using Books.API.Models;
using Books.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Books.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[TypeFilter(typeof(BooksResultFilter))]
public class BookCollectionsController(IBooksRepository booksRepository,
    IMapper mapper) : ControllerBase
{
    private readonly IBooksRepository _booksRepository = booksRepository;
    private readonly IMapper _mapper = mapper;

    [HttpPost]
    public async Task<IActionResult> CreateBookCollection(
        [FromBody] IEnumerable<BookForCreationDto> bookCollection)
    {
        var bookEntities = _mapper.Map<IEnumerable<Entities.Book>>(bookCollection);
        foreach (var bookEntity in bookEntities)
        {
            _booksRepository.AddBook(bookEntity);
        }

        await _booksRepository.SaveChangesAsync();

        var booksToReturn = await _booksRepository.GetBooksAsync(
            bookEntities.Select(b => b.Id));
        var bookIds = string.Join(",", booksToReturn.Select(b => b.Id));

        return CreatedAtRoute(nameof(GetBookCollection),
            new { bookIds },
            booksToReturn);
    }

    [HttpGet("{bookIds}", Name = nameof(GetBookCollection))]
    public async Task<IActionResult> GetBookCollection(
        [ModelBinder(BinderType = typeof(ArrayModelBinder))]IEnumerable<Guid> bookIds)
    {
        var bookEntities = await _booksRepository.GetBooksAsync(bookIds);
        
        if (bookIds.Count() != bookEntities.Count())
        {
            return NotFound();
        }

        return Ok(bookEntities);
    }
}