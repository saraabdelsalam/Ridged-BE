using MediatR;
using Ridged.Application.Features.Auth.Login.DTOs;
using Ridged.Domain.Common;

namespace Ridged.Application.Features.Auth.Login
{
    public record LoginCommand(
        string Email,
        string Password
    ) : IRequest<Result<LoginResponse>>;
}
