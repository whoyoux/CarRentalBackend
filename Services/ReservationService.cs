using CarRentalBackend.Data;
using CarRentalBackend.Entities;
using CarRentalBackend.ModelsDto;
using Microsoft.EntityFrameworkCore;

namespace CarRentalBackend.Services
{
    public class ReservationService : IReservationService
    {
        private readonly DataContext _context;

        public ReservationService(DataContext context)
        {
            _context = context;
        }

        public async Task<ReservationDto?> CreateReservationAsync(Guid userId, CreateReservationDto dto)
        {
            // Validate dates
            if (dto.StartDateTime >= dto.EndDateTime)
            {
                throw new InvalidOperationException("End date must be after start date.");
            }

            if (dto.StartDateTime < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Cannot reserve in the past.");
            }

            // Check if car exists
            var car = await _context.Cars.FindAsync(dto.CarId);
            if (car == null)
            {
                return null;
            }

            // Check if car is available
            var hasConflict = await _context.Reservations
                .AnyAsync(r => r.CarId == dto.CarId &&
                    ((dto.StartDateTime >= r.StartDateTime && dto.StartDateTime < r.EndDateTime) ||
                     (dto.EndDateTime > r.StartDateTime && dto.EndDateTime <= r.EndDateTime) ||
                     (dto.StartDateTime <= r.StartDateTime && dto.EndDateTime >= r.EndDateTime)));

            if (hasConflict)
            {
                throw new InvalidOperationException("Car is not available for the selected dates.");
            }

            // Calculate total price
            var days = (dto.EndDateTime - dto.StartDateTime).TotalDays;
            var totalPrice = (decimal)days * car.PricePerDay;

            // Create reservation
            var reservation = new Reservation
            {
                CarId = dto.CarId,
                UserId = userId,
                StartDateTime = dto.StartDateTime,
                EndDateTime = dto.EndDateTime,
                TotalPrice = totalPrice,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return new ReservationDto
            {
                Id = reservation.Id,
                CarId = reservation.CarId,
                CarBrand = car.Brand,
                CarModel = car.Model,
                StartDateTime = reservation.StartDateTime,
                EndDateTime = reservation.EndDateTime,
                TotalPrice = reservation.TotalPrice,
                CreatedAt = reservation.CreatedAt
            };
        }

        public async Task<bool> CancelReservationAsync(int reservationId, Guid userId)
        {
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == reservationId && r.UserId == userId);

            if (reservation == null)
            {
                return false;
            }

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<AdminReservationDto>> GetAllReservationsAsync()
        {
            var reservations = await _context.Reservations
                .Include(r => r.Car)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return [.. reservations.Select(r => new AdminReservationDto
            {
                Id = r.Id,
                CarId = r.CarId,
                CarBrand = r.Car?.Brand ?? "Unknown",
                CarModel = r.Car?.Model ?? "Unknown",
                UserId = r.UserId,
                UserEmail = r.User?.Email ?? "Unknown",
                StartDateTime = r.StartDateTime,
                EndDateTime = r.EndDateTime,
                TotalPrice = r.TotalPrice,
                CreatedAt = r.CreatedAt
            })];
        }

        public async Task<bool> AdminCancelReservationAsync(int reservationId)
        {
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null)
            {
                return false;
            }

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
