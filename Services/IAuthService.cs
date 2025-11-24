using CarRentalBackend.Models;
using CarRentalBackend.ModelsDto;

namespace CarRentalBackend.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(RegisterUserDto request);
        Task<LoginResponseDto?> LoginAsync(UserDto request);
        Task EnsureAdminAccountExistsAsync();
        Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto request);
    }
}
