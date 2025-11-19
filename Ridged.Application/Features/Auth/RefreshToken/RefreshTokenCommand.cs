using MediatR;
using Ridged.Application.Features.Auth.RefreshToken.DTOs;
using Ridged.Domain.Common;

namespace Ridged.Application.Features.Auth.RefreshToken
{
    public record RefreshTokenCommand(
        string AccessToken,
        string RefreshToken
    ) : IRequest<Result<RefreshTokenResponse>>;
}
