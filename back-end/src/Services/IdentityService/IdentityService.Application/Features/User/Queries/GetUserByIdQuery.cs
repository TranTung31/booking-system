using AutoMapper;
using IdentityService.Application.Dtos.User;
using IdentityService.Domain.Interfaces;
using MediatR;

namespace IdentityService.Application.Features.User.Queries;

public record GetUserByIdQuery(Guid userId) : IRequest<UserDto?>
{
}

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetUserByIdQueryHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindByIdAsync(request.userId);
        var result = _mapper.Map<UserDto>(user);

        return result;
    }
}
