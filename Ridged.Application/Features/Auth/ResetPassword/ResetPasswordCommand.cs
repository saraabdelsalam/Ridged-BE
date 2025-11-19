using MediatR;
using Ridged.Domain.Common;

namespace Ridged.Application.Features.Auth.ResetPassword
{
    public record ResetPasswordCommand(
        string Token,
        string NewPassword,
        string ConfirmPassword
    ) : IRequest<Result>;
}
