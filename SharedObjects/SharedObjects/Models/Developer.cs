namespace SharedObjects.Models;

public class Developer : User
{
    public ICollection<Task> Tasks { get; set; } = [];
}