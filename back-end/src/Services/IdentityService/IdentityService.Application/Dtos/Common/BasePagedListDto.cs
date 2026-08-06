namespace IdentityService.Application.Dtos.Common;

public partial record BaseModel
{
}

public abstract record BasePagedListDto<TModel> : BaseModel where TModel : BaseModel
{
    public IEnumerable<TModel> Data { get; set; } = Enumerable.Empty<TModel>();

    public PaginationDto Pagination { get; set; } = new PaginationDto();
}
