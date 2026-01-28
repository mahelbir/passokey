namespace Application.Models.PasskeyLogin;

public class FinishPasskeyLoginResponse
{
    public Guid UserId { get; set; }
    public string? State { get; set; }
    public string Token { get; set; }
    public string? Redirect { get; set; }
}