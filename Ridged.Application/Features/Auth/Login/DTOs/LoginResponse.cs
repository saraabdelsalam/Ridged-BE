namespace Ridged.Application.Features.Auth.Login.DTOs
{
    public record LoginResponse(
        string AccessToken,
        string RefreshToken,
        int UserId,
        string Email,
        string FirstName,
        string LastName,
        string Role
    );
}
