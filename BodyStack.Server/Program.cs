using Microsoft.EntityFrameworkCore;

namespace BodyStack.Server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();

        builder.Services.AddSignalR();

        builder.Services.AddDataProtection();

        builder.Services.AddDbContext<Infrastructure.Persistence.AppDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

        builder.Services.AddScoped<Infrastructure.Security.ITokenProtector, Infrastructure.Security.TokenProtector>();
        builder.Services.AddScoped<Application.Fitatu.IFitatuSessionRepository, Infrastructure.Persistence.FitatuSessionRepository>();
        builder.Services.AddScoped<Application.Fitatu.FitatuLoginUseCase>();
        builder.Services.AddScoped<Application.Fitatu.FitatuGetDayUseCase>();
        builder.Services.AddScoped<Application.Fitatu.FitatuStartMonthRecalculationUseCase>();

        builder.Services.AddSingleton<Domain.Fitatu.FitatuDayPlanTotalsCalculator>();

        builder.Services.AddSingleton<Infrastructure.Background.IBackgroundTaskQueue<Application.Fitatu.FitatuMonthRecalculationRequest>, Infrastructure.Background.BackgroundTaskQueue<Application.Fitatu.FitatuMonthRecalculationRequest>>();
        builder.Services.AddHostedService<Infrastructure.Background.FitatuMonthRecalculationWorker>();

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

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<Infrastructure.Persistence.AppDbContext>();
            db.Database.EnsureCreated();
        }

        app.UseDefaultFiles();
        app.MapStaticAssets();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapHub<Realtime.FitatuMonthHub>("/hubs/fitatu-month");

        app.MapPost("/api/fitatu/login", async (
                Api.Fitatu.FitatuLoginRequest request,
                Application.Fitatu.FitatuLoginUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                await useCase.ExecuteAsync(request.Username, request.Password, cancellationToken);
                return TypedResults.Ok(new Api.Fitatu.FitatuLoginResponse("ok"));
            })
            .WithName("FitatuLogin");

        app.MapGet("/api/fitatu/day/{date}", async (
                string date,
                Application.Fitatu.FitatuGetDayUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", out var day))
                {
                    return Results.BadRequest(new { error = "Invalid date format. Expected yyyy-MM-dd." });
                }

                var computed = await useCase.ExecuteAsync(day, cancellationToken);

                var response = new Api.Fitatu.FitatuDayResponse(
                    Date: day.ToString("yyyy-MM-dd"),
                    Totals: Api.Fitatu.FitatuTotals.From(computed.Totals),
                    Meals: computed.Meals.Select(Api.Fitatu.FitatuMealTotals.From).ToArray());

                return Results.Ok(response);
            })
            .WithName("FitatuGetDay");

        app.MapPost("/api/fitatu/month/{yearMonth}/recalculate", async (
                string yearMonth,
                Application.Fitatu.FitatuStartMonthRecalculationUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                await useCase.ExecuteAsync(yearMonth, cancellationToken);
                return Results.Accepted();
            })
            .WithName("FitatuRecalculateMonth");

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
