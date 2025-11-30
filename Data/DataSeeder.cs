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
            await SeedReviewsAsync(context);

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
                    Email = "emily.chen@example.com",
                    Role = "User"
                },
                new User
                {
                    Email = "david.martinez@example.com",
                    Role = "User"
                },
                new User
                {
                    Email = "sophia.anderson@example.com",
                    Role = "User"
                },
                new User
                {
                    Email = "james.wilson@example.com",
                    Role = "User"
                },
                new User
                {
                    Email = "olivia.brown@example.com",
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
                    Year = 2024,
                    PricePerDay = 45.00m,
                    Description = "A reliable and comfortable midsize sedan perfect for business trips or family vacations. Features advanced safety systems and excellent fuel economy.",
                    ImageUrl = "/car.png"
                },
                new Car
                {
                    Brand = "Honda",
                    Model = "Civic",
                    Year = 2024,
                    PricePerDay = 40.00m,
                    Description = "Fuel-efficient compact car with modern features and excellent safety ratings. Perfect for city driving and daily commutes.",
                    ImageUrl = "/car.png"
                },
                new Car
                {
                    Brand = "Tesla",
                    Model = "Model 3",
                    Year = 2024,
                    PricePerDay = 85.00m,
                    Description = "Electric vehicle with cutting-edge technology, autopilot features, and zero emissions. Fast charging and impressive range.",
                    ImageUrl = "/car.png"
                },
                new Car
                {
                    Brand = "BMW",
                    Model = "X5",
                    Year = 2024,
                    PricePerDay = 95.00m,
                    Description = "Luxury SUV with premium comfort, advanced technology, and spacious interior. Perfect for long journeys and family trips.",
                    ImageUrl = "/car.png"
                },
                new Car
                {
                    Brand = "Ford",
                    Model = "Mustang",
                    Year = 2024,
                    PricePerDay = 75.00m,
                    Description = "Iconic American muscle car with powerful performance and head-turning style. Experience the thrill of driving.",
                    ImageUrl = "/car.png"
                },
                new Car
                {
                    Brand = "Mercedes-Benz",
                    Model = "E-Class",
                    Year = 2024,
                    PricePerDay = 100.00m,
                    Description = "Premium luxury sedan combining elegance, performance, and advanced safety features. The epitome of German engineering.",
                    ImageUrl = "/car.png"
                },
                new Car
                {
                    Brand = "Audi",
                    Model = "A4",
                    Year = 2024,
                    PricePerDay = 70.00m,
                    Description = "Sophisticated sedan with quattro all-wheel drive and premium interior craftsmanship. Sporty yet elegant.",
                    ImageUrl = "/car.png"
                },
                new Car
                {
                    Brand = "Volkswagen",
                    Model = "Golf",
                    Year = 2024,
                    PricePerDay = 50.00m,
                    Description = "Compact hatchback with excellent build quality and practical design. Great for urban environments.",
                    ImageUrl = "/car.png"
                },
                new Car
                {
                    Brand = "Mazda",
                    Model = "CX-5",
                    Year = 2024,
                    PricePerDay = 60.00m,
                    Description = "Stylish compact SUV with sporty handling and premium interior. Perfect balance of comfort and performance.",
                    ImageUrl = "/car.png"
                },
                new Car
                {
                    Brand = "Hyundai",
                    Model = "Elantra",
                    Year = 2024,
                    PricePerDay = 42.00m,
                    Description = "Modern compact sedan with impressive technology features and fuel efficiency. Great value for money.",
                    ImageUrl = "/car.png"
                },
                new Car
                {
                    Brand = "Nissan",
                    Model = "Altima",
                    Year = 2024,
                    PricePerDay = 48.00m,
                    Description = "Comfortable midsize sedan with smooth ride and spacious cabin. Ideal for daily commuting and road trips.",
                    ImageUrl = "/car.png"
                },
                new Car
                {
                    Brand = "Subaru",
                    Model = "Outback",
                    Year = 2024,
                    PricePerDay = 65.00m,
                    Description = "Versatile wagon with all-wheel drive and excellent ground clearance. Perfect for adventure and outdoor activities.",
                    ImageUrl = "/car.png"
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

            for (int i = 0; i < 8; i++)
            {
                var user = users[random.Next(users.Count)];
                var car = cars[random.Next(cars.Count)];
                var daysAgo = random.Next(60, 120);
                var duration = random.Next(2, 7);

                var startDate = DateTime.UtcNow.AddDays(-daysAgo);
                var endDate = startDate.AddDays(duration);

                reservations.Add(new Reservation
                {
                    UserId = user.Id,
                    CarId = car.Id,
                    StartDateTime = startDate,
                    EndDateTime = endDate,
                    TotalPrice = car.PricePerDay * duration,
                    CreatedAt = startDate.AddDays(-random.Next(1, 5))
                });
            }

            for (int i = 0; i < 5; i++)
            {
                var user = users[random.Next(users.Count)];
                var car = cars[random.Next(cars.Count)];
                var daysAgo = random.Next(1, 5);
                var duration = random.Next(3, 10);

                var startDate = DateTime.UtcNow.AddDays(-daysAgo);
                var endDate = startDate.AddDays(duration);

                reservations.Add(new Reservation
                {
                    UserId = user.Id,
                    CarId = car.Id,
                    StartDateTime = startDate,
                    EndDateTime = endDate,
                    TotalPrice = car.PricePerDay * duration,
                    CreatedAt = startDate.AddDays(-random.Next(1, 3))
                });
            }

            for (int i = 0; i < 7; i++)
            {
                var user = users[random.Next(users.Count)];
                var car = cars[random.Next(cars.Count)];
                var daysFromNow = random.Next(5, 45);
                var duration = random.Next(2, 8);

                var startDate = DateTime.UtcNow.AddDays(daysFromNow);
                var endDate = startDate.AddDays(duration);

                reservations.Add(new Reservation
                {
                    UserId = user.Id,
                    CarId = car.Id,
                    StartDateTime = startDate,
                    EndDateTime = endDate,
                    TotalPrice = car.PricePerDay * duration,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 10))
                });
            }

            context.Reservations.AddRange(reservations);
            await context.SaveChangesAsync();

            Console.WriteLine($"Seeded {reservations.Count} reservations.");
        }

        private static async Task SeedReviewsAsync(DataContext context)
        {
            if (await context.Reviews.AnyAsync())
            {
                Console.WriteLine("Reviews already seeded. Skipping review seeding.");
                return;
            }

            var users = await context.Users.Where(u => u.Role == "User").ToListAsync();
            var cars = await context.Cars.ToListAsync();
            var reservations = await context.Reservations
                .Where(r => r.EndDateTime < DateTime.UtcNow)
                .ToListAsync();

            if (!users.Any() || !cars.Any() || !reservations.Any())
            {
                Console.WriteLine("No users, cars, or completed reservations found. Skipping review seeding.");
                return;
            }

            Console.WriteLine("Seeding reviews...");

            var usersWithReservations = reservations
                .GroupBy(r => r.UserId)
                .Where(g => g.Any())
                .Take(3)
                .Select(g => g.Key)
                .ToList();

            if (usersWithReservations.Count < 3)
            {
                usersWithReservations = users.Take(3).Select(u => u.Id).ToList();
            }

            var reviews = new List<Review>();

            if (usersWithReservations.Count > 0)
            {
                var user1Id = usersWithReservations[0];
                var user1Reservations = reservations.Where(r => r.UserId == user1Id).Take(3).ToList();

                if (user1Reservations.Count > 0)
                {
                    reviews.Add(new Review
                    {
                        UserId = user1Id,
                        CarId = user1Reservations[0].CarId,
                        Rating = 5,
                        Comment = "Excellent car! Very comfortable and fuel-efficient. The service was outstanding and the vehicle was in perfect condition. Highly recommend!",
                        CreatedAt = user1Reservations[0].EndDateTime.AddDays(1)
                    });
                }
                if (user1Reservations.Count > 1)
                {
                    reviews.Add(new Review
                    {
                        UserId = user1Id,
                        CarId = user1Reservations[1].CarId,
                        Rating = 4,
                        Comment = "Great experience overall. The car handled well on long trips and had all the features I needed. Minor issue with the navigation system, but nothing major.",
                        CreatedAt = user1Reservations[1].EndDateTime.AddDays(2)
                    });
                }
            }

            if (usersWithReservations.Count > 1)
            {
                var user2Id = usersWithReservations[1];
                var user2Reservations = reservations.Where(r => r.UserId == user2Id).Take(3).ToList();

                if (user2Reservations.Count > 0)
                {
                    reviews.Add(new Review
                    {
                        UserId = user2Id,
                        CarId = user2Reservations[0].CarId,
                        Rating = 5,
                        Comment = "Amazing vehicle! The performance exceeded my expectations. Smooth ride, great acceleration, and very spacious interior. Will definitely rent again!",
                        CreatedAt = user2Reservations[0].EndDateTime.AddDays(1)
                    });
                }
                if (user2Reservations.Count > 1)
                {
                    reviews.Add(new Review
                    {
                        UserId = user2Id,
                        CarId = user2Reservations[1].CarId,
                        Rating = 4,
                        Comment = "Very good car for the price. Comfortable seats and good fuel economy. The only downside was the limited trunk space, but overall a solid choice.",
                        CreatedAt = user2Reservations[1].EndDateTime.AddDays(1)
                    });
                }
                if (user2Reservations.Count > 2)
                {
                    reviews.Add(new Review
                    {
                        UserId = user2Id,
                        CarId = user2Reservations[2].CarId,
                        Rating = 5,
                        Comment = "Perfect rental experience! The car was clean, well-maintained, and had all modern features. Great value for money and excellent customer service.",
                        CreatedAt = user2Reservations[2].EndDateTime.AddDays(3)
                    });
                }
            }

            if (usersWithReservations.Count > 2)
            {
                var user3Id = usersWithReservations[2];
                var user3Reservations = reservations.Where(r => r.UserId == user3Id).Take(2).ToList();

                if (user3Reservations.Count > 0)
                {
                    reviews.Add(new Review
                    {
                        UserId = user3Id,
                        CarId = user3Reservations[0].CarId,
                        Rating = 4,
                        Comment = "Nice car with good features. The ride was smooth and comfortable. Some minor scratches on the exterior, but the interior was spotless. Would rent again.",
                        CreatedAt = user3Reservations[0].EndDateTime.AddDays(2)
                    });
                }
                if (user3Reservations.Count > 1)
                {
                    reviews.Add(new Review
                    {
                        UserId = user3Id,
                        CarId = user3Reservations[1].CarId,
                        Rating = 5,
                        Comment = "Outstanding vehicle! Everything worked perfectly, from the infotainment system to the safety features. Very satisfied with this rental and the overall experience.",
                        CreatedAt = user3Reservations[1].EndDateTime.AddDays(1)
                    });
                }
            }

            context.Reviews.AddRange(reviews);
            await context.SaveChangesAsync();

            Console.WriteLine($"Seeded {reviews.Count} reviews.");
        }
    }
}
