using AutoMapper;
using IdentityService.Application.Dtos.Common;
using IdentityService.Domain.Interfaces;

namespace IdentityService.Application.Extensions;

public static class MappingEtensions
{
    public static IPagedList<TModel> ToModel<TModel, TEntity>(this IPagedList<TEntity> pagedList, IMapper mapper) where TModel : BaseModel where TEntity : class
    {
        var destinationData = mapper.Map<IEnumerable<TModel>>(pagedList);
        return new PageListDto<TModel>(destinationData, pagedList.PageIndex, pagedList.PageSize, pagedList.TotalCount);
    }
}
