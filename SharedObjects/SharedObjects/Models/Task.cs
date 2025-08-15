using System.ComponentModel.DataAnnotations;

namespace SharedObjects.Models;

// TODO: think about attachments and comments to task

public class Task
{
    [Key] public Guid Id { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }

    public int TaskTypeId { get; set; }
    public TaskType TaskType { get; set; } = null!;

    public int TaskStatusId { get; set; }
    public TaskStatus TaskStatus { get; set; } = null!;

    public Guid? DeveloperId { get; set; }
    public Developer? Developer { get; set; }

    public Guid? SprintId { get; set; }
    public Sprint? Sprint { get; set; }

    public Guid? ProjectId { get; set; }
    public Project? Project { get; set; } = null!;

    public ICollection<TaskHistory> TaskHistory { get; set; } = [];
}