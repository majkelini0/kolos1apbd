namespace kolos1s27799.Models.DTOs;

public class BookAuthorsWithId
{
    public int id { get; set; }
    public string title { get; set; }
    public List<Author> authors { get; set; }
}