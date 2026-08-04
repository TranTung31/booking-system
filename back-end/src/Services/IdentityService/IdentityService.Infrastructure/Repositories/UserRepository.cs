using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using IdentityService.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserRepository(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<User?> FindByIdAsync(Guid id)
        {
            var appUser = await _userManager.FindByIdAsync(id.ToString());

            // Implement logic to find user by ID
            return await Task.FromResult<User?>(null);
        }

        public async Task<User?> FindByNameAsync(string userName)
        {
            // Implement logic to find user by ID
            return await Task.FromResult<User?>(null);
        }
    }
}
