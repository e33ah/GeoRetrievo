using GeoRetrievo.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add CORS policy to allow requests from the React development server
builder.Services.AddCors(options => {
    options.AddPolicy("ReactDev", policy => {
        policy
            .WithOrigins("http://localhost:5173")
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

string filePath = Path.Combine(AppContext.BaseDirectory, "Data", "countries.json");
string json = File.ReadAllText(filePath);
var countries = CountryCatalog.FromJson(json);

app.MapGet("/countries", () => {
    var allCountries = countries.GetAll();
    return allCountries is null ? Results.NotFound() : Results.Ok(allCountries);
});

app.MapGet("/countries/{code}", (string code) => {
    var country = countries.GetByCode(code);
    return country is null ? Results.NotFound() : Results.Ok(country);
});

//string port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
//app.Run($"http://0.0.0.0:{port}");


app.Run();
