using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Interfaces
{
    public interface IUserRepository : IRepository<User, Guid>
    {
        Task<User?> FindByIdAsync(Guid id);
        Task<User?> FindByNameAsync(string userName);
    }
}
