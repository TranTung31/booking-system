using IdentityService.Domain.Common;
using IdentityService.Domain.Interfaces;
using IdentityService.Infrastructure.Extensions;
using IdentityService.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Repositories;

public class EfRepository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : BaseEntity<TKey>
{
    private readonly AppIdentityDbContext _context;
    private DbSet<TEntity>? _entitySet;

    public EfRepository(AppIdentityDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    protected virtual DbSet<TEntity> Entities => _entitySet ??= _context.Set<TEntity>();

    public virtual IQueryable<TEntity> Table => Entities;

    public virtual async Task AddAsync(TEntity entity)
    {
        await Entities.AddAsync(entity);
    }

    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        await Entities.AddRangeAsync(entities);
    }

    protected virtual IQueryable<TEntity> AddDeletedFilter(IQueryable<TEntity> query, bool includeDeleted)
    {
        // Nếu includeDeleted là true, trả về query mà không áp dụng bộ lọc xóa mềm
        if (includeDeleted)
            return query;

        // Nếu TEntity triển khai ISoftDelete, áp dụng bộ lọc để loại bỏ các bản ghi đã xóa mềm
        if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity))) 
            query = query.Where(e => !((ISoftDelete)e).IsDeleted);

        return query;
    }

    public virtual Task DeleteAsync(TEntity entity)
    {
        switch (entity)
        {
            case ISoftDelete softDelete:
                softDelete.IsDeleted = true;
                Entities.Update(entity);
                break;

            default:
                Entities.Remove(entity);
                break;
        }

        return Task.CompletedTask;
    }

    public virtual Task DeleteRangeAsync(IEnumerable<TEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        var enumerable = entities.ToList();
        if (enumerable.Count == 0)
            return Task.CompletedTask;

        var softDeletes = enumerable.OfType<ISoftDelete>().ToList();
        var hardDeletes = enumerable.Where(e => e is not ISoftDelete).ToList();

        foreach (var softDelete in softDeletes)
            softDelete.IsDeleted = true;

        if (softDeletes.Count > 0)
            Entities.UpdateRange(softDeletes.Cast<TEntity>());

        if (hardDeletes.Count > 0)
            Entities.RemoveRange(hardDeletes);

        return Task.CompletedTask;
    }

    public virtual async Task<IList<TEntity>> GetAllAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>>? func = null, bool includeDeleted = false)
    {
        var query = AddDeletedFilter(Table, includeDeleted);

        query = func != null ? func(query) : query;

        return await query.ToListAsync();
    }

    public virtual async Task<IPagedList<TEntity>> GetLstPagedAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>>? func = null, int pageIndex = 1, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, bool includeDeleted = false)
    {
        var query = AddDeletedFilter(Table, includeDeleted);

        query = func != null ? func(query) : query;

        return await query.ToPagedListAsync(pageIndex, pageSize, getOnlyTotalCount);
    }

    public virtual async Task<TEntity?> GetByIdAsync(TKey id)
    {
        if (id == null)
            return null;

        var entity = await Entities.FindAsync(id);

        if (entity == null)
            return null;

        return entity;
    }

    public virtual Task UpdateAsync(TEntity entity)
    {
        Entities.Update(entity);

        return Task.CompletedTask;
    }

    public virtual Task UpdateRangeAsync(IEnumerable<TEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        Entities.UpdateRange(entities);

        return Task.CompletedTask;
    }
}
