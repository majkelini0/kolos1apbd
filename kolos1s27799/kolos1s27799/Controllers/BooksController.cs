using System.Net;
using kolos1s27799.Models.DTOs;
using kolos1s27799.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace kolos1s27799.Controllers;

[ApiController]
[Route("api/books/")]
public class BooksController : ControllerBase
{
    private readonly IBooksRepository _booksRepository;
    
    public BooksController(IBooksRepository booksRepository)
    {
        _booksRepository = booksRepository;
    }
    
    [HttpGet]
    [Route("{id:int}/authors")]
    public async Task<IActionResult> GetBooks(int id)
    {
        if (!await _booksRepository.DoesBookExist(id))
        {
            return NotFound("Book doesn't exist");
        }
        
        BookAuthorsWithId book = await _booksRepository.GetBookWithAuthors(id);
        if (book is null)
        {
            return StatusCode((int)HttpStatusCode.NoContent, "Something went wrong. No content was returned");
        }
        return Ok(book);
    }

    [HttpPost]
    public async Task<IActionResult> AddBook([FromBody] BookAuthors bookAuthors)
    {
        if (await _booksRepository.DoesBookExist(bookAuthors.title))
        {
            return StatusCode((int)HttpStatusCode.Conflict, "Book with this title already exists");
        }

        BookAuthorsWithId bAWI = await _booksRepository.AddBook(bookAuthors);
        if (bAWI is null)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, "An error occurred while adding the book and its authors");
        }
        
        // 0. open transaction
        // 1. Add book
        // 2. Add authors to the book
        // 3. Update books_authors table
        // 4. commit transaction

        return Ok(bAWI);
    }
}