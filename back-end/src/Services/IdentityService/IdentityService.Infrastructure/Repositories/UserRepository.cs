using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using IdentityService.Infrastructure.Extensions;
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

        public async Task<IPagedList<User>> GetLstPagingUserAsync(string? keyword, int pageNumber, int pageSize)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(x => (x.UserName != null && x.UserName.Contains(keyword)));

            query = query.OrderByDescending(x => x.CreatedOnUtc);

            var users = await query.ToPagedListAsync(pageNumber, pageSize);
            var result = users.Select(MapToDomain).ToList();

            return new PagedList<User>(result, users.PageIndex, users.PageSize, users.TotalCount);
        }

        public async Task<Guid> CreateUserAsync(string userName, string email, string? fullName, string password)
        {
            if (await _userManager.FindByNameAsync(userName) != null)
            {
                throw new InvalidOperationException($"User with username '{userName}' already exists.");
            }

            var appUser = new ApplicationUser
            {
                UserName = userName,
                Email = email,
                FullName = fullName,
                EmailConfirmed = true,
            };

            var result = await _userManager.CreateAsync(appUser, password);

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create user: {errors}");
            }

            return appUser.Id;
        }

        public async Task<Guid> UpdateUserAsync(Guid userId, string email, string? fullName)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString()) ?? throw new InvalidOperationException($"User {userId} not found!");

            user.Email = email;
            user.FullName = fullName;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to update user: {errors}");
            }

            return user.Id;
        }

        public async Task DeleteUserAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString()) ?? throw new InvalidOperationException($"User {userId} not found!");

            await _userManager.DeleteAsync(user);
        }

        public async Task<User?> FindByIdAsync(Guid id)
        {
            var appUser = await _userManager.FindByIdAsync(id.ToString());

            if (appUser == null)
                return null;

            return MapToDomain(appUser);
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
                FullName = appUser.FullName,
                CreatedOnUtc = appUser.CreatedOnUtc,
                UpdatedOnUtc = appUser.UpdatedOnUtc,
            };
        }
    }
}
