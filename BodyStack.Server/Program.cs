using Microsoft.EntityFrameworkCore;
using BodyStack.Server.Domain.Exceptions;
using BodyStack.Server.Integrations.Fitatu;
using Microsoft.Extensions.Options;
using BodyStack.Server.Infrastructure.Background;
using BodyStack.Server.Application.Fitatu;
using BodyStack.Server.Infrastructure.Security;
using BodyStack.Server.Infrastructure.Persistence;
using BodyStack.Server.Application.Suunto;
using BodyStack.Server.Integrations.Suunto;
using BodyStack.Server.Domain.Services;
using BodyStack.Server.Security;

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

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

        builder.Services.AddScoped<ITokenProtector, TokenProtector>();
        builder.Services.AddScoped<IFitatuSessionRepository, FitatuSessionRepository>();
        builder.Services.AddScoped<IMonthDaySummaryRepository, MonthDaySummaryRepository>();
        builder.Services.AddScoped<FitatuLoginUseCase>();
        builder.Services.AddScoped<FitatuGetDayUseCase>();
        builder.Services.AddScoped<FitatuStartMonthRecalculationUseCase>();
        builder.Services.AddScoped<FitatuExportDayCsvUseCase>();
        builder.Services.AddScoped<FitatuExportMonthCsvUseCase>();

        builder.Services.AddSingleton<Domain.Fitatu.FitatuDayPlanTotalsCalculator>();

        builder.Services.AddSingleton<IBackgroundTaskQueue<FitatuMonthRecalculationRequest>, BackgroundTaskQueue<FitatuMonthRecalculationRequest>>();
        builder.Services.AddHostedService<FitatuMonthRecalculationWorker>();

        builder.Services.AddOptions<FitatuOptions>()
            .Bind(builder.Configuration.GetSection("Fitatu"))
            .Validate(o => !string.IsNullOrWhiteSpace(o.BaseUrl), "Fitatu:BaseUrl is required")
            .Validate(o => !string.IsNullOrWhiteSpace(o.ApiKey), "Fitatu:ApiKey is required")
            .ValidateOnStart();

        builder.Services.AddHttpClient<IFitatuClient, FitatuClient>((sp, httpClient) =>
        {
            var options = sp.GetRequiredService<IOptions<FitatuOptions>>().Value;
            httpClient.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
        });

        builder.Services.AddSingleton<JwtParser>();

        builder.Services.AddHttpClient<ISuuntoActivityExportClient, SuuntoActivityExportClient>(httpClient =>
        {
            httpClient.BaseAddress = new Uri("https://247.sports-tracker.com", UriKind.Absolute);
        });

        builder.Services.AddHttpClient<ISuuntoSleepExportClient, SuuntoSleepExportClient>(httpClient =>
        {
            httpClient.BaseAddress = new Uri("https://247.sports-tracker.com", UriKind.Absolute);
        });

        builder.Services.AddScoped<SuuntoGetDailyActivitySummaryUseCase>();
        builder.Services.AddScoped<SuuntoGetDailySleepSummaryUseCase>();
        
        // Suunto Workouts & BMR
        builder.Services.AddHttpClient<ISuuntoWorkoutClient, SuuntoWorkoutClient>();
        builder.Services.AddScoped<SuuntoGetWorkoutsUseCase>();
        builder.Services.AddScoped<SuuntoGetDailySummaryUseCase>();
        builder.Services.AddSingleton<BmrCalculator>();
        
        // Suunto User Profile
        builder.Services.AddHttpClient<ISuuntoUserClient, SuuntoUserClient>();
        builder.Services.AddScoped<SuuntoGetUserProfileUseCase>();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
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
                IFitatuSessionRepository repository,
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
                FitatuLoginUseCase useCase,
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
                IFitatuSessionRepository repository,
                CancellationToken cancellationToken) =>
            {
                await repository.ClearAsync(cancellationToken);
                return TypedResults.Ok(new { status = "ok" });
            })
            .WithName("FitatuLogout");

        app.MapGet("/api/fitatu/day/{date}", async (
                string date,
                FitatuGetDayUseCase useCase,
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
                FitatuStartMonthRecalculationUseCase useCase,
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
                IFitatuSessionRepository sessionRepository,
                IMonthDaySummaryRepository summaryRepository,
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
                FitatuExportDayCsvUseCase useCase,
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
                FitatuExportMonthCsvUseCase useCase,
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
                SuuntoGetDailyActivitySummaryUseCase useCase,
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
                SuuntoGetDailySleepSummaryUseCase useCase,
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

        // Suunto Workouts Endpoint
        app.MapGet("/api/suunto/workouts", async (
                HttpRequest request,
                string? from,
                string? to,
                int? ttlMinutes,
                SuuntoGetWorkoutsUseCase useCase,
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

                try
                {
                    var result = await useCase.ExecuteAsync(authHeader.ToString(), ttl, fromDate, toDate, cancellationToken);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            })
            .WithName("SuuntoWorkouts");

        // Suunto Daily Summary Endpoint
        app.MapGet("/api/suunto/daily-summary", async (
                HttpRequest request,
                string date,
                double weightKg,
                double heightCm,
                int age,
                string gender,
                int? ttlMinutes,
                SuuntoGetDailySummaryUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                if (!request.Headers.TryGetValue("sttauthorization", out var authHeader) || string.IsNullOrWhiteSpace(authHeader.ToString()))
                {
                    return Results.Unauthorized();
                }

                if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", out var parsedDate))
                {
                    return Results.BadRequest(new { error = "Invalid date format. Expected yyyy-MM-dd." });
                }

                if (weightKg <= 0 || heightCm <= 0 || age <= 0)
                {
                    return Results.BadRequest(new { error = "Weight, height, and age must be positive values." });
                }

                var ttl = TimeSpan.FromMinutes(ttlMinutes.GetValueOrDefault(15));
                if (ttl < TimeSpan.FromMinutes(1)) ttl = TimeSpan.FromMinutes(1);
                if (ttl > TimeSpan.FromHours(24)) ttl = TimeSpan.FromHours(24);

                try
                {
                    var result = await useCase.ExecuteAsync(
                        authHeader.ToString(),
                        parsedDate,
                        weightKg,
                        heightCm,
                        age,
                        gender,
                        ttl,
                        cancellationToken);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            })
            .WithName("SuuntoDailySummary");

        // Suunto User Profile Endpoint
        app.MapGet("/api/suunto/user/profile", async (
                HttpRequest request,
                SuuntoGetUserProfileUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                if (!request.Headers.TryGetValue("sttauthorization", out var authHeader) || string.IsNullOrWhiteSpace(authHeader.ToString()))
                {
                    return Results.Unauthorized();
                }

                try
                {
                    var profile = await useCase.ExecuteAsync(authHeader.ToString(), cancellationToken);
                    return Results.Ok(profile);
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            })
            .WithName("SuuntoUserProfile");

        app.Run();
    }
}
