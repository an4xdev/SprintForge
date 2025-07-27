namespace SharedObjects.DTOs.Requests;

public class ChangePasswordRequest
{
    public Guid UserId { get; set; }

    public string OldPassword { get; set; } = string.Empty;

    public string NewPassword { get; set; } = string.Empty;
}