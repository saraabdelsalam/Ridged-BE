namespace Ridged.Application.Features.Auth.ResetPassword.DTOs
{
    public record ResetPasswordRequest(
        string Token,
        string NewPassword,
        string ConfirmPassword
    );
}
