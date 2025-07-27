namespace SharedObjects.DTOs.Responses;

public class LoginResponse : TokenResponse
{
    public bool NeedResetPassword { get; set; }
}