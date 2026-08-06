using IdentityService.Application.Dtos.Common;

namespace IdentityService.Application.Dtos.User;

public record UserDto : BaseEntityDto<Guid>
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
}
