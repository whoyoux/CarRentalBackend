using CarRentalBackend.ModelsDto;
using CarRentalBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarRentalBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController(IReviewService reviewService) : ControllerBase
    {
        [HttpGet("car/{carId}")]
        public async Task<ActionResult<List<ReviewDto>>> GetReviewsByCar(int carId)
        {
            var reviews = await reviewService.GetReviewsByCarIdAsync(carId);
            return Ok(reviews);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ReviewDto>> CreateReview([FromBody] CreateReviewDto dto)
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
                var review = await reviewService.CreateReviewAsync(userId, dto);
                if (review == null)
                {
                    return NotFound("Car not found.");
                }
                return Ok(review);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<ReviewDto>> UpdateReview(int id, [FromBody] UpdateReviewDto dto)
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

            var review = await reviewService.UpdateReviewAsync(id, userId, dto);
            if (review == null)
            {
                return NotFound("Review not found or you don't have permission to update it.");
            }

            return Ok(review);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteReview(int id)
        {
            var userIdClaim = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("User is not authenticated.");
            }

            var success = await reviewService.DeleteReviewAsync(id, userId);
            if (!success)
            {
                return NotFound("Review not found or you don't have permission to delete it.");
            }

            return Ok(new { success = true });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<ActionResult<List<ReviewDto>>> GetAllReviews()
        {
            var reviews = await reviewService.GetAllReviewsAsync();
            return Ok(reviews);
        }
    }
}

