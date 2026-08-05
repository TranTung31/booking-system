using AutoMapper;
using IdentityService.Application.Dtos.User;
using UserEntity = IdentityService.Domain.Entities.User;

namespace IdentityService.Application.Features.User.Mapping;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<UserEntity, UserDto>();
    }
}
