using CarRentalBackend.ModelsDto;
using CarRentalBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarController(ICarService carService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<CarDto>>> GetAllCars()
        {
            var cars = await carService.GetAllCarsAsync();
            return Ok(cars);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CarDetailsDto>> GetCarDetails(int id)
        {
            var car = await carService.GetCarDetailsAsync(id);
            if (car == null)
            {
                return NotFound();
            }
            return Ok(car);
        }
    }
}
