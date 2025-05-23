using System.ComponentModel.DataAnnotations;

namespace SharedObjects.Models;

public class Project
{
    [Key] public Guid Id { get; set; }
    public string Name { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public ICollection<Sprint> Sprints { get; set; } = [];
}