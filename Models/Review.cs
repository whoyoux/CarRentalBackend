using System.ComponentModel.DataAnnotations;

namespace CarRentalBackend.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Comment { get; set; }

        public int CarId { get; set; }
        public Guid UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Car? Car { get; set; }
        public User? User { get; set; }
    }
}

