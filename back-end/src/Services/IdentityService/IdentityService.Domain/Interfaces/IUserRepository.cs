using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> FindByIdAsync(Guid id);
        Task<User?> FindByNameAsync(string userName);
    }
}
