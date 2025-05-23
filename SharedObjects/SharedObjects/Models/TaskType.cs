using System.ComponentModel.DataAnnotations;

namespace SharedObjects.Models;

public class TaskType
{
    [Key] public int Id { get; set; }

    public string Name { get; set; }

    public ICollection<Task> Tasks { get; set; } = [];
}