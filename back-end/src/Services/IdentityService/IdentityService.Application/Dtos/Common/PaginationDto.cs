namespace IdentityService.Application.Dtos.Common;

public partial record PaginationDto
{
    public int CurrentPage { get; init; }

    public int PageSize { get; init; }

    public int TotalRecords { get; init; }

    public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
}
