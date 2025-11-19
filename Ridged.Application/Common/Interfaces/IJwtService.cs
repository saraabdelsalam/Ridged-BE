using System.Security.Claims;

namespace Ridged.Application.Common.Interfaces
{
    public interface IJwtService
    {
        string GenerateAccessToken(int userId, string email, string role);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
        bool ValidateToken(string token);
    }
}
