using CarRentalBackend.Entities;
using CarRentalBackend.Models;
using Microsoft.EntityFrameworkCore;

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
    }
}
