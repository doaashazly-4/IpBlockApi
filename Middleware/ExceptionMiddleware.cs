using System.Net;
using Newtonsoft.Json;

namespace IpBlockApi.Middleware;

public sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Argument validation error");
            await HandleExceptionAsync(context, ex, StatusCodes.Status400BadRequest, "Invalid argument provided.");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation");
            await HandleExceptionAsync(context, ex, StatusCodes.Status400BadRequest, "Invalid operation.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(context, ex, StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception, int statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new
        {
            message,
            error = exception.Message,
            timestamp = DateTimeOffset.UtcNow
        };

        return context.Response.WriteAsync(JsonConvert.SerializeObject(response));
    }
}
