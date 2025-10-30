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
        var request = context.Request;
        var stopwatch = Stopwatch.StartNew();

        // Collect request details
        var method = request.Method;
        var path = request.Path;
        var query = request.QueryString.HasValue ? request.QueryString.Value : string.Empty;
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var headers = string.Join("; ", request.Headers.Select(h => $"{h.Key}={h.Value}"));

        _logger.LogInformation("➡️ Request {Method} {Path}{Query} from {ClientIP}", method, path, query, clientIp);
        _logger.LogDebug("Headers: {Headers}", headers);

        // Read request body if available
        string? requestBody = null;
        if (request.ContentLength > 0 && request.Body.CanSeek)
        {
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            request.Body.Position = 0;
        }

        if (!string.IsNullOrWhiteSpace(requestBody))
            _logger.LogDebug("Request Body: {Body}", requestBody);

        // Capture and replace response stream
        var originalResponseBody = context.Response.Body;
        await using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        try
        {
            await _next(context);
            stopwatch.Stop();

            // Read response
            responseBodyStream.Position = 0;
            var responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();
            responseBodyStream.Position = 0;

            var statusCode = context.Response.StatusCode;
            _logger.LogInformation("⬅️ Response {StatusCode} for {Method} {Path} in {Elapsed} ms",
                statusCode, method, path, stopwatch.ElapsedMilliseconds);

            if (!string.IsNullOrWhiteSpace(responseBody))
                _logger.LogDebug("Response Body: {Body}", responseBody);

            await responseBodyStream.CopyToAsync(originalResponseBody);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "❌ Exception in {Method} {Path} after {Elapsed} ms from {ClientIP}",
                method, path, stopwatch.ElapsedMilliseconds, clientIp);
            throw;
        }
        finally
        {
            context.Response.Body = originalResponseBody;
        }
    }
}