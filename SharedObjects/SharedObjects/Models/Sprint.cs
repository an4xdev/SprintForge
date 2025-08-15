using System.ComponentModel.DataAnnotations;

namespace SharedObjects.Models;

public class Sprint
{
    [Key] public Guid Id { get; set; }
    public string Name { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public Guid ManagerId { get; set; }
    public Manager Manager { get; set; } = null!;

    public Guid TeamId { get; set; }
    public Team Team { get; set; } = null!;

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public ICollection<Task> Tasks { get; set; } = [];
}