using GeoRetrievo.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure the application to listen on the port specified in the PORT environment variable
if (builder.Configuration["PORT"] is { } port) {
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddSingleton<ICountryDataSource, GoogleCloudStorageCountryDataSource>();
builder.Services.AddSingleton<CountryCatalogLoader>();

// Add CORS policy to allow requests from the React development server
builder.Services.AddCors(options => {
    options.AddPolicy("ReactDev", policy => {
        policy
            .WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                    ?? ["http://localhost:5173"])
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Enable CORS for the React development server
app.UseCors("ReactDev");

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
});

app.MapGet("/countries/{code}", (string code) => {
    var country = countries.GetByCode(code);
    return country is null ? Results.NotFound() : Results.Ok(country);
});

app.Run();
