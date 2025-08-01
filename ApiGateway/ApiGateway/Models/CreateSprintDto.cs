namespace ApiGateway.Models;

public class CreateSprintDto
{
    public string Name { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public Guid TeamId { get; set; }
    public Guid ManagerId { get; set; }
    public Guid ProjectId { get; set; }
}