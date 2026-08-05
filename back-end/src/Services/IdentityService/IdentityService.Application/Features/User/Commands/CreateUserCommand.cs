using IdentityService.Application.Dtos.User;
using IdentityService.Domain.Interfaces;
using MediatR;

namespace IdentityService.Application.Features.User.Commands;

public record CreateUserCommand(UserCreateDto UserCreateDto) : IRequest<Guid>
{
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;

    public CreateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var dto = request.UserCreateDto;
        var result = await _userRepository.CreateUserAsync(dto.Username, dto.Email, dto.FullName, dto.Password);

        return result;
    }
}
