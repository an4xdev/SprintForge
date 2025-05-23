using System.ComponentModel.DataAnnotations;

namespace SharedObjects.Models;

public class TaskHistory
{
    [Key] public Guid Id { get; set; }

    public Guid TaskId { get; set; }
    public Task Task { get; set; } = null!;

    public DateTime ChangeDate { get; set; }

    public string? OldStatus { get; set; } = string.Empty;

    public string NewStatus { get; set; } = string.Empty;
}