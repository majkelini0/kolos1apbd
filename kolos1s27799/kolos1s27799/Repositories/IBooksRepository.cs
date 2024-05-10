using kolos1s27799.Models.DTOs;

namespace kolos1s27799.Repositories;

public interface IBooksRepository
{
    Task<bool> DoesBookExist(int id);
    Task<BookAuthorsWithId> GetBookWithAuthors(int id);
    Task<bool> DoesBookExist(string title);
    Task<BookAuthorsWithId> AddBook(BookAuthors bookAuthors);
}