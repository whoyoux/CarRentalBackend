using CarRentalBackend.ModelsDto;

namespace CarRentalBackend.Services
{
    public interface ICarService
    {
        Task<List<CarDto>> GetAllCarsAsync();
        Task<CarDetailsDto?> GetCarDetailsAsync(int carId);
    }
}
