using AutoMapper;
using Books.API.Entities;
using Books.API.Models;
using Books.API.Models.External;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Books.API.Filters;

public class BookWithCoversResultFilter(IMapper mapper) : IAsyncResultFilter
{
    private readonly IMapper _mapper = mapper;

    public async Task OnResultExecutionAsync(
        ResultExecutingContext context, ResultExecutionDelegate next)
    {
        var resultFromAction = context.Result as ObjectResult;
        if (resultFromAction?.Value == null
            || resultFromAction.StatusCode < 200
            || resultFromAction.StatusCode >= 300)
        {
            await next();
            return;
        }

        var (book, bookCovers) = 
                ((Book book, IEnumerable<Models.External.BookCoverDto> bookCovers))resultFromAction.Value;

        var mappedBook = _mapper.Map<BookWithCoversDto>(book);
        resultFromAction.Value = _mapper.Map(bookCovers, mappedBook);

        await next();
    }
}
