namespace IdentityService.Application.Dtos.User;

public class UserUpdateDto
{
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
}
