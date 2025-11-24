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

        public DbSet<Car> Cars { get; set; }

        public DbSet<User> Users { get; set; }
    }
}
