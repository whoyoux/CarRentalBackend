using CarRentalBackend.ModelsDto;

namespace CarRentalBackend.Services
{
    public interface IReviewService
    {
        Task<List<ReviewDto>> GetReviewsByCarIdAsync(int carId);
        Task<ReviewDto?> CreateReviewAsync(Guid userId, CreateReviewDto dto);
        Task<ReviewDto?> UpdateReviewAsync(int reviewId, Guid userId, UpdateReviewDto dto);
        Task<bool> DeleteReviewAsync(int reviewId, Guid userId);
        Task<List<ReviewDto>> GetAllReviewsAsync();
    }
}

