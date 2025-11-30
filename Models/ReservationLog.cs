namespace CarRentalBackend.Models
{
    public class ReservationLog
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public Guid UserId { get; set; }
        public required string Action { get; set; }
        public DateTime LogDate { get; set; } = DateTime.UtcNow;
    }
}

