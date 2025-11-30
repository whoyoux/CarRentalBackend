using CarRentalBackend.Data;
using CarRentalBackend.Models;
using CarRentalBackend.ModelsDto;
using Microsoft.EntityFrameworkCore;

namespace CarRentalBackend.Services
{
    public class ReviewService(DataContext context) : IReviewService
    {

        public async Task<List<ReviewDto>> GetReviewsByCarIdAsync(int carId)
        {
            return await context.Reviews
                .Where(r => r.CarId == carId)
                .Include(r => r.User)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CarId = r.CarId,
                    UserId = r.UserId,
                    UserEmail = r.User!.Email,
                    CreatedAt = r.CreatedAt
                })
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<ReviewDto?> CreateReviewAsync(Guid userId, CreateReviewDto dto)
        {
            var existingReview = await context.Reviews
                .FirstOrDefaultAsync(r => r.CarId == dto.CarId && r.UserId == userId);

            if (existingReview != null)
            {
                throw new InvalidOperationException("You have already reviewed this car.");
            }

            var carExists = await context.Cars.AnyAsync(c => c.Id == dto.CarId);
            if (!carExists)
            {
                return null;
            }

            var review = new Review
            {
                Rating = dto.Rating,
                Comment = dto.Comment,
                CarId = dto.CarId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            context.Reviews.Add(review);
            await context.SaveChangesAsync();

            var user = await context.Users.FindAsync(userId);
            return new ReviewDto
            {
                Id = review.Id,
                Rating = review.Rating,
                Comment = review.Comment,
                CarId = review.CarId,
                UserId = review.UserId,
                UserEmail = user?.Email ?? "Unknown",
                CreatedAt = review.CreatedAt
            };
        }

        public async Task<ReviewDto?> UpdateReviewAsync(int reviewId, Guid userId, UpdateReviewDto dto)
        {
            var review = await context.Reviews
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId);

            if (review == null)
            {
                return null;
            }

            review.Rating = dto.Rating;
            review.Comment = dto.Comment;

            await context.SaveChangesAsync();

            var user = await context.Users.FindAsync(userId);
            return new ReviewDto
            {
                Id = review.Id,
                Rating = review.Rating,
                Comment = review.Comment,
                CarId = review.CarId,
                UserId = review.UserId,
                UserEmail = user?.Email ?? "Unknown",
                CreatedAt = review.CreatedAt
            };
        }

        public async Task<bool> DeleteReviewAsync(int reviewId, Guid userId)
        {
            var review = await context.Reviews
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId);

            if (review == null)
            {
                return false;
            }

            context.Reviews.Remove(review);
            await context.SaveChangesAsync();

            return true;
        }

        public async Task<List<ReviewDto>> GetAllReviewsAsync()
        {
            return await context.Reviews
                .Include(r => r.User)
                .Include(r => r.Car)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CarId = r.CarId,
                    UserId = r.UserId,
                    UserEmail = r.User!.Email,
                    CreatedAt = r.CreatedAt
                })
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
    }
}

