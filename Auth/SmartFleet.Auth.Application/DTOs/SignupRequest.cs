namespace SmartFleet.Auth.Application.DTOs
{
    public record SignupRequest(string FullName,
    string Email,
    string Password);
}
