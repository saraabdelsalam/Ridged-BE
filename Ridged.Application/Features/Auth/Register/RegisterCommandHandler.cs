using MediatR;
using Microsoft.AspNetCore.Identity;
using Ridged.Application.Common.Interfaces;
using Ridged.Application.Features.Auth.Register.DTOs;
using Ridged.Domain.Common;
using Ridged.Domain.Entities;
using Ridged.Domain.Enums;
using System.Security.Cryptography;

namespace Ridged.Application.Features.Auth.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;

        public RegisterCommandHandler(
            IUserRepository userRepository,
            IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<Result<RegisterResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            // Check if email already exists
            if (await _userRepository.EmailExistsAsync(request.Email, cancellationToken))
            {
                return Result.Failure<RegisterResponse>(
                    "An account with this email already exists",
                    409 // Conflict
                );
            }

            // Generate verification token
            var verificationToken = GenerateVerificationToken();

            // Create user
            var user = new User
            {
                Email = request.Email,
                UserName = request.Email,
                NormalizedEmail = request.Email.ToUpper(),
                NormalizedUserName = request.Email.ToUpper(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = UserRole.Customer,
                EmailConfirmed = false,
                VerificationToken = verificationToken,
                VerificationTokenExpiryTime = DateTime.UtcNow.AddHours(24),
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            // Hash password
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            // Save user
            await _userRepository.AddAsync(user, cancellationToken);
            var saved = await _userRepository.SaveChangesAsync(cancellationToken);

            if (!saved)
            {
                return Result.Failure<RegisterResponse>(
                    "Failed to create user account. Please try again",
                    500 // Internal Server Error
                );
            }

            // TODO: Send verification email with verificationToken
            // For now, we'll return the token in the response (remove this in production)

            var response = new RegisterResponse(
                user.Id,
                user.Email!,
                $"Account created successfully. Please verify your email using the verification token. Token: {verificationToken}"
            );

            return Result.Success(response, "Registration successful. Please check your email to verify your account.");
        }

        private static string GenerateVerificationToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
