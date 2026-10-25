using System.Globalization;
using System.Threading.RateLimiting;
using GeoRetrievo.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
if (builder.Environment.IsDevelopment()) {
    builder.Logging.AddSimpleConsole(options => {
        options.IncludeScopes = true;
        options.SingleLine = true;
        options.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff ";
        options.UseUtcTimestamp = true;
    });
} else {
    builder.Logging.AddJsonConsole(options => {
        options.IncludeScopes = true;
        options.UseUtcTimestamp = true;
    });
}

// Configure the application to listen on the port specified in the PORT environment variable
if (builder.Configuration["PORT"] is { } port) {
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddSingleton<ICountryDataSource, GoogleCloudStorageCountryDataSource>();
builder.Services.AddSingleton<CountryCatalogLoader>();
builder.Services.AddRateLimiter(options => {
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) => {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)) {
            context.HttpContext.Response.Headers.RetryAfter =
                Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds))
                    .ToString(CultureInfo.InvariantCulture);
        }

        await context.HttpContext.Response.WriteAsync(
            "Too many requests. Please try again later.",
            cancellationToken);
    };

    options.AddPolicy("countries", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

var app = builder.Build();

app.UseRouting();

app.UseRateLimiter();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
    app.UseHttpsRedirection();
}


app.MapGet("/health", () => Results.Ok(new {
    status = "healthy"
}));

var catalogLoader = app.Services.GetRequiredService<CountryCatalogLoader>();
var countries = await catalogLoader.LoadAsync();

app.MapGet("/countries", () => {
    var allCountries = countries.GetAll();
    return allCountries is null ? Results.NotFound() : Results.Ok(allCountries);
}).RequireRateLimiting("countries");

app.MapGet("/countries/{code}", (string code) => {
    var country = countries.GetByCode(code);
    return country is null ? Results.NotFound() : Results.Ok(country);
}).RequireRateLimiting("countries");

app.Run();
