namespace IdentityService.Domain.Interfaces;

public interface ISoftDelete
{
    public bool IsDeleted { get; set; }

    public DateTime? DeletedOnUtc { get; set; }
}
