using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using IdentityService.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Infrastructure.Repositories
{
    public class UserRepository : EfRepository<User, Guid>, IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserRepository(AppIdentityDbContext context, UserManager<ApplicationUser> userManager) : base(context)
        {
            _userManager = userManager;
        }

        public async Task<User?> FindByIdAsync(Guid id)
        {
            return await GetByIdAsync(id);
        }

        public async Task<User?> FindByNameAsync(string userName)
        {
            var appUser = await _userManager.FindByNameAsync(userName);

            if (appUser == null)
                return null;

            return MapToDomain(appUser);
        }

        private static User MapToDomain(ApplicationUser appUser)
        {
            return new User(appUser.Id)
            {
                Username = appUser.UserName ?? string.Empty,
                Email = appUser.Email ?? string.Empty,
                PasswordHash = appUser.PasswordHash ?? string.Empty,
                FullName = appUser.FullName,
                CreatedOnUtc = appUser.CreatedOnUtc,
                UpdatedOnUtc = appUser.UpdatedOnUtc,
            };
        }
    }
}
