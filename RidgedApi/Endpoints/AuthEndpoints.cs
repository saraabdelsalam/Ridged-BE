using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ridged.Application.Common.Models;
using Ridged.Application.Features.Auth.ForgotPassword;
using Ridged.Application.Features.Auth.ForgotPassword.DTOs;
using Ridged.Application.Features.Auth.Login;
using Ridged.Application.Features.Auth.Login.DTOs;
using Ridged.Application.Features.Auth.RefreshToken;
using Ridged.Application.Features.Auth.RefreshToken.DTOs;
using Ridged.Application.Features.Auth.Register;
using Ridged.Application.Features.Auth.Register.DTOs;
using Ridged.Application.Features.Auth.ResetPassword;
using Ridged.Application.Features.Auth.ResetPassword.DTOs;
using Ridged.Application.Features.Auth.VerifyEmail;

namespace RidgedApi.Endpoints
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/auth")
                .WithTags("Authentication");

            group.MapPost("/register", RegisterAsync)
                .WithName("Register")
                .WithDescription("Register a new customer account");

            group.MapPost("/verify-email", VerifyEmailAsync)
                .WithName("VerifyEmail")
                .WithDescription("Verify email address with token");

            group.MapPost("/login", LoginAsync)
                .WithName("Login")
                .WithDescription("Login with email and password");

            group.MapPost("/refresh-token", RefreshTokenAsync)
                .WithName("RefreshToken")
                .WithDescription("Refresh access token using refresh token");

            group.MapPost("/forgot-password", ForgotPasswordAsync)
                .WithName("ForgotPassword")
                .WithDescription("Request password reset token");

            group.MapPost("/reset-password", ResetPasswordAsync)
                .WithName("ResetPassword")
                .WithDescription("Reset password using token");
        }

        private static async Task<IResult> RegisterAsync(
            [FromBody] RegisterRequest request,
            IMediator mediator,
            CancellationToken cancellationToken)
        {
            var command = new RegisterCommand(
                request.Email,
                request.Password,
                request.ConfirmPassword,
                request.FirstName,
                request.LastName
            );

            var result = await mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
            {
                var response = ApiResponse<RegisterResponse>.FailureResponse(
                    result.Message,
                    result.StatusCode
                );
                return Results.Json(response, statusCode: result.StatusCode);
            }

            return Results.Ok(ApiResponse<RegisterResponse>.SuccessResponse(
                result.Data!,
                result.Message,
                200
            ));
        }

        private static async Task<IResult> VerifyEmailAsync(
            [FromBody] VerifyEmailRequest request,
            IMediator mediator,
            CancellationToken cancellationToken)
        {
            var command = new VerifyEmailCommand(request.Token);
            var result = await mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
            {
                var response = ApiResponse.FailureResponse(
                    result.Message,
                    result.StatusCode
                );
                return Results.Json(response, statusCode: result.StatusCode);
            }

            return Results.Ok(ApiResponse.SuccessResponse(result.Message, 200));
        }

        private static async Task<IResult> LoginAsync(
            [FromBody] LoginRequest request,
            IMediator mediator,
            CancellationToken cancellationToken)
        {
            var command = new LoginCommand(request.Email, request.Password);
            var result = await mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
            {
                var response = ApiResponse<LoginResponse>.FailureResponse(
                    result.Message,
                    result.StatusCode
                );
                return Results.Json(response, statusCode: result.StatusCode);
            }

            return Results.Ok(ApiResponse<LoginResponse>.SuccessResponse(
                result.Data!,
                result.Message,
                200
            ));
        }

        private static async Task<IResult> RefreshTokenAsync(
            [FromBody] RefreshTokenRequest request,
            IMediator mediator,
            CancellationToken cancellationToken)
        {
            var command = new RefreshTokenCommand(request.AccessToken, request.RefreshToken);
            var result = await mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
            {
                var response = ApiResponse<RefreshTokenResponse>.FailureResponse(
                    result.Message,
                    result.StatusCode
                );
                return Results.Json(response, statusCode: result.StatusCode);
            }

            return Results.Ok(ApiResponse<RefreshTokenResponse>.SuccessResponse(
                result.Data!,
                result.Message,
                200
            ));
        }

        private static async Task<IResult> ForgotPasswordAsync(
            [FromBody] ForgotPasswordRequest request,
            IMediator mediator,
            CancellationToken cancellationToken)
        {
            var command = new ForgotPasswordCommand(request.Email);
            var result = await mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
            {
                var response = ApiResponse.FailureResponse(
                    result.Message,
                    result.StatusCode
                );
                return Results.Json(response, statusCode: result.StatusCode);
            }

            return Results.Ok(ApiResponse.SuccessResponse(result.Message, 200));
        }

        private static async Task<IResult> ResetPasswordAsync(
            [FromBody] ResetPasswordRequest request,
            IMediator mediator,
            CancellationToken cancellationToken)
        {
            var command = new ResetPasswordCommand(
                request.Token,
                request.NewPassword,
                request.ConfirmPassword
            );

            var result = await mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
            {
                var response = ApiResponse.FailureResponse(
                    result.Message,
                    result.StatusCode
                );
                return Results.Json(response, statusCode: result.StatusCode);
            }

            return Results.Ok(ApiResponse.SuccessResponse(result.Message, 200));
        }
    }

    public record VerifyEmailRequest(string Token);
}
