using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace Item.API.Middleware;

public class GlobalExceptionMiddleware
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
            _logger.LogError(ex, "An unhandled exception occurred while processing the request");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var problemDetails = CreateProblemDetails(context, exception);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(problemDetails, options);
        await context.Response.WriteAsync(json);
    }

    private static ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
    {
        var statusCode = GetStatusCode(exception);
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Type = GetErrorType(exception),
            Title = GetTitle(exception),
            Detail = GetDetail(exception),
            Instance = context.Request.Path
        };

        // Add additional properties for specific exception types
        switch (exception)
        {
            case ValidationException validationException:
                problemDetails.Extensions["errors"] = validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );
                break;

            case ArgumentException _:
                problemDetails.Extensions["parameterName"] = 
                    ((ArgumentException)exception).ParamName;
                break;
        }

        return problemDetails;
    }

    private static int GetStatusCode(Exception exception) =>
        exception switch
        {
            ValidationException => (int)HttpStatusCode.BadRequest,
            ArgumentException => (int)HttpStatusCode.BadRequest,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            NotImplementedException => (int)HttpStatusCode.NotImplemented,
            TimeoutException => (int)HttpStatusCode.RequestTimeout,
            _ => (int)HttpStatusCode.InternalServerError
        };

    private static string GetTitle(Exception exception) =>
        exception switch
        {
            ValidationException => "Validation Error",
            ArgumentException => "Invalid Request",
            KeyNotFoundException => "Resource Not Found",
            UnauthorizedAccessException => "Unauthorized",
            NotImplementedException => "Not Implemented",
            TimeoutException => "Request Timeout",
            _ => "An error occurred while processing your request"
        };

    private static string GetErrorType(Exception exception) =>
        exception switch
        {
            ValidationException => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
            ArgumentException => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
            KeyNotFoundException => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
            UnauthorizedAccessException => "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1",
            NotImplementedException => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.2",
            TimeoutException => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.7",
            _ => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1"
        };

    private static string GetDetail(Exception exception)
    {
        // Don't expose sensitive information in production
        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
            ? exception.Message
            : "An error occurred while processing your request. Please try again later.";
    }
}