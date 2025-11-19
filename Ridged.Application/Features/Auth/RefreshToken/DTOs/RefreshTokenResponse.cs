namespace Ridged.Application.Features.Auth.RefreshToken.DTOs
{
    public record RefreshTokenResponse(
        string AccessToken,
        string RefreshToken
    );
}
