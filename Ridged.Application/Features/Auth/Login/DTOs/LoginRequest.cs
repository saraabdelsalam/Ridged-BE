namespace Ridged.Application.Features.Auth.Login.DTOs
{
    public record LoginRequest(
        string Email,
        string Password
    );
}
