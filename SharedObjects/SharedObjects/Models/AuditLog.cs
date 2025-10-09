using System.ComponentModel.DataAnnotations;

namespace SharedObjects.Models;

public class AuditLog
{
    [Key]
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Service { get; set; }
    public string Action { get; set; }
    public string Entity { get; set; }
    public string Description { get; set; }
}

