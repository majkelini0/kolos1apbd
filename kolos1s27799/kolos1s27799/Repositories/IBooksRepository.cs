namespace kolos1s27799.Repositories;

public interface IBooksRepository
{
    Task<bool> DoesBookExist(int id);
}