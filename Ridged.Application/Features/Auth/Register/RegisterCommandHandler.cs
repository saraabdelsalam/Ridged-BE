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
        private readonly UserManager<User> _userManager;

        public RegisterCommandHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Result<RegisterResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
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
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = UserRole.Customer,
                EmailConfirmed = false,
                VerificationToken = verificationToken,
                VerificationTokenExpiryTime = DateTime.UtcNow.AddHours(24),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // Create user with password using UserManager
            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result.Failure<RegisterResponse>(
                    $"Failed to create user account: {errors}",
                    400 // Bad Request
                );
            }

            // Assign role to user
            var roleResult = await _userManager.AddToRoleAsync(user, UserRole.Customer.ToString());
            
            if (!roleResult.Succeeded)
            {
                // Rollback: delete the user if role assignment fails
                await _userManager.DeleteAsync(user);
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                return Result.Failure<RegisterResponse>(
                    $"Failed to assign role: {errors}",
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
