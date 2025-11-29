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

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

// Konfiguracja Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.File("logs/carrental-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

var envVars = DotEnv.Read();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(envVars["DB_CONNECTION"]);
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var issuer = envVars["JWT_ISSUER"];
        var audience = envVars["JWT_AUDIENCE"];
        var jwt_key = envVars["JWT_KEY"];

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
        policy.WithOrigins(envVars["FRONTEND_URL"])
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

// Globalna obsługa błędów - musi być przed UseAuthorization
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors("AllowFrontend");

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    // Apply pending migrations
    var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
    try
    {
        await dataContext.Database.MigrateAsync();
        Log.Information("Database migrations applied successfully.");
        
        // Ensure stored procedures exist
        await EnsureStoredProceduresExistAsync(dataContext);
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
        // Check if GetMonthlyRevenue procedure exists
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

        // Check if GetUserReservationHistory procedure exists
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

        // Check if CalculateDiscount function exists
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

app.Run();
