namespace IdentityService.Application.Dtos.Common;

public abstract record BaseSearchDto
{
    public string Keyword { get; set; } = string.Empty;

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}
