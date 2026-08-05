using IdentityService.Application.Dtos.User;
using IdentityService.Application.Features.User.Commands;
using IdentityService.Application.Features.User.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Controllers.User;

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly ISender _sender;

    public UserController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<UserDto>> GetById(Guid userId, CancellationToken ct)
    {
        var command = new GetUserByIdQuery(userId);
        var result = await _sender.Send(command, ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(UserCreateDto dto, CancellationToken ct)
    {
        var command = new CreateUserCommand(dto);
        var result = await _sender.Send(command, ct);
        return Ok(result);
    }

    [HttpPut("{userId}")]
    public async Task<ActionResult<Guid>> Update(Guid userId, UserUpdateDto dto, CancellationToken ct)
    {
        var command = new UpdateUserCommand(userId, dto);
        var result = await _sender.Send(command, ct);
        return Ok(result);
    }

    [HttpDelete("{userId}")]
    public async Task<ActionResult<Guid>> Delete(Guid userId, CancellationToken ct)
    {
        var command = new DeleteUserCommand(userId);
        var result = await _sender.Send(command, ct);
        return Ok(result);
    }
}
