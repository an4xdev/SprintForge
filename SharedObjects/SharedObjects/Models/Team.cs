using System.ComponentModel.DataAnnotations;

namespace SharedObjects.Models;

public class Team
{
    [Key]
    public Guid Id { get; set; }

    public string Name { get; set; }

    public Guid ManagerId { get; set; }
    public Manager Manager { get; set; } = null!;

    public Guid? ProjectId { get; set; }
    public Project? Project { get; set; } = null!;

    public List<Developer> Developers { get; set; } = [];
}