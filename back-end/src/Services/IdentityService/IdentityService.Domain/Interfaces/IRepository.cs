using IdentityService.Domain.Common;

namespace IdentityService.Domain.Interfaces;

public interface IRepository<TEntity, TKey> where TEntity : BaseEntity<TKey>
{
    /// <summary>
    /// Lấy tất cả các đối tượng
    /// </summary>
    /// <param name="func">Một hàm để xây dựng truy vấn</param>
    /// <param name="includeDeleted">Một giá trị cho biết có bao gồm các bản ghi đã xóa hay không (đối với các đối tượng bị xóa mềm)</param>
    /// <returns></returns>
    Task<IList<TEntity>> GetAllAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>>? func = null, bool includeDeleted = false);

    /// <summary>
    /// Lấy danh sách các đối tượng theo phân trang
    /// </summary>
    /// <param name="func"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="getOnlyTotalCount"></param>
    /// <param name="includeDeleted"></param>
    /// <returns></returns>
    Task<IPagedList<TEntity>> GetLstPagedAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>>? func = null,
        int pageIndex = 1, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, bool includeDeleted = false);

    /// <summary>
    /// Lấy một đối tượng theo Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<TEntity?> GetByIdAsync(TKey id);

    /// <summary>
    /// Thêm một đối tượng mới vào cơ sở dữ liệu
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task AddAsync(TEntity entity);

    /// <summary>
    /// Thêm nhiều đối tượng mới vào cơ sở dữ liệu
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    Task AddRangeAsync(IEnumerable<TEntity> entities);

    /// <summary>
    /// Cập nhật một đối tượng trong cơ sở dữ liệu
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task UpdateAsync(TEntity entity);

    /// <summary>
    /// Cập nhật nhiều đối tượng trong cơ sở dữ liệu
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    Task UpdateRangeAsync(IEnumerable<TEntity> entities);

    /// <summary>
    /// Xóa một đối tượng khỏi cơ sở dữ liệu
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task DeleteAsync(TEntity entity);

    /// <summary>
    /// Xóa nhiều đối tượng khỏi cơ sở dữ liệu
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    Task DeleteRangeAsync(IEnumerable<TEntity> entities);
}
