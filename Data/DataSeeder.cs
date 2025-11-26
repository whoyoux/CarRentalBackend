using CarRentalBackend.Entities;
using CarRentalBackend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CarRentalBackend.Data
{
    public static class DataSeeder
    {
        public static async Task SeedDataAsync(DataContext context)
        {
            Console.WriteLine("Starting data seeding...");

            await SeedUsersAsync(context);
            await SeedCarsAsync(context);
            await SeedReservationsAsync(context);

            Console.WriteLine("Data seeding completed.");
        }

        private static async Task SeedUsersAsync(DataContext context)
        {
            if (await context.Users.AnyAsync(u => u.Role == "User"))
            {
                Console.WriteLine("Users already seeded. Skipping user seeding.");
                return;
            }

            Console.WriteLine("Seeding users...");

            var users = new List<User>
            {
                new User
                {
                    Email = "john.doe@example.com",
                    Role = "User"
                },
                new User
                {
                    Email = "jane.smith@example.com",
                    Role = "User"
                },
                new User
                {
                    Email = "mike.johnson@example.com",
                    Role = "User"
                },
                new User
                {
                    Email = "sarah.williams@example.com",
                    Role = "User"
                }
            };

            var passwordHasher = new PasswordHasher<User>();
            foreach (var user in users)
            {
                user.PasswordHash = passwordHasher.HashPassword(user, "Password123!");
            }

            context.Users.AddRange(users);
            await context.SaveChangesAsync();

            Console.WriteLine($"Seeded {users.Count} users.");
        }

        private static async Task SeedCarsAsync(DataContext context)
        {
            if (await context.Cars.AnyAsync())
            {
                Console.WriteLine("Cars already seeded. Skipping car seeding.");
                return;
            }

            Console.WriteLine("Seeding cars...");

            var cars = new List<Car>
            {
                new Car
                {
                    Brand = "Toyota",
                    Model = "Camry",
                    Year = 2023,
                    PricePerDay = 45.00m,
                    Description = "A reliable and comfortable midsize sedan perfect for business trips or family vacations.",
                    ImageUrl = "https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=800"
                },
                new Car
                {
                    Brand = "Honda",
                    Model = "Civic",
                    Year = 2024,
                    PricePerDay = 40.00m,
                    Description = "Fuel-efficient compact car with modern features and excellent safety ratings.",
                    ImageUrl = "https://images.unsplash.com/photo-1590362891991-f776e747a588?w=800"
                },
                new Car
                {
                    Brand = "Tesla",
                    Model = "Model 3",
                    Year = 2024,
                    PricePerDay = 85.00m,
                    Description = "Electric vehicle with cutting-edge technology, autopilot features, and zero emissions.",
                    ImageUrl = "https://images.unsplash.com/photo-1560958089-b8a1929cea89?w=800"
                },
                new Car
                {
                    Brand = "BMW",
                    Model = "X5",
                    Year = 2023,
                    PricePerDay = 95.00m,
                    Description = "Luxury SUV with premium comfort, advanced technology, and spacious interior.",
                    ImageUrl = "https://images.unsplash.com/photo-1555215695-3004980ad54e?w=800"
                },
                new Car
                {
                    Brand = "Ford",
                    Model = "Mustang",
                    Year = 2023,
                    PricePerDay = 75.00m,
                    Description = "Iconic American muscle car with powerful performance and head-turning style.",
                    ImageUrl = "https://images.unsplash.com/photo-1584345604476-8ec5f1f69b06?w=800"
                },
                new Car
                {
                    Brand = "Mercedes-Benz",
                    Model = "E-Class",
                    Year = 2024,
                    PricePerDay = 100.00m,
                    Description = "Premium luxury sedan combining elegance, performance, and advanced safety features.",
                    ImageUrl = "https://images.unsplash.com/photo-1617814076367-b759c7d7e738?w=800"
                },
                new Car
                {
                    Brand = "Chevrolet",
                    Model = "Tahoe",
                    Year = 2023,
                    PricePerDay = 80.00m,
                    Description = "Full-size SUV perfect for large groups or families with plenty of cargo space.",
                    ImageUrl = "https://images.unsplash.com/photo-1519641471654-76ce0107ad1b?w=800"
                },
                new Car
                {
                    Brand = "Audi",
                    Model = "A4",
                    Year = 2024,
                    PricePerDay = 70.00m,
                    Description = "Sophisticated sedan with quattro all-wheel drive and premium interior craftsmanship.",
                    ImageUrl = "https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6?w=800"
                },
                new Car
                {
                    Brand = "Jeep",
                    Model = "Wrangler",
                    Year = 2023,
                    PricePerDay = 65.00m,
                    Description = "Rugged off-road vehicle perfect for adventure seekers and outdoor enthusiasts.",
                    ImageUrl = "https://images.unsplash.com/photo-1606220838315-056192d5e927?w=800"
                },
                new Car
                {
                    Brand = "Volkswagen",
                    Model = "Passat",
                    Year = 2023,
                    PricePerDay = 50.00m,
                    Description = "Spacious and comfortable family sedan with excellent fuel economy.",
                    ImageUrl = "https://images.unsplash.com/photo-1622353219448-46a00181c510?w=800"
                }
            };

            context.Cars.AddRange(cars);
            await context.SaveChangesAsync();

            Console.WriteLine($"Seeded {cars.Count} cars.");
        }

        private static async Task SeedReservationsAsync(DataContext context)
        {
            if (await context.Reservations.AnyAsync())
            {
                Console.WriteLine("Reservations already seeded. Skipping reservation seeding.");
                return;
            }

            var users = await context.Users.Where(u => u.Role == "User").ToListAsync();
            var cars = await context.Cars.ToListAsync();

            if (!users.Any() || !cars.Any())
            {
                Console.WriteLine("No users or cars found. Skipping reservation seeding.");
                return;
            }

            Console.WriteLine("Seeding reservations...");

            var random = new Random();
            var reservations = new List<Reservation>();

            // Create past reservations
            for (int i = 0; i < 5; i++)
            {
                var user = users[random.Next(users.Count)];
                var car = cars[random.Next(cars.Count)];
                var daysAgo = random.Next(30, 90);
                var duration = random.Next(1, 7);

                var startDate = DateTime.UtcNow.AddDays(-daysAgo);
                var endDate = startDate.AddDays(duration);

                reservations.Add(new Reservation
                {
                    UserId = user.Id,
                    CarId = car.Id,
                    StartDateTime = startDate,
                    EndDateTime = endDate,
                    TotalPrice = car.PricePerDay * duration,
                    CreatedAt = startDate.AddDays(-2)
                });
            }

            // Create current/upcoming reservations
            for (int i = 0; i < 8; i++)
            {
                var user = users[random.Next(users.Count)];
                var car = cars[random.Next(cars.Count)];
                var daysFromNow = random.Next(-3, 30);
                var duration = random.Next(2, 10);

                var startDate = DateTime.UtcNow.AddDays(daysFromNow);
                var endDate = startDate.AddDays(duration);

                reservations.Add(new Reservation
                {
                    UserId = user.Id,
                    CarId = car.Id,
                    StartDateTime = startDate,
                    EndDateTime = endDate,
                    TotalPrice = car.PricePerDay * duration,
                    CreatedAt = DateTime.UtcNow.AddDays(daysFromNow - random.Next(1, 5))
                });
            }

            context.Reservations.AddRange(reservations);
            await context.SaveChangesAsync();

            Console.WriteLine($"Seeded {reservations.Count} reservations.");
        }
    }
}
