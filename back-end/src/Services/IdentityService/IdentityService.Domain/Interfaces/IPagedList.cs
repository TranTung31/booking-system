namespace IdentityService.Domain.Interfaces;

public partial interface IPagedList<T> : IList<T>
{
    /// <summary>
    /// Page index
    /// </summary>
    int PageIndex { get; }

    /// <summary>
    /// Page size
    /// </summary>
    int PageSize { get; }

    /// <summary>
    /// Total count
    /// </summary>
    int TotalCount { get; }

    /// <summary>
    /// Total pages
    /// </summary>
    int TotalPages { get; }

    /// <summary>
    /// Has previous page
    /// </summary>
    bool HasPreviousPage { get; }

    /// <summary>
    /// Has next page
    /// </summary>
    bool HasNextPage { get; }
}
