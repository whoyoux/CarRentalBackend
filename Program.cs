using CarRentalBackend.Data;
using CarRentalBackend.Services;
using dotenv.net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

var envVars = DotEnv.Read();
Console.WriteLine("Environment Variables Loaded:");
foreach (var kvp in envVars)
{
    Console.WriteLine($"{kvp.Key}={kvp.Value}");
}
Console.WriteLine("-----------------------------");

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(envVars["DB_CONNECTION"]);
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = envVars["JWT_ISSUER"],
            ValidateAudience = true,
            ValidAudience = envVars["JWT_AUDIENCE"],
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(envVars["JWT_KEY"])),
            ValidateIssuerSigningKey = true
        };
    });

builder.Services.AddScoped<IAuthService, AuthService>();

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

app.UseAuthorization();

app.UseCors("AllowFrontend");

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
    await authService.EnsureAdminAccountExistsAsync();
}

app.Run();
