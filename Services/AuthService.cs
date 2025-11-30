using CarRentalBackend.Data;

using CarRentalBackend.Models;
using CarRentalBackend.ModelsDto;
using dotenv.net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CarRentalBackend.Services
{
    public class AuthService(DataContext context) : IAuthService
    {
        public async Task<LoginResponseDto?> LoginAsync(UserDto request)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());
            if (user is null)
            {
                return null;
            }

            var passwordVerificationResult = new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                return null;
            }

            var tokens = await CreateTokenResponse(user);

            return new LoginResponseDto
            {
                Id = user.Id.ToString(),
                AccessToken = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken,
                Email = user.Email,
                Role = user.Role,
            };
        }

        private async Task<TokenResponseDto> CreateTokenResponse(User user)
        {
            return new TokenResponseDto
            {
                AccessToken = CreateToken(user),
                RefreshToken = await GenerateAndSaveRefreshTokenAsync(user)
            };
        }

        public async Task<User?> RegisterAsync(RegisterUserDto request)
        {
            if (request.Password != request.ConfirmPassword)
            {
                return null;
            }


            if (await context.Users.AnyAsync(user => user.Email.ToLower() == request.Email.ToLower()))
            {
                return null;
            }

            var user = new User();

            var hashedPassword = new PasswordHasher<User>().HashPassword(user, request.Password);

            user.Email = request.Email;
            user.PasswordHash = hashedPassword;

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return user;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
        {
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await context.SaveChangesAsync();

            return refreshToken;
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim> {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role)
            };

            var envVars = DotEnv.Read();
            var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? (envVars.TryGetValue("JWT_ISSUER", out var issuerVal) ? issuerVal : throw new InvalidOperationException("JWT_ISSUER environment variable is not set"));
            var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? (envVars.TryGetValue("JWT_AUDIENCE", out var audienceVal) ? audienceVal : throw new InvalidOperationException("JWT_AUDIENCE environment variable is not set"));
            var jwt_key = Environment.GetEnvironmentVariable("JWT_KEY") ?? (envVars.TryGetValue("JWT_KEY", out var keyVal) ? keyVal : throw new InvalidOperationException("JWT_KEY environment variable is not set"));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt_key));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddDays(1),
                    signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        public async Task EnsureAdminAccountExistsAsync()
        {
            var envVars = DotEnv.Read();
            var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? (envVars.TryGetValue("ADMIN_EMAIL", out var emailVal) ? emailVal : "admin@carrental.com");
            var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? (envVars.TryGetValue("ADMIN_PASSWORD", out var passVal) ? passVal : "adminadmin");

            if (!await context.Users.AnyAsync(user => user.Email.ToLower() == adminEmail.ToLower()))
            {
                Console.WriteLine($"Creating admin account with email: {adminEmail}.");

                var adminUser = new User
                {
                    Email = adminEmail,
                    Role = "Admin"
                };

                var hashedPassword = new PasswordHasher<User>().HashPassword(adminUser, adminPassword);
                adminUser.PasswordHash = hashedPassword;

                context.Users.Add(adminUser);
                await context.SaveChangesAsync();
            }

            Console.WriteLine("Admin account has been already created.");
        }

        private async Task<User?> ValidateRefreshTokenAsync(Guid userId, string refreshToken)
        {
            var user = await context.Users.FirstAsync(u => u.Id == userId);
            if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return null;
            }

            return user;
        }

        public async Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto request)
        {
            var user = await ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);

            if (user is null)
            {
                return null;
            }

            return await CreateTokenResponse(user);
        }
    }
}
