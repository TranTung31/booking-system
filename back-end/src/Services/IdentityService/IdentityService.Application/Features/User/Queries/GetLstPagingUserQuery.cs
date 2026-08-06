using AutoMapper;
using IdentityService.Application.Dtos.Common;
using IdentityService.Application.Dtos.User;
using IdentityService.Application.Extensions;
using IdentityService.Domain.Interfaces;
using MediatR;

namespace IdentityService.Application.Features.User.Queries;

public record GetLstPagingUserQuery(UserSearchDto SearchDto) : IRequest<UserPagedListDto>
{
}

public class GetLstPagingUserQueryHandler : IRequestHandler<GetLstPagingUserQuery, UserPagedListDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetLstPagingUserQueryHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<UserPagedListDto> Handle(GetLstPagingUserQuery request, CancellationToken cancellationToken)
    {
        var requestDto = request.SearchDto;
        var lstUser = await _userRepository.GetLstPagingUserAsync(requestDto.Keyword, requestDto.PageNumber, requestDto.PageSize);

        var users = lstUser.ToModel<UserDto, Domain.Entities.User>(_mapper);

        return new UserPagedListDto
        {
            Data = users,
            Pagination = new PaginationDto
            {
                CurrentPage = requestDto.PageNumber,
                PageSize = requestDto.PageSize,
                TotalRecords = lstUser.TotalCount,
            }
        };
    }
}
