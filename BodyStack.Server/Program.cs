using Microsoft.EntityFrameworkCore;
using BodyStack.Server.Domain.Exceptions;

namespace BodyStack.Server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            ContentRootPath = AppContext.BaseDirectory
        });

        // Add services to the container.
        builder.Services.AddAuthorization();

        builder.Services.AddMemoryCache();

        builder.Services.AddSignalR();

        builder.Services.AddDataProtection();

        builder.Services.AddDbContext<Infrastructure.Persistence.AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

        builder.Services.AddScoped<Infrastructure.Security.ITokenProtector, Infrastructure.Security.TokenProtector>();
        builder.Services.AddScoped<Application.Fitatu.IFitatuSessionRepository, Infrastructure.Persistence.FitatuSessionRepository>();
        builder.Services.AddScoped<Application.Fitatu.IMonthDaySummaryRepository, Infrastructure.Persistence.MonthDaySummaryRepository>();
        builder.Services.AddScoped<Application.Fitatu.FitatuLoginUseCase>();
        builder.Services.AddScoped<Application.Fitatu.FitatuGetDayUseCase>();
        builder.Services.AddScoped<Application.Fitatu.FitatuStartMonthRecalculationUseCase>();
        builder.Services.AddScoped<Application.Fitatu.FitatuExportDayCsvUseCase>();
        builder.Services.AddScoped<Application.Fitatu.FitatuExportMonthCsvUseCase>();

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

        builder.Services.AddHttpClient<Integrations.Suunto.ISuuntoActivityExportClient, Integrations.Suunto.SuuntoActivityExportClient>(httpClient =>
        {
            httpClient.BaseAddress = new Uri("https://247.sports-tracker.com", UriKind.Absolute);
        });

        builder.Services.AddHttpClient<Integrations.Suunto.ISuuntoSleepExportClient, Integrations.Suunto.SuuntoSleepExportClient>(httpClient =>
        {
            httpClient.BaseAddress = new Uri("https://247.sports-tracker.com", UriKind.Absolute);
        });

        builder.Services.AddScoped<Application.Suunto.SuuntoGetDailyActivitySummaryUseCase>();
        builder.Services.AddScoped<Application.Suunto.SuuntoGetDailySleepSummaryUseCase>();

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

        app.MapGet("/api/fitatu/session", async (
                Application.Fitatu.IFitatuSessionRepository repository,
                CancellationToken cancellationToken) =>
            {
                var session = await repository.GetLatestAsync(cancellationToken);
                if (session is null)
                {
                    return Results.Unauthorized();
                }

                return Results.Ok(new
                {
                    fitatuUserId = session.FitatuUserId,
                    updatedAt = session.UpdatedAt
                });
            })
            .WithName("FitatuSession");

        app.MapPost("/api/fitatu/login", async (
                Api.Fitatu.FitatuLoginRequest request,
                Application.Fitatu.FitatuLoginUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    await useCase.ExecuteAsync(request.Username, request.Password, cancellationToken);
                    return TypedResults.Ok(new Api.Fitatu.FitatuLoginResponse("ok"));
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            })
            .WithName("FitatuLogin");

        app.MapPost("/api/fitatu/logout", async (
                Application.Fitatu.IFitatuSessionRepository repository,
                CancellationToken cancellationToken) =>
            {
                await repository.ClearAsync(cancellationToken);
                return TypedResults.Ok(new { status = "ok" });
            })
            .WithName("FitatuLogout");

        app.MapGet("/api/fitatu/day/{date}", async (
                string date,
                Application.Fitatu.FitatuGetDayUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", out var day))
                {
                    return Results.BadRequest(new { error = "Invalid date format. Expected yyyy-MM-dd." });
                }

                try
                {
                    var computed = await useCase.ExecuteAsync(day, cancellationToken);

                    var response = new Api.Fitatu.FitatuDayResponse(
                        Date: day.ToString("yyyy-MM-dd"),
                        Totals: Api.Fitatu.FitatuTotals.From(computed.Totals),
                        Meals: computed.Meals.Select(Api.Fitatu.FitatuMealTotals.From).ToArray());

                    return Results.Ok(response);
                }
                catch (FitatuSessionNotFoundException)
                {
                    return Results.Unauthorized();
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            })
            .WithName("FitatuGetDay");

        app.MapPost("/api/fitatu/month/{yearMonth}/recalculate", async (
                string yearMonth,
                Application.Fitatu.FitatuStartMonthRecalculationUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    await useCase.ExecuteAsync(yearMonth, cancellationToken);
                    return Results.Accepted();
                }
                catch (FitatuSessionNotFoundException)
                {
                    return Results.Unauthorized();
                }
                catch (MonthExportIncompleteException ex)
                {
                    return Results.Conflict(new { error = ex.Message, missingDays = ex.MissingDays });
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            })
            .WithName("FitatuRecalculateMonth");

        app.MapGet("/api/fitatu/month/{yearMonth}/statuses", async (
                string yearMonth,
                Application.Fitatu.IFitatuSessionRepository sessionRepository,
                Application.Fitatu.IMonthDaySummaryRepository summaryRepository,
                CancellationToken cancellationToken) =>
            {
                var session = await sessionRepository.GetLatestAsync(cancellationToken);
                if (session is null)
                {
                    return Results.Unauthorized();
                }

                try
                {
                    var summaries = await summaryRepository.GetByYearMonthAsync(session.FitatuUserId, yearMonth, cancellationToken);
                    var statuses = summaries.Select(s => new
                    {
                        date = s.Date,
                        status = s.Status.ToLowerInvariant(),
                        energy = s.Energy,
                        protein = s.Protein,
                        fat = s.Fat,
                        carbohydrate = s.Carbohydrate,
                        fiber = s.Fiber,
                        sugars = s.Sugars,
                        salt = s.Salt
                    }).ToList();

                    return Results.Ok(new { statuses });
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            })
            .WithName("FitatuGetMonthStatuses");

        app.MapGet("/api/fitatu/export/day/{date}", async (
                string date,
                Application.Fitatu.FitatuExportDayCsvUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", out var day))
                {
                    return Results.BadRequest(new { error = "Invalid date format. Expected yyyy-MM-dd." });
                }

                try
                {
                    var csv = await useCase.ExecuteAsync(day, cancellationToken);
                    return Results.Text(csv, "text/csv");
                }
                catch (FitatuSessionNotFoundException)
                {
                    return Results.Unauthorized();
                }
            })
            .WithName("FitatuExportDayCsv");

        app.MapGet("/api/fitatu/export/month/{yearMonth}", async (
                string yearMonth,
                Application.Fitatu.FitatuExportMonthCsvUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    var csv = await useCase.ExecuteAsync(yearMonth, cancellationToken);
                    return Results.Text(csv, "text/csv");
                }
                catch (MonthExportIncompleteException ex)
                {
                    return Results.Conflict(new { error = ex.Message, missingDays = ex.MissingDays });
                }
                catch (FitatuSessionNotFoundException)
                {
                    return Results.Unauthorized();
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            })
            .WithName("FitatuExportMonthCsv");

        app.MapFallbackToFile("/index.html");

        app.MapGet("/api/health", () => TypedResults.Ok(new { status = "ok" }))
        .WithName("GetHealth");

        app.MapGet("/api/suunto/activity/daily", async (
                HttpRequest request,
                string? from,
                string? to,
                int? ttlMinutes,
                Application.Suunto.SuuntoGetDailyActivitySummaryUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                if (!request.Headers.TryGetValue("sttauthorization", out var authHeader) || string.IsNullOrWhiteSpace(authHeader.ToString()))
                {
                    return Results.Unauthorized();
                }

                DateOnly? fromDate = null;
                DateOnly? toDate = null;

                if (!string.IsNullOrWhiteSpace(from))
                {
                    if (!DateOnly.TryParseExact(from, "yyyy-MM-dd", out var parsedFrom))
                    {
                        return Results.BadRequest(new { error = "Invalid from date format. Expected yyyy-MM-dd." });
                    }
                    fromDate = parsedFrom;
                }

                if (!string.IsNullOrWhiteSpace(to))
                {
                    if (!DateOnly.TryParseExact(to, "yyyy-MM-dd", out var parsedTo))
                    {
                        return Results.BadRequest(new { error = "Invalid to date format. Expected yyyy-MM-dd." });
                    }
                    toDate = parsedTo;
                }

                var ttl = TimeSpan.FromMinutes(ttlMinutes.GetValueOrDefault(15));
                if (ttl < TimeSpan.FromMinutes(1)) ttl = TimeSpan.FromMinutes(1);
                if (ttl > TimeSpan.FromHours(24)) ttl = TimeSpan.FromHours(24);

                var days = await useCase.ExecuteAsync(authHeader.ToString(), ttl, fromDate, toDate, cancellationToken);

                var responseDays = days
                    .Select(d => new Api.Suunto.SuuntoDailyActivityResponse(d.Date, d.Steps, d.EnergyConsumption, d.AvgHr, d.AvgHrv, d.Samples))
                    .ToArray();

                var response = new Api.Suunto.SuuntoDailyActivitySummaryResponse(
                    Days: responseDays,
                    TotalSteps: responseDays.Sum(d => d.Steps),
                    TotalEnergyConsumption: responseDays.Sum(d => d.EnergyConsumption));

                return Results.Ok(response);
            })
            .WithName("SuuntoActivityDaily");

        app.MapGet("/api/suunto/sleep/daily", async (
                HttpRequest request,
                string? from,
                string? to,
                int? ttlMinutes,
                Application.Suunto.SuuntoGetDailySleepSummaryUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                if (!request.Headers.TryGetValue("sttauthorization", out var authHeader) || string.IsNullOrWhiteSpace(authHeader.ToString()))
                {
                    return Results.Unauthorized();
                }

                DateOnly? fromDate = null;
                DateOnly? toDate = null;

                if (!string.IsNullOrWhiteSpace(from))
                {
                    if (!DateOnly.TryParseExact(from, "yyyy-MM-dd", out var parsedFrom))
                    {
                        return Results.BadRequest(new { error = "Invalid from date format. Expected yyyy-MM-dd." });
                    }
                    fromDate = parsedFrom;
                }

                if (!string.IsNullOrWhiteSpace(to))
                {
                    if (!DateOnly.TryParseExact(to, "yyyy-MM-dd", out var parsedTo))
                    {
                        return Results.BadRequest(new { error = "Invalid to date format. Expected yyyy-MM-dd." });
                    }
                    toDate = parsedTo;
                }

                var ttl = TimeSpan.FromMinutes(ttlMinutes.GetValueOrDefault(15));
                if (ttl < TimeSpan.FromMinutes(1)) ttl = TimeSpan.FromMinutes(1);
                if (ttl > TimeSpan.FromHours(24)) ttl = TimeSpan.FromHours(24);

                var days = await useCase.ExecuteAsync(authHeader.ToString(), ttl, fromDate, toDate, cancellationToken);

                var responseDays = days
                    .Select(d => new Api.Suunto.SuuntoDailySleepResponse(
                        d.Date,
                        d.TotalSleepSeconds,
                        d.NightSleepSeconds,
                        d.NapSleepSeconds,
                        d.SleepSessionsCount,
                        d.NapSessionsCount))
                    .ToArray();

                var response = new Api.Suunto.SuuntoDailySleepSummaryResponse(
                    Days: responseDays,
                    TotalSleepSeconds: responseDays.Sum(d => d.TotalSleepSeconds));

                return Results.Ok(response);
            })
            .WithName("SuuntoSleepDaily");

        app.Run();
    }
}
