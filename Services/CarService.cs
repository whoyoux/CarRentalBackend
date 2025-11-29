using CarRentalBackend.Data;
using CarRentalBackend.ModelsDto;
using Microsoft.EntityFrameworkCore;

namespace CarRentalBackend.Services
{
    public class CarService(DataContext context) : ICarService
    {

        public async Task<List<CarDto>> GetAllCarsAsync()
        {
            return await context.Cars
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
            var car = await context.Cars
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
                    Reservations = context.Reservations
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

        public async Task<CarDto?> UpdateCarAsync(int carId, CarDto carDto)
        {
            var car = await context.Cars.FindAsync(carId);
            if (car == null)
            {
                return null;
            }

            car.Brand = carDto.Brand;
            car.Model = carDto.Model;
            car.Year = carDto.Year;
            car.PricePerDay = carDto.PricePerDay;
            car.Description = carDto.Description;
            car.ImageUrl = carDto.ImageUrl;

            await context.SaveChangesAsync();

            return new CarDto
            {
                Id = car.Id,
                Brand = car.Brand,
                Model = car.Model,
                Year = car.Year,
                PricePerDay = car.PricePerDay,
                Description = car.Description,
                ImageUrl = car.ImageUrl
            };
        }

        public async Task<CarDto> CreateCarAsync(CarDto carDto)
        {
            var car = new Models.Car
            {
                Brand = carDto.Brand,
                Model = carDto.Model,
                Year = carDto.Year,
                PricePerDay = carDto.PricePerDay,
                Description = carDto.Description,
                ImageUrl = carDto.ImageUrl
            };

            context.Cars.Add(car);
            await context.SaveChangesAsync();

            return new CarDto
            {
                Id = car.Id,
                Brand = car.Brand,
                Model = car.Model,
                Year = car.Year,
                PricePerDay = car.PricePerDay,
                Description = car.Description,
                ImageUrl = car.ImageUrl
            };
        }

        public async Task<bool> DeleteCarAsync(int carId)
        {
            var car = await context.Cars.FindAsync(carId);
            if (car == null)
            {
                return false;
            }

            context.Cars.Remove(car);
            await context.SaveChangesAsync();

            return true;
        }
    }
}
