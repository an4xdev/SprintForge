namespace SharedObjects.Models;

public class Manager : User
{
    public ICollection<Sprint> Sprints { get; set; } = [];
}