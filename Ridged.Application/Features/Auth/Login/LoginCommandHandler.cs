using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Ridged.Application.Common.Interfaces;
using Ridged.Application.Common.Settings;
using Ridged.Application.Features.Auth.Login.DTOs;
using Ridged.Domain.Common;
using Ridged.Domain.Entities;

namespace Ridged.Application.Features.Auth.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IJwtService _jwtService;
        private readonly JwtSettings _jwtSettings;

        public LoginCommandHandler(
            IUserRepository userRepository,
            IPasswordHasher<User> passwordHasher,
            IJwtService jwtService,
            IOptions<JwtSettings> jwtSettings)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            // Find user by email
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

            if (user == null)
            {
                return Result.Failure<LoginResponse>(
                    "Invalid email or password",
                    401 // Unauthorized
                );
            }

            // Verify password
            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash!, request.Password);

            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return Result.Failure<LoginResponse>(
                    "Invalid email or password",
                    401 // Unauthorized
                );
            }

            // Check if email is verified
            if (!user.EmailConfirmed)
            {
                return Result.Failure<LoginResponse>(
                    "Please verify your email before logging in",
                    403 // Forbidden
                );
            }

            // Check if account is active
            if (!user.IsActive)
            {
                return Result.Failure<LoginResponse>(
                    "Your account has been deactivated. Please contact support",
                    403 // Forbidden
                );
            }

            // Generate tokens
            var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Email!, user.Role.ToString());
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Save refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            var response = new LoginResponse(
                accessToken,
                refreshToken,
                user.Id,
                user.Email!,
                user.FirstName,
                user.LastName,
                user.Role.ToString()
            );

            return Result.Success(response, "Login successful");
        }
    }
}
