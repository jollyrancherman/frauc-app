using FluentValidation;
using System.Net;
using System.Text.Json;

namespace User.API.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest, 
                CreateValidationErrorResponse(validationEx)
            ),
            InvalidOperationException => (
                HttpStatusCode.BadRequest,
                new { message = exception.Message }
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                new { message = "An internal server error occurred." }
            )
        };

        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(message));
    }

    private static object CreateValidationErrorResponse(ValidationException validationException)
    {
        return new
        {
            message = "Validation failed",
            errors = validationException.Errors.GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                )
        };
    }
}