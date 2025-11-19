using MediatR;
using Microsoft.Extensions.Options;
using Ridged.Application.Common.Interfaces;
using Ridged.Application.Common.Settings;
using Ridged.Application.Features.Auth.RefreshToken.DTOs;
using Ridged.Domain.Common;
using System.Security.Claims;

namespace Ridged.Application.Features.Auth.RefreshToken
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly JwtSettings _jwtSettings;

        public RefreshTokenCommandHandler(
            IUserRepository userRepository,
            IJwtService jwtService,
            IOptions<JwtSettings> jwtSettings)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            // Get principal from expired access token
            var principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken);

            if (principal == null)
            {
                return Result.Failure<RefreshTokenResponse>(
                    "Invalid access token",
                    401 // Unauthorized
                );
            }

            // Get user ID from claims
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Result.Failure<RefreshTokenResponse>(
                    "Invalid token claims",
                    401 // Unauthorized
                );
            }

            // Get user
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

            if (user == null)
            {
                return Result.Failure<RefreshTokenResponse>(
                    "User not found",
                    404 // Not Found
                );
            }

            // Validate refresh token
            if (user.RefreshToken != request.RefreshToken)
            {
                return Result.Failure<RefreshTokenResponse>(
                    "Invalid refresh token",
                    401 // Unauthorized
                );
            }

            if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return Result.Failure<RefreshTokenResponse>(
                    "Refresh token has expired",
                    401 // Unauthorized
                );
            }

            // Generate new tokens
            var newAccessToken = _jwtService.GenerateAccessToken(user.Id, user.Email!, user.Role.ToString());
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            // Update refresh token
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            var response = new RefreshTokenResponse(newAccessToken, newRefreshToken);

            return Result.Success(response, "Token refreshed successfully");
        }
    }
}
