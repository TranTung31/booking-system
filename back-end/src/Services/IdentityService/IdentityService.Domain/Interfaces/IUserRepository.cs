using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Interfaces
{
    public interface IUserRepository : IRepository<User, Guid>
    {
        Task<Guid> CreateUserAsync(string userName, string email, string? fullName, string password);
        Task<Guid> UpdateUserAsync(Guid userId, string email, string? fullName);
        Task DeleteUserAsync(Guid userId);
        Task<User?> FindByIdAsync(Guid id);
        Task<User?> FindByNameAsync(string userName);
    }
}
