using kolos1s27799.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace kolos1s27799.Repositories;

public class BooksRepository : IBooksRepository
{
    private readonly IConfiguration _configuration;

    public BooksRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<bool> DoesBookExist(int id)
    {
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        
        command.CommandText = "SELECT 1 FROM books WHERE PK = @id";
        command.Parameters.AddWithValue("@id", id);
        
        await connection.OpenAsync();
        var res = await command.ExecuteScalarAsync();
        
        if(res is not null)
        {
            return true;
        }
        
        return false;
    }

    public async Task<BookAuthorsWithId> GetBookWithAuthors(int id)
    {
        var query = "Select b.PK, b.Title, a.first_name, a.last_name from Books b " +
                    "join books_authors ba on b.PK = ba.FK_book " + 
                    "join authors a on a.PK = ba.FK_author " +
                    "WHERE b.PK = @id";
        
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@id", id);
        await connection.OpenAsync();
        
        var reader = await command.ExecuteReaderAsync();
        
        var bookIdOrdinal = reader.GetOrdinal("PK");
        var bookTitleOrdinal = reader.GetOrdinal("Title");
        var bookFirstOrdinal = reader.GetOrdinal("first_name");
        var bookLastOrdinal = reader.GetOrdinal("last_name");
        
        BookAuthorsWithId book = null;
        
        while (await reader.ReadAsync())
        {
            if (book == null)
            {
                book = new BookAuthorsWithId()
                {
                    id  = reader.GetInt32(bookIdOrdinal),
                    title = reader.GetString(bookTitleOrdinal),
                    authors = new List<Author>()
                    {
                        new Author()
                        {
                            firstName = reader.GetString(bookFirstOrdinal),
                            lastName = reader.GetString(bookLastOrdinal)
                        }
                    }
                };
            }
            else
            {
                book.authors.Add(new Author()
                {
                    firstName = reader.GetString(bookFirstOrdinal),
                    lastName = reader.GetString(bookLastOrdinal)
                });
            }
        }

        return book;
    }
}