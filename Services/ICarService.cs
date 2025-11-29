using CarRentalBackend.ModelsDto;

namespace CarRentalBackend.Services
{
    public interface ICarService
    {
        Task<List<CarDto>> GetAllCarsAsync();
        Task<CarDetailsDto?> GetCarDetailsAsync(int carId);
        Task<CarDto?> UpdateCarAsync(int carId, CarDto carDto);
        Task<CarDto> CreateCarAsync(CarDto carDto);
        Task<bool> DeleteCarAsync(int carId);
    }
}
