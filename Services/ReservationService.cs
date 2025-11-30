using CarRentalBackend.Data;
using CarRentalBackend.Models;
using CarRentalBackend.ModelsDto;
using Microsoft.EntityFrameworkCore;

namespace CarRentalBackend.Services
{
    public class ReservationService(DataContext context) : IReservationService
    {

        public async Task<ReservationDto?> CreateReservationAsync(Guid userId, CreateReservationDto dto)
        {
            if (dto.StartDateTime >= dto.EndDateTime)
            {
                throw new InvalidOperationException("End date must be after start date.");
            }

            var todayMinusOneDay = DateTime.UtcNow.Date.AddDays(-1);
            if (dto.StartDateTime.Date < todayMinusOneDay)
            {
                throw new InvalidOperationException("Cannot reserve in the past.");
            }

            var car = await context.Cars.FindAsync(dto.CarId);
            if (car == null)
            {
                return null;
            }

            var hasConflict = await context.Reservations
                .AnyAsync(r => r.CarId == dto.CarId &&
                    ((dto.StartDateTime >= r.StartDateTime && dto.StartDateTime < r.EndDateTime) ||
                     (dto.EndDateTime > r.StartDateTime && dto.EndDateTime <= r.EndDateTime) ||
                     (dto.StartDateTime <= r.StartDateTime && dto.EndDateTime >= r.EndDateTime)));

            if (hasConflict)
            {
                throw new InvalidOperationException("Car is not available for the selected dates.");
            }

            var days = (dto.EndDateTime - dto.StartDateTime).TotalDays;
            var totalPrice = (decimal)days * car.PricePerDay;

            var reservation = new Reservation
            {
                CarId = dto.CarId,
                UserId = userId,
                StartDateTime = dto.StartDateTime,
                EndDateTime = dto.EndDateTime,
                TotalPrice = totalPrice,
                CreatedAt = DateTime.UtcNow
            };

            context.Reservations.Add(reservation);
            await context.SaveChangesAsync();

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
            var reservation = await context.Reservations
                .FirstOrDefaultAsync(r => r.Id == reservationId && r.UserId == userId);

            if (reservation == null)
            {
                return false;
            }

            if (reservation.EndDateTime < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Cannot cancel a reservation that has already ended.");
            }

            context.Reservations.Remove(reservation);
            await context.SaveChangesAsync();

            return true;
        }

        public async Task<List<ReservationDto>> GetUserReservationsAsync(Guid userId)
        {
            var reservations = await context.Reservations
                .Where(r => r.UserId == userId)
                .Include(r => r.Car)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reservations.Select(r => new ReservationDto
            {
                Id = r.Id,
                CarId = r.CarId,
                CarBrand = r.Car?.Brand ?? "Unknown",
                CarModel = r.Car?.Model ?? "Unknown",
                StartDateTime = r.StartDateTime,
                EndDateTime = r.EndDateTime,
                TotalPrice = r.TotalPrice,
                CreatedAt = r.CreatedAt
            }).ToList();
        }

        public async Task<List<AdminReservationDto>> GetAllReservationsAsync()
        {
            var reservations = await context.Reservations
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
            var reservation = await context.Reservations
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null)
            {
                return false;
            }

            if (reservation.EndDateTime < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Cannot cancel a reservation that has already ended.");
            }

            context.Reservations.Remove(reservation);
            await context.SaveChangesAsync();

            return true;
        }
    }
}
