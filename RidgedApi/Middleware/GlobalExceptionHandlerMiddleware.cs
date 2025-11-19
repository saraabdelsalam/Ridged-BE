using Ridged.Application.Common.Exceptions;
using Ridged.Application.Common.Models;
using System.Text.Json;

namespace RidgedApi.Middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var requestPath = context.Request?.Path.Value;
                var userId = context.User?.Identity?.IsAuthenticated == true
                    ? context.User.Identity.Name ?? context.User.FindFirst("sub")?.Value ?? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                    : "Anonymous";
                var correlationId = context.Request.Headers.ContainsKey("X-Correlation-ID")
                    ? context.Request.Headers["X-Correlation-ID"].ToString()
                    : context.TraceIdentifier;

                _logger.LogError(ex,
                    $"An unhandled exception occurred. Path: {requestPath}, UserId: {userId}, CorrelationId: {correlationId}");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            ApiResponse response;
            int statusCode;

            switch (exception)
            {
                case AppException appException:
                    statusCode = appException.StatusCode;
                    response = ApiResponse.FailureResponse(
                        appException.Message,
                        appException.StatusCode,
                        appException.ValidationErrors
                    );
                    break;

                default:
                    statusCode = 500;
                    response = ApiResponse.FailureResponse(
                        "An internal server error occurred. Please try again later.",
                        500
                    );
                    break;
            }

            context.Response.StatusCode = statusCode;

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }
    }

    public static class GlobalExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        }
    }
}
