namespace AuthService.Models
{
    public class User
    {
        public int Id { get; set; }             // PK
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
