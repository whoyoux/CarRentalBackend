namespace CarRentalBackend.Models
{
    public class Car
    {
        public int Id { get; set; }
        public required string Brand { get; set; }
        public required string Model { get; set; }
        public int Year { get; set; }
        public decimal PricePerDay { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
    }
}
