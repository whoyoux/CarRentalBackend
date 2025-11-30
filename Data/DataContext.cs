using CarRentalBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CarRentalBackend.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        public required DbSet<Car> Cars { get; set; }

        public required DbSet<User> Users { get; set; }

        public required DbSet<Reservation> Reservations { get; set; }

        public required DbSet<Review> Reviews { get; set; }

        public required DbSet<ReservationLog> ReservationLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.ToTable("Reservations", t => t.HasTrigger("LogReservationDelete"));
            });
        }
    }
}
