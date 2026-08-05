using IdentityService.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser<Guid>, IAuditable
    {
        public string? FullName { get; set; }

        public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedOnUtc { get; set; }
    }
}
