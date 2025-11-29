namespace CarRentalBackend.ModelsDto
{
    public class ReservationLogDto
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public DateTime LogDate { get; set; }
    }
}

