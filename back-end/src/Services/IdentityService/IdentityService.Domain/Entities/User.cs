using IdentityService.Domain.Common;

namespace IdentityService.Domain.Entities
{
    public class User : BaseEntity<Guid>
    {
        public User()
        {
        }

        public User(Guid id)
        {
            Id = id;
        }

        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        // Các thuộc tính nghiệp vụ khác
    }
}
