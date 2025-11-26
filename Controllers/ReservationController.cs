using CarRentalBackend.ModelsDto;
using CarRentalBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarRentalBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController(IReservationService reservationService) : ControllerBase
    {

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ReservationDto>> CreateReservation([FromBody] CreateReservationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("User is not authenticated.");
            }

            try
            {
                var reservation = await reservationService.CreateReservationAsync(userId, dto);
                if (reservation == null)
                {
                    return NotFound("Car not found.");
                }
                return Ok(reservation);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> CancelReservation(int id)
        {
            var userIdClaim = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("User is not authenticated.");
            }

            var success = await reservationService.CancelReservationAsync(id, userId);
            if (!success)
            {
                return NotFound("Reservation not found or you don't have permission to cancel it.");
            }

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/all")]
        public async Task<ActionResult<List<AdminReservationDto>>> GetAllReservations()
        {
            var reservations = await reservationService.GetAllReservationsAsync();
            return Ok(reservations);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("admin/{id}")]
        public async Task<ActionResult> AdminCancelReservation(int id)
        {
            var success = await reservationService.AdminCancelReservationAsync(id);
            if (!success)
            {
                return NotFound("Reservation not found.");
            }

            return NoContent();
        }
    }
}
