namespace CarRentalBackend.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public int CarId { get; set; }
        public Guid UserId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }

        public Car? Car { get; set; }
        public User? User { get; set; }
    }
}

