using IdentityService.Domain.Interfaces;

namespace IdentityService.Application.Dtos.Common;

public class PageListDto<T> : List<T>, IPagedList<T>
{
    public int PageIndex { get; }

    public int PageSize { get; }

    public int TotalCount { get; }

    public int TotalPages { get; }

    public bool HasPreviousPage => PageIndex > 0;

    public bool HasNextPage => PageIndex + 1 < TotalPages;

    public PageListDto(IEnumerable<T> source, int pageIndex, int pageSize, int totalCount)
    {
        TotalCount = totalCount;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        PageSize = pageSize;
        PageIndex = pageIndex;
        AddRange(source);
    }
}
