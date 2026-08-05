namespace IdentityService.Domain.Interfaces;

public interface IAuditable
{
    public DateTime CreatedOnUtc { get; set; }

    public DateTime? UpdatedOnUtc { get; set; }
}
