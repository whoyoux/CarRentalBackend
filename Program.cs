using CarRentalBackend.Data;
using CarRentalBackend.Middleware;
using CarRentalBackend.Services;
using dotenv.net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using System.Text;

// Load .env file only if it exists (for local development)
// In Docker, environment variables are passed directly via docker-compose
if (File.Exists(".env"))
{
    DotEnv.Load();
}

var builder = WebApplication.CreateBuilder(args);

// Konfiguracja Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.File("logs/carrental-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

var envVars = DotEnv.Read();

static string GetEnvVar(string key, IDictionary<string, string> envVars)
{
    return Environment.GetEnvironmentVariable(key) ?? (envVars.TryGetValue(key, out var value) ? value : throw new InvalidOperationException($"{key} environment variable is not set"));
}

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<DataContext>(options =>
{
    var connectionString = GetEnvVar("DB_CONNECTION", envVars);
    options.UseSqlServer(connectionString);
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var issuer = GetEnvVar("JWT_ISSUER", envVars);
        var audience = GetEnvVar("JWT_AUDIENCE", envVars);
        var jwt_key = GetEnvVar("JWT_KEY", envVars);

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt_key)),
            ValidateIssuerSigningKey = true
        };
    });

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICarService, CarService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var frontendUrl = GetEnvVar("FRONTEND_URL", envVars);
        policy.WithOrigins(frontendUrl)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors("AllowFrontend");

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
    try
    {
        await dataContext.Database.MigrateAsync();
        Log.Information("Database migrations applied successfully.");

        await EnsureStoredProceduresExistAsync(dataContext);
        await EnsureTriggerExistsAsync(dataContext);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error applying database migrations.");
        throw;
    }

    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
    await authService.EnsureAdminAccountExistsAsync();

    await DataSeeder.SeedDataAsync(dataContext);
}

static async Task EnsureStoredProceduresExistAsync(DataContext context)
{
    try
    {
        var procedureExists = await context.Database
            .SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetMonthlyRevenue]') AND type in (N'P', N'PC')")
            .FirstOrDefaultAsync();

        if (procedureExists == 0)
        {
            Log.Information("Creating GetMonthlyRevenue stored procedure...");
            await context.Database.ExecuteSqlRawAsync(@"
                CREATE PROCEDURE [dbo].[GetMonthlyRevenue]
                    @Year INT = NULL,
                    @Month INT = NULL
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    IF @Year IS NULL SET @Year = YEAR(GETDATE());
                    IF @Month IS NULL SET @Month = MONTH(GETDATE());
                    
                    SELECT 
                        YEAR(r.StartDateTime) AS Year,
                        MONTH(r.StartDateTime) AS Month,
                        COUNT(r.Id) AS TotalReservations,
                        SUM(r.TotalPrice) AS TotalRevenue,
                        AVG(r.TotalPrice) AS AverageReservationValue
                    FROM Reservations r
                    WHERE YEAR(r.StartDateTime) = @Year 
                        AND MONTH(r.StartDateTime) = @Month
                    GROUP BY YEAR(r.StartDateTime), MONTH(r.StartDateTime);
                END
            ");
            Log.Information("GetMonthlyRevenue stored procedure created successfully.");
        }

        var userHistoryExists = await context.Database
            .SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetUserReservationHistory]') AND type in (N'P', N'PC')")
            .FirstOrDefaultAsync();

        if (userHistoryExists == 0)
        {
            Log.Information("Creating GetUserReservationHistory stored procedure...");
            await context.Database.ExecuteSqlRawAsync(@"
                CREATE PROCEDURE [dbo].[GetUserReservationHistory]
                    @UserId UNIQUEIDENTIFIER
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    SELECT 
                        r.Id,
                        r.CarId,
                        c.Brand,
                        c.Model,
                        r.StartDateTime,
                        r.EndDateTime,
                        r.TotalPrice,
                        r.CreatedAt,
                        CASE 
                            WHEN r.EndDateTime < GETDATE() THEN 'Completed'
                            WHEN r.StartDateTime > GETDATE() THEN 'Upcoming'
                            ELSE 'Active'
                        END AS Status
                    FROM Reservations r
                    INNER JOIN Cars c ON r.CarId = c.Id
                    WHERE r.UserId = @UserId
                    ORDER BY r.StartDateTime DESC;
                END
            ");
            Log.Information("GetUserReservationHistory stored procedure created successfully.");
        }

        var functionExists = await context.Database
            .SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CalculateDiscount]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT')")
            .FirstOrDefaultAsync();

        if (functionExists == 0)
        {
            Log.Information("Creating CalculateDiscount function...");
            await context.Database.ExecuteSqlRawAsync(@"
                CREATE FUNCTION [dbo].[CalculateDiscount](@UserId UNIQUEIDENTIFIER)
                RETURNS DECIMAL(5,2)
                AS
                BEGIN
                    DECLARE @ReservationCount INT;
                    DECLARE @Discount DECIMAL(5,2) = 0;
                    
                    SELECT @ReservationCount = COUNT(*)
                    FROM Reservations
                    WHERE UserId = @UserId;
                    
                    IF @ReservationCount >= 10
                        SET @Discount = 10.00;
                    ELSE IF @ReservationCount >= 5
                        SET @Discount = 5.00;
                    
                    RETURN @Discount;
                END
            ");
            Log.Information("CalculateDiscount function created successfully.");
        }
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Error ensuring stored procedures exist. They may already exist or there was a connection issue.");
    }
}

static async Task EnsureTriggerExistsAsync(DataContext context)
{
    try
    {
        var triggerExists = await context.Database
            .SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM sys.triggers WHERE name = 'LogReservationDelete'")
            .FirstOrDefaultAsync();

        if (triggerExists == 0)
        {
            Log.Information("Creating LogReservationDelete trigger...");

            await context.Database.ExecuteSqlRawAsync(@"
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'LogReservationDelete')
                    DROP TRIGGER [dbo].[LogReservationDelete];
            ");

            await context.Database.ExecuteSqlRawAsync(@"
                CREATE TRIGGER [dbo].[LogReservationDelete]
                ON [dbo].[Reservations]
                AFTER DELETE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    INSERT INTO [dbo].[ReservationLogs] (ReservationId, UserId, Action, LogDate)
                    SELECT 
                        d.Id,
                        d.UserId,
                        'Deleted',
                        GETDATE()
                    FROM deleted d;
                END
            ");
            Log.Information("LogReservationDelete trigger created successfully.");
        }
        else
        {
            Log.Information("LogReservationDelete trigger already exists.");
        }
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Error ensuring trigger exists. It may already exist or there was a connection issue.");
    }
}

app.Run();
