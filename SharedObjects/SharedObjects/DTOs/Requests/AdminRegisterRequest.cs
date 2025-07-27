namespace SharedObjects.DTOs.Requests;

public class AdminRegisterRequest : UserLoginRequest
{
    public string Role { get; set; } = string.Empty;
}