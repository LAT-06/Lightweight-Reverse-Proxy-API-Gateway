using System.Net;

var builder =
WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/api/health", () =>
{
    return Results.Ok(new
    {
        status = "healthy",
        service = "dotnet-api",
        hostname = Dns.GetHostName(),
        timestamp = DateTime.UtcNow
    });
});

app.MapGet("/api/data", () =>
{
    return Results.Ok(new
    {
        message = "Data from .NETAPI",

        service = "dotnet-api",
        hostname = Dns.GetHostName(),
        data = new
        {
            items = new[] { "item1", "item2", "item3" },
            count = 3
        },
        timestamp = DateTime.UtcNow
    });
});

app.MapGet("/api/info", () =>
{
    return Results.Ok(new
    {
        service = "dotnet-api",
        version = "1.0.0",
        hostname = Dns.GetHostName(),
        framework = Environment.Version.ToString(),
        os = Environment.OSVersion.ToString(),
        timestamp = DateTime.UtcNow
    });
});

app.Run();
