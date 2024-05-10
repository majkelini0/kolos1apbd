namespace kolos1s27799.Repositories;

public class BooksRepository : IBooksRepository
{
    private readonly IConfiguration _configuration;

    public BooksRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }
}