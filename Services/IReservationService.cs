using CarRentalBackend.ModelsDto;

namespace CarRentalBackend.Services
{
    public interface IReservationService
    {
        Task<ReservationDto?> CreateReservationAsync(Guid userId, CreateReservationDto dto);
        Task<bool> CancelReservationAsync(int reservationId, Guid userId);
        Task<List<ReservationDto>> GetUserReservationsAsync(Guid userId);
        Task<List<AdminReservationDto>> GetAllReservationsAsync();
        Task<bool> AdminCancelReservationAsync(int reservationId);
    }
}
