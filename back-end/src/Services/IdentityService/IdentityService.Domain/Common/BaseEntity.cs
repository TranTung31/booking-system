using IdentityService.Domain.Interfaces;

namespace IdentityService.Domain.Common;

public abstract class BaseEntity<TKey> : IAuditable
{
    public TKey Id { get; protected set; } = default!;

    public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedOnUtc { get; set; }
}
