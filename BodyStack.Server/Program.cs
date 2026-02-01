
namespace BodyStack.Server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();

        builder.Services.AddOptions<Integrations.Fitatu.FitatuOptions>()
            .Bind(builder.Configuration.GetSection("Fitatu"))
            .Validate(o => !string.IsNullOrWhiteSpace(o.BaseUrl), "Fitatu:BaseUrl is required")
            .Validate(o => !string.IsNullOrWhiteSpace(o.ApiKey), "Fitatu:ApiKey is required")
            .ValidateOnStart();

        builder.Services.AddHttpClient<Integrations.Fitatu.IFitatuClient, Integrations.Fitatu.FitatuClient>((sp, httpClient) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<Integrations.Fitatu.FitatuOptions>>().Value;
            httpClient.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
        });

        builder.Services.AddSingleton<Security.JwtParser>();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        app.UseDefaultFiles();
        app.MapStaticAssets();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        app.MapGet("/weatherforecast", (HttpContext httpContext) =>
        {
            var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = summaries[Random.Shared.Next(summaries.Length)]
                })
                .ToArray();
            return forecast;
        })
        .WithName("GetWeatherForecast");

        app.MapFallbackToFile("/index.html");

        app.MapGet("/api/health", () => TypedResults.Ok(new { status = "ok" }))
        .WithName("GetHealth");

        app.Run();
    }
}
