using System.Diagnostics;
using System.Text;

namespace API.Middleware;

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
        var stopwatch = Stopwatch.StartNew();
        var request = context.Request;

        // Log request info
        _logger.LogInformation("➡️ HTTP {Method} {Path}{QueryString}", request.Method, request.Path, request.QueryString);

        // Optional: log body (for POST/PUT)
        if (request.ContentLength > 0 && request.Body.CanSeek)
        {
            request.Body.Position = 0;
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            _logger.LogInformation("Request Body: {Body}", body);
        }

        try
        {
            // Capture response body
            var originalBody = context.Response.Body;
            using var memStream = new MemoryStream();
            context.Response.Body = memStream;

            await _next(context); // Continue pipeline

            stopwatch.Stop();

            // Read response
            memStream.Position = 0;
            var responseBody = await new StreamReader(memStream).ReadToEndAsync();
            memStream.Position = 0;

            // Log response info
            _logger.LogInformation("⬅️ HTTP {StatusCode} {Method} {Path} completed in {Elapsed} ms",
                context.Response.StatusCode, request.Method, request.Path, stopwatch.ElapsedMilliseconds);

            if (!string.IsNullOrWhiteSpace(responseBody))
                _logger.LogDebug("Response Body: {ResponseBody}", responseBody);

            // Copy back to original response stream
            await memStream.CopyToAsync(originalBody);
            context.Response.Body = originalBody;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "❌ Exception during {Method} {Path} after {Elapsed} ms",
                request.Method, request.Path, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}