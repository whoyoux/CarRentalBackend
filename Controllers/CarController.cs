using CarRentalBackend.Data;
using CarRentalBackend.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarController : ControllerBase
    {
        private readonly DataContext _context;

        public CarController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Car>>> GetAllCars()
        {
            var cars = await _context.Cars.ToListAsync();
            return Ok(cars);
        }
    }
}
