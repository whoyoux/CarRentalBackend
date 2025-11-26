namespace CarRentalBackend.ModelsDto
{
    public class ReservationDto
    {
        public int Id { get; set; }
        public int CarId { get; set; }
        public required string CarBrand { get; set; }
        public required string CarModel { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
