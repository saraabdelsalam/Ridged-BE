using MediatR;
using Ridged.Application.Common.Interfaces;
using Ridged.Domain.Common;

namespace Ridged.Application.Features.Auth.Logout
{
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
    {   
        private readonly IUserRepository _userRepository;

        public LogoutCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            // Get the user from the database
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

            if (user == null)
            {
                return Result.Failure("User not found.", 404);
            }

            // Clear the refresh token to invalidate it
            // This prevents the user from using the refresh token to get new access tokens
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            user.UpdatedAt = DateTime.UtcNow;

            // Save changes to the database
            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            return Result.Success("User logged out successfully.");
        }
    }
}
