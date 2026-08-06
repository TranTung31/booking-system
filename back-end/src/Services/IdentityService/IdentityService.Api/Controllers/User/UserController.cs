using IdentityService.Application.Dtos.User;
using IdentityService.Application.Features.User.Commands;
using IdentityService.Application.Features.User.Queries;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Controllers.User;

[ApiController]
[Route("api/user")]
public class UserController : BaseApiController
{
    [HttpPost("search")]
    public async Task<ActionResult<UserDto>> GetLstPaging([FromBody] UserSearchDto dto, CancellationToken ct)
    {
        var command = new GetLstPagingUserQuery(dto);
        var result = await Mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<UserDto>> GetById(Guid userId, CancellationToken ct)
    {
        var command = new GetUserByIdQuery(userId);
        var result = await Mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(UserCreateDto dto, CancellationToken ct)
    {
        var command = new CreateUserCommand(dto);
        var result = await Mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpPut("{userId}")]
    public async Task<ActionResult<Guid>> Update(Guid userId, UserUpdateDto dto, CancellationToken ct)
    {
        var command = new UpdateUserCommand(userId, dto);
        var result = await Mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpDelete("{userId}")]
    public async Task<ActionResult<Guid>> Delete(Guid userId, CancellationToken ct)
    {
        var command = new DeleteUserCommand(userId);
        var result = await Mediator.Send(command, ct);
        return Ok(result);
    }
}
