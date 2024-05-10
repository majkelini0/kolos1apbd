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
}