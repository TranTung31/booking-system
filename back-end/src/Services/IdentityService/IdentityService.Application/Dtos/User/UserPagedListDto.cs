using IdentityService.Application.Dtos.Common;

namespace IdentityService.Application.Dtos.User;

public partial record UserPagedListDto : BasePagedListDto<UserDto>
{
}
