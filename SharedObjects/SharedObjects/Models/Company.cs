using System.ComponentModel.DataAnnotations;

namespace SharedObjects.Models;

public class Company
{
    [Key] public int Id { get; set; }
    public string Name { get; set; }

    public ICollection<Project> Projects { get; set; } = [];
}