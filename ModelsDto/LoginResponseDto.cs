namespace CarRentalBackend.ModelsDto
{
    public class LoginResponseDto
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
        public required string Id { get; set; }

    }
}
