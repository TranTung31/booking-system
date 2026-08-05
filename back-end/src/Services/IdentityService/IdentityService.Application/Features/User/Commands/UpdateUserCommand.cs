using IdentityService.Application.Dtos.User;
using IdentityService.Domain.Interfaces;
using MediatR;

namespace IdentityService.Application.Features.User.Commands;

public record UpdateUserCommand(Guid userId, UserUpdateDto UserUpdateDto) : IRequest<Guid>
{
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;

    public UpdateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Guid> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var dto = request.UserUpdateDto;
        var result = await _userRepository.UpdateUserAsync(request.userId, dto.Email, dto.FullName);

        return result;
    }
}
