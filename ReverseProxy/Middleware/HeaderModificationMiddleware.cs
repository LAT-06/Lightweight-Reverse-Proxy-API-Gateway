namespace ReverseProxy.Middleware;

public class HeaderModificationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<HeaderModificationMiddleware> _logger;

    public HeaderModificationMiddleware(RequestDelegate next, ILogger<HeaderModificationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add custom request headers
        var requestId = context.Items["RequestId"]?.ToString() ?? Guid.NewGuid().ToString();
        
        context.Request.Headers["X-Request-ID"] = requestId;
        context.Request.Headers["X-Forwarded-For"] = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        context.Request.Headers["X-Forwarded-Proto"] = context.Request.Scheme;
        context.Request.Headers["X-Proxy-By"] = "YARP-Gateway";

        _logger.LogDebug("Added custom headers to request: {RequestId}", requestId);

        await _next(context);

        // Add custom response headers
        context.Response.Headers["X-Request-ID"] = requestId;
        context.Response.Headers["X-Powered-By"] = "YARP-Gateway";
        
        // Add CORS headers (if needed)
        context.Response.Headers["Access-Control-Allow-Origin"] = "*";
        context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, OPTIONS";
        context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization";
    }
}
