using System.Text;

namespace ECommerce.Huit.API.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Log request
        var request = context.Request;
        var requestBody = "";
        if (request.Body.CanRead && request.ContentLength > 0)
        {
            request.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            request.Body.Seek(0, SeekOrigin.Begin);
        }

        _logger.LogInformation(
            "Request: {Method} {Path} {QueryString} {RequestBody}",
            request.Method,
            request.Path,
            request.QueryString,
            requestBody);

        // Capture response
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        stopwatch.Stop();
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        _logger.LogInformation(
            "Response: {StatusCode} {ResponseText} (elapsed: {ElapsedMs}ms)",
            context.Response.StatusCode,
            responseText,
            stopwatch.ElapsedMilliseconds);

        await responseBody.CopyToAsync(originalBodyStream);
    }
}
