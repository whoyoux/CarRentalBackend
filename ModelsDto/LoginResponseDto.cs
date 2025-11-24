namespace CarRentalBackend.ModelsDto
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Id { get; set; }

    }
}
