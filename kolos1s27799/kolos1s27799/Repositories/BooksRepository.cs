using System.Net;
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

    public async Task<bool> DoesBookExist(string title)
    {
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        
        command.CommandText = "SELECT 1 FROM books WHERE title = @title";
        command.Parameters.AddWithValue("@title", title);
        
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

    public async Task<BookAuthorsWithId> AddBook(BookAuthors bookAuthors)
    {
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await connection.OpenAsync();
        SqlTransaction transaction = connection.BeginTransaction();


        BookAuthorsWithId newBook = new BookAuthorsWithId();
        try
        {
            // 1. Adding book
            var query = "INSERT INTO books VALUES (@title); SELECT @@IDENTITY AS ID;";
            using SqlCommand command = new SqlCommand(query);
            command.Connection = connection;
            command.Transaction = transaction;
            
            command.Parameters.AddWithValue("@title", bookAuthors.title);
            var bookId = await command.ExecuteScalarAsync();

            int res = Convert.ToInt32(bookId);
            
            
            int authorId;
            // 2. Adding authors
            foreach (var author in bookAuthors.authors)
            {
                
                query = "SELECT PK FROM authors WHERE first_name = @firstName AND last_name = @lastName";
                using SqlCommand checkAuthorCommand = new SqlCommand(query);
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@firstName", author.firstName);
                command.Parameters.AddWithValue("@lastName", author.lastName);
                var result = await command.ExecuteScalarAsync();
                
                newBook.authors.Add(new Author()
                {
                    firstName = author.firstName,
                    lastName = author.lastName
                });

                if (result is null)
                {
                    query = "INSERT INTO authors (first_name, last_name) VALUES (@firstName, @lastName); SELECT @@IDENTITY AS ID;";
                    using SqlCommand addAuthorCommand = new SqlCommand(query);
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@firstName", author.firstName);
                    command.Parameters.AddWithValue("@lastName", author.lastName);
                    authorId = (int) await addAuthorCommand.ExecuteScalarAsync();
                }
                else
                {
                    authorId = (int)result;
                    
                }

                // 3. Update books_authors table
                query = "INSERT INTO books_authors (FK_book, FK_author) VALUES (@bookId, @authorId)";
                using SqlCommand linkBookAuthorCommand = new SqlCommand(query);
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@bookId", res);
                command.Parameters.AddWithValue("@authorId", authorId);
                await linkBookAuthorCommand.ExecuteNonQueryAsync();
                
                
                newBook.id = authorId;
                newBook.title = bookAuthors.title;
                
            }
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            return null;
        }

        return newBook;
    }
}