using CarRentalBackend.ModelsDto;
using CarRentalBackend.Services;
using Microsoft.AspNetCore.Authorization;
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

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<CarDto>> CreateCar([FromBody] CarDto carDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdCar = await carService.CreateCarAsync(carDto);
            return Ok(createdCar);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<CarDto>> UpdateCar(int id, [FromBody] CarDto carDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedCar = await carService.UpdateCarAsync(id, carDto);
            if (updatedCar == null)
            {
                return NotFound("Car not found.");
            }

            return Ok(updatedCar);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCar(int id)
        {
            var success = await carService.DeleteCarAsync(id);
            if (!success)
            {
                return NotFound("Car not found.");
            }

            return Ok(new { success = true });
        }
    }
}
