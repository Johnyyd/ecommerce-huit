using System.Net;
using System.Text.Json;

namespace ECommerce.Huit.API.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new
            {
                error = "InternalServerError",
                message = "An unexpected error occurred. Please try again later.",
                // In production, don't expose exception details
                // details = context.Request.IsLocal ? ex.ToString() : null
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
