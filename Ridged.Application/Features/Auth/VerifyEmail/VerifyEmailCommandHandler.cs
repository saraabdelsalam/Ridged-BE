using MediatR;
using Ridged.Application.Common.Interfaces;
using Ridged.Domain.Common;

namespace Ridged.Application.Features.Auth.VerifyEmail
{
    public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Result>
    {
        private readonly IUserRepository _userRepository;

        public VerifyEmailCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByVerificationTokenAsync(request.Token, cancellationToken);

            if (user == null)
            {
                return Result.Failure(
                    "Invalid or expired verification token",
                    400 // Bad Request
                );
            }

            user.EmailConfirmed = true;
            user.VerificationToken = null;
            user.VerificationTokenExpiryTime = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            return Result.Success("Email verified successfully. You can now login to your account.");
        }
    }
}
