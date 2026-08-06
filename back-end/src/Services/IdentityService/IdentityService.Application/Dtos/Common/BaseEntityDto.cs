namespace IdentityService.Application.Dtos.Common;

public partial record BaseEntityDto<TKey> : BaseModel
{
    public TKey Id { get; set; } = default!;

    public DateTime CreatedOnUtc { get; set; }

    public DateTime? UpdatedOnUtc { get; set; }
}
