var builder = WebApplication.CreateBuilder(args);

// Configure application to use the HTTPS URL of the private Cloud Run API, which is provided by the environment variable Api:BaseUrl.
string? apiBaseUrl = builder.Configuration["Api:BaseUrl"];
if (!Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out var apiUri) ||
    (apiUri.Scheme != Uri.UriSchemeHttps && !(apiUri.IsLoopback && apiUri.Scheme == Uri.UriSchemeHttp)) ||
    apiUri.AbsolutePath != "/" ||
    !string.IsNullOrEmpty(apiUri.Query) ||
    !string.IsNullOrEmpty(apiUri.Fragment)) {
    throw new InvalidOperationException(
        "Configure Api:BaseUrl with the HTTPS URL of the private Cloud Run API.");
}

// Configure HTTP clients for the private Cloud Run API and the Google Cloud metadata server.
builder.Services.AddHttpClient("api", client => {
    client.BaseAddress = apiUri;
    client.Timeout = TimeSpan.FromSeconds(30);
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler {
    AllowAutoRedirect = false,
});
builder.Services.AddHttpClient("metadata", client => {
    client.DefaultRequestHeaders.Add("Metadata-Flavor", "Google");
    client.Timeout = TimeSpan.FromSeconds(5);
});

// Configure services for the Cloud Run identity token provider and the Country API proxy.
builder.Services.AddSingleton<ICloudRunIdentityTokenProvider>(serviceProvider =>
    new CloudRunIdentityTokenProvider(apiUri, serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("metadata")));
builder.Services.AddSingleton(serviceProvider =>
    new CountryApiProxy(
        serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("api"),
        serviceProvider.GetRequiredService<ICloudRunIdentityTokenProvider>(),
        serviceProvider.GetRequiredService<ILogger<CountryApiProxy>>()));

if (builder.Configuration["PORT"] is { } port) {
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapGet("/api/countries", (HttpContext context, CountryApiProxy proxy) =>
    proxy.ForwardAsync(context, "/countries"));
app.MapGet("/api/countries/{code}", (string code, HttpContext context, CountryApiProxy proxy) => {
    if (code.Length is < 1 or > 16 ||
        code.Any(character => !char.IsAsciiLetterOrDigit(character) && character != '-' && character != '_')) {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return Task.CompletedTask;
    }

    return proxy.ForwardAsync(context, $"/countries/{Uri.EscapeDataString(code)}");
});

// Fallback route for any other /api/* requests to return 404 Not Found.
app.MapFallback("/api/{**path}", (string? path) => Results.NotFound());
app.MapFallbackToFile("index.html");

app.Run();
