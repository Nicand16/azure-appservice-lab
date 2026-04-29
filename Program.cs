using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// If APPLICATIONINSIGHTS_CONNECTION_STRING exists, telemetry will be sent to Application Insights.
// If it does not exist, the app still runs normally.
builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

var logger = app.Logger;

string GetEnv(string name, string fallback = "not-set")
{
    return Environment.GetEnvironmentVariable(name) ?? fallback;
}

app.MapGet("/", () =>
{
    var siteName = GetEnv("WEBSITE_SITE_NAME", "local-machine");
    var slotName = GetEnv("WEBSITE_SLOT_NAME", "local");
    var appVersion = GetEnv("APP_VERSION", "v1-local");
    var customMessage = GetEnv("CUSTOM_MESSAGE", "Hello from local/app default");

    var html = $$"""
    <!doctype html>
    <html lang="en">
    <head>
      <meta charset="utf-8" />
      <meta name="viewport" content="width=device-width, initial-scale=1" />
      <title>Azure App Service Lab</title>
      <style>
        body { font-family: system-ui, Arial, sans-serif; margin: 40px; line-height: 1.5; }
        code { background: #f3f3f3; padding: 2px 6px; border-radius: 4px; }
        .card { border: 1px solid #ddd; border-radius: 12px; padding: 20px; max-width: 820px; }
        li { margin-bottom: 8px; }
      </style>
    </head>
    <body>
      <div class="card">
        <h1>Azure App Service Lab</h1>
        <p>This simple app is for practicing Azure App Service operations.</p>

        <h2>Current environment</h2>
        <p><strong>Site:</strong> {{siteName}}</p>
        <p><strong>Slot:</strong> {{slotName}}</p>
        <p><strong>Version:</strong> {{appVersion}}</p>
        <p><strong>Message:</strong> {{customMessage}}</p>
        <p><strong>UTC time:</strong> {{DateTimeOffset.UtcNow}}</p>

        <h2>Useful test endpoints</h2>
        <ul>
          <li><a href="/api/health">/api/health</a> — health and environment check</li>
          <li><a href="/api/config-check">/api/config-check</a> — read app settings</li>
          <li><a href="/api/log-demo">/api/log-demo</a> — generate informational/warning logs</li>
          <li><a href="/api/error">/api/error</a> — intentionally generate HTTP 500</li>
          <li><a href="/api/slow?delay=3000">/api/slow?delay=3000</a> — simulate slow request</li>
          <li><a href="/api/status/404">/api/status/404</a> — return a chosen status code</li>
        </ul>
      </div>
    </body>
    </html>
    """;

    return Results.Content(html, "text/html");
});

app.MapGet("/api/health", () =>
{
    logger.LogInformation("Health endpoint called at {UtcTime}", DateTimeOffset.UtcNow);

    return Results.Ok(new
    {
        status = "Healthy",
        app = "Azure App Service Lab",
        site = GetEnv("WEBSITE_SITE_NAME", "local-machine"),
        slot = GetEnv("WEBSITE_SLOT_NAME", "local"),
        version = GetEnv("APP_VERSION", "v1-local"),
        environment = GetEnv("ASPNETCORE_ENVIRONMENT", "Development/local"),
        utcTime = DateTimeOffset.UtcNow
    });
});

app.MapGet("/api/config-check", () =>
{
    logger.LogInformation("Config check endpoint called");

    return Results.Ok(new
    {
        appVersion = GetEnv("APP_VERSION", "v1-local"),
        customMessage = GetEnv("CUSTOM_MESSAGE", "not configured"),
        featureFlag = GetEnv("FEATURE_FLAG", "false"),
        importantNote = "In real production, never expose secrets in an endpoint like this."
    });
});

app.MapGet("/api/log-demo", () =>
{
    logger.LogInformation("This is an INFORMATION log from /api/log-demo");
    logger.LogWarning("This is a WARNING log from /api/log-demo. Nothing is broken; this is only for practice.");

    return Results.Ok(new
    {
        message = "Logs were generated. Check Log stream, App Service logs, or Application Insights.",
        utcTime = DateTimeOffset.UtcNow
    });
});

app.MapGet("/api/error", () =>
{
    logger.LogError("Intentional error endpoint called. This will throw an exception.");
    throw new InvalidOperationException("Intentional test exception from /api/error");
});

app.MapGet("/api/slow", async ([FromQuery] int delay = 2000) =>
{
    delay = Math.Clamp(delay, 0, 10000);
    logger.LogInformation("Slow endpoint called with delay {Delay} ms", delay);

    await Task.Delay(delay);

    return Results.Ok(new
    {
        message = "Slow request completed",
        delayMs = delay,
        utcTime = DateTimeOffset.UtcNow
    });
});

app.MapGet("/api/status/{code:int}", (int code) =>
{
    if (code < 100 || code > 599)
    {
        return Results.BadRequest(new { error = "Status code must be between 100 and 599" });
    }

    logger.LogWarning("Returning manual HTTP status code {StatusCode}", code);

    return Results.StatusCode(code);
});

app.MapGet("/api/headers", (HttpRequest request) =>
{
    var safeHeaders = request.Headers
        .Where(h => !h.Key.Contains("Authorization", StringComparison.OrdinalIgnoreCase)
                 && !h.Key.Contains("Cookie", StringComparison.OrdinalIgnoreCase))
        .ToDictionary(h => h.Key, h => h.Value.ToString());

    return Results.Ok(new
    {
        headers = safeHeaders,
        note = "Authorization and Cookie headers are intentionally excluded."
    });
});

app.Run();
