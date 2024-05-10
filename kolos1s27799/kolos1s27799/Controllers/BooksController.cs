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
        
        
        
        
        
        return Ok();
    }
}