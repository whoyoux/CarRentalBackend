namespace CarRentalBackend.ModelsDto
{
    public class CreateReservationDto
    {
        public int CarId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
    }
}
