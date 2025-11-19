using MediatR;
using Ridged.Application.Common.Interfaces;
using Ridged.Domain.Common;
using System.Security.Cryptography;

namespace Ridged.Application.Features.Auth.ForgotPassword
{
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
    {
        private readonly IUserRepository _userRepository;

        public ForgotPasswordCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

            // Don't reveal whether user exists or not for security
            if (user == null)
            {
                return Result.Success("If an account with that email exists, a password reset link has been sent");
            }

            // Generate password reset token
            var resetToken = GenerateResetToken();

            user.PasswordResetToken = resetToken;
            user.PasswordResetTokenExpiryTime = DateTime.UtcNow.AddHours(1);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            // TODO: Send email with reset token
            // For now, we'll return the token in the response (remove this in production)

            return Result.Success($"If an account with that email exists, a password reset link has been sent. Token: {resetToken}");
        }

        private static string GenerateResetToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
