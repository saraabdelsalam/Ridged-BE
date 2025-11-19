namespace Ridged.Application.Features.Auth.RefreshToken.DTOs
{
    public record RefreshTokenRequest(
        string AccessToken,
        string RefreshToken
    );
}
