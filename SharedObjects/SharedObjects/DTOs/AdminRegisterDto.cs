namespace SharedObjects.DTOs;

public class AdminRegisterDto : UserLoginDto
{
    public string Role { get; set; } = string.Empty;
}