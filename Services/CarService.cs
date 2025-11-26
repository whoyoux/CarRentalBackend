using CarRentalBackend.Data;
using CarRentalBackend.ModelsDto;
using Microsoft.EntityFrameworkCore;

namespace CarRentalBackend.Services
{
    public class CarService : ICarService
    {
        private readonly DataContext _context;

        public CarService(DataContext context)
        {
            _context = context;
        }

        public async Task<List<CarDto>> GetAllCarsAsync()
        {
            return await _context.Cars
                .Select(c => new CarDto
                {
                    Id = c.Id,
                    Brand = c.Brand,
                    Model = c.Model,
                    Year = c.Year,
                    PricePerDay = c.PricePerDay,
                    Description = c.Description,
                    ImageUrl = c.ImageUrl
                })
                .ToListAsync();
        }

        public async Task<CarDetailsDto?> GetCarDetailsAsync(int carId)
        {
            var car = await _context.Cars
                .Where(c => c.Id == carId)
                .Select(c => new CarDetailsDto
                {
                    Id = c.Id,
                    Brand = c.Brand,
                    Model = c.Model,
                    Year = c.Year,
                    PricePerDay = c.PricePerDay,
                    Description = c.Description,
                    ImageUrl = c.ImageUrl,
                    Reservations = _context.Reservations
                        .Where(r => r.CarId == carId && r.EndDateTime > DateTime.UtcNow)
                        .OrderBy(r => r.StartDateTime)
                        .Select(r => new ReservationPeriodDto
                        {
                            StartDateTime = r.StartDateTime,
                            EndDateTime = r.EndDateTime
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            return car;
        }
    }
}
