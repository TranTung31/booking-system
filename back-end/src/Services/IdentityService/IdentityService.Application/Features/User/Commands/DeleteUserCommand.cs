using IdentityService.Domain.Interfaces;
using MediatR;

namespace IdentityService.Application.Features.User.Commands;

public record DeleteUserCommand(Guid userId) : IRequest<string>
{
}

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, string>
{
    private readonly IUserRepository _userRepository;

    public DeleteUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<string> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        await _userRepository.DeleteUserAsync(request.userId);

        return "Xóa người dùng thành công!";
    }
}
