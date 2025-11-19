namespace Ridged.Application.Features.Auth.Register.DTOs
{
    public record RegisterResponse(
        int UserId,
        string Email,
        string Message
    );
}
