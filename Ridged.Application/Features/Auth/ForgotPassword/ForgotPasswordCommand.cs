using MediatR;
using Ridged.Domain.Common;

namespace Ridged.Application.Features.Auth.ForgotPassword
{
    public record ForgotPasswordCommand(string Email) : IRequest<Result>;
}
