using MediatR;
using Ridged.Domain.Common;

namespace Ridged.Application.Features.Auth.Logout
{
    public record LogoutCommand(int UserId) : IRequest<Result>;
}   