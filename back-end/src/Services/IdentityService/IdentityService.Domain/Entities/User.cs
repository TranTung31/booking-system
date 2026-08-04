namespace IdentityService.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string? FullName { get; set; }
        // Các thuộc tính nghiệp vụ khác
    }
}
