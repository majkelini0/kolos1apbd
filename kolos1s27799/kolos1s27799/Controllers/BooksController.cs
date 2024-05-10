using kolos1s27799.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace kolos1s27799.Controllers;

[ApiController]
[Route("api/books/")]
public class BooksController
{
    private readonly IBooksRepository _booksRepository;
    
    public BooksController(IBooksRepository booksRepository)
    {
        _booksRepository = booksRepository;
    }
}