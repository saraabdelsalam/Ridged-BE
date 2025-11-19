using MediatR;
using Microsoft.AspNetCore.Identity;
using Ridged.Application.Common.Interfaces;
using Ridged.Domain.Common;
using Ridged.Domain.Entities;

namespace Ridged.Application.Features.Auth.ResetPassword
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;

        public ResetPasswordCommandHandler(
            IUserRepository userRepository,
            IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByPasswordResetTokenAsync(request.Token, cancellationToken);

            if (user == null)
            {
                return Result.Failure(
                    "Invalid or expired password reset token",
                    400 // Bad Request
                );
            }

            // Hash new password
            user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);

            // Clear reset token
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiryTime = null;
            user.UpdatedAt = DateTime.UtcNow;

            // Invalidate all refresh tokens for security
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            return Result.Success("Password has been reset successfully. Please login with your new password.");
        }
    }
}
