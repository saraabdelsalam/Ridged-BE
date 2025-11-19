using MediatR;
using Ridged.Application.Features.Auth.Register.DTOs;
using Ridged.Domain.Common;

namespace Ridged.Application.Features.Auth.Register
{
    public record RegisterCommand(
        string Email,
        string Password,
        string ConfirmPassword,
        string FirstName,
        string LastName
    ) : IRequest<Result<RegisterResponse>>;
}
