namespace CarRentalBackend.ModelsDto
{
    public class GetCurrentUserResponseDto
    {
        public required string Id { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
    }
}
