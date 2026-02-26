// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AutoX.Gara.Infrastructure.Database;

/// <summary>
/// Repository chung cho tất cả entity.
/// Sử dụng Entity Framework Core.
/// </summary>
/// <typeparam name="T">Kiểu entity.</typeparam>
public class DataRepository<T>(AutoXDbContext context) where T : class
{
    #region Fields

    private readonly AutoXDbContext _context = context ?? throw new System.ArgumentNullException(nameof(context));
    private readonly DbSet<T> _dbSet = context.Set<T>();

    #endregion Fields

    #region Query APIs

    /// <summary>
    /// Lấy tất cả entity theo trang (async).
    /// </summary>
    /// <param name="pageNumber">Số trang.</param>
    /// <param name="pageSize">Số phần tử trên mỗi trang.</param>
    /// <param name="cancellationToken">Token hủy thực thi.</param>
    /// <returns>Danh sách entity.</returns>
    public async System.Threading.Tasks.Task<System.Collections.Generic.List<T>> GetAllAsync(
        System.Int32 pageNumber = 1, System.Int32 pageSize = 10,
        System.Threading.CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking()
                       .Skip((pageNumber - 1) * pageSize)
                       .Take(pageSize)
                       .ToListAsync(cancellationToken);

    /// <summary>
    /// Đếm tổng số entity.
    /// </summary>
    /// <param name="cancellationToken">Token hủy.</param>
    /// <returns>Số lượng.</returns>
    public async System.Threading.Tasks.Task<System.Int32> CountAsync(
        System.Threading.CancellationToken cancellationToken = default)
        => await _dbSet.CountAsync(cancellationToken);

    /// <summary>
    /// Kiểm tra có entity nào thỏa mãn điều kiện không.
    /// </summary>
    /// <param name="predicate">Biểu thức điều kiện.</param>
    /// <param name="cancellationToken">Token hủy.</param>
    /// <returns>true nếu có, false nếu không.</returns>
    public async System.Threading.Tasks.Task<System.Boolean> AnyAsync(
        System.Linq.Expressions.Expression<System.Func<T, System.Boolean>> predicate,
        System.Threading.CancellationToken cancellationToken = default)
        => await _dbSet.AnyAsync(predicate, cancellationToken);

    /// <summary>
    /// Lấy entity theo id (khoá chính, bản ghi).
    /// </summary>
    public async System.Threading.Tasks.Task<T> GetByIdAsync(System.Object id, System.Threading.CancellationToken cancellationToken = default)
        => await _dbSet.FindAsync([id, cancellationToken], cancellationToken: cancellationToken);

    /// <summary>
    /// Lấy danh sách entity thỏa mãn điều kiện (có trang, include, order).
    /// </summary>
    /// <param name="filter">Biểu thức lọc.</param>
    /// <param name="orderBy">Biểu thức sắp xếp (nullable).</param>
    /// <param name="includeProperties">Danh sách property include, phân cách bằng dấu phẩy.</param>
    /// <param name="pageNumber">Số trang.</param>
    /// <param name="pageSize">Số phần tử/trang.</param>
    /// <param name="cancellationToken">Token hủy.</param>
    /// <returns>Danh sách entity.</returns>
    public async System.Threading.Tasks.Task<System.Collections.Generic.List<T>> GetAsync(
        System.Linq.Expressions.Expression<System.Func<T, System.Boolean>> filter = null,
        System.Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        System.String includeProperties = "",
        System.Int32 pageNumber = 1, System.Int32 pageSize = 10,
        System.Threading.CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet.AsQueryable();

        if (filter is not null)
        {
            query = query.Where(filter);
        }

        foreach (var includeProp in includeProperties.Split(',', System.StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(includeProp.Trim());
        }

        if (orderBy is not null)
        {
            query = orderBy(query);
        }

        query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

        return await query.AsNoTracking().ToListAsync(cancellationToken);
    }

    #endregion Query APIs

    #region Modification APIs

    /// <summary>
    /// Thêm mới một entity.
    /// </summary>
    public async System.Threading.Tasks.Task AddAsync(
        T entity,
        System.Threading.CancellationToken cancellationToken = default)
        => await _dbSet.AddAsync(entity, cancellationToken);

    /// <summary>
    /// Thêm mới nhiều entity.
    /// </summary>
    public async System.Threading.Tasks.Task AddRangeAsync(
        System.Collections.Generic.IEnumerable<T> entities,
        System.Threading.CancellationToken cancellationToken = default)
        => await _dbSet.AddRangeAsync(entities, cancellationToken);

    /// <summary>
    /// Cập nhật một entity (đưa vào trạng thái Modified).
    /// </summary>
    public void Update(T entity) => _dbSet.Update(entity);

    /// <summary>
    /// Xoá entity theo id.
    /// </summary>
    public async System.Threading.Tasks.Task DeleteAsync(System.Object id, System.Threading.CancellationToken cancellationToken = default)
    {
        T entity = await _dbSet.FindAsync([id, cancellationToken], cancellationToken: cancellationToken);
        if (entity is not null)
        {
            _dbSet.Remove(entity);
        }
    }

    /// <summary>
    /// Xoá một entity.
    /// </summary>
    public void Delete(T entity) => _dbSet.Remove(entity);

    /// <summary>
    /// Xoá nhiều entity.
    /// </summary>
    public void DeleteRange(System.Collections.Generic.IEnumerable<T> entities) => _dbSet.RemoveRange(entities);

    /// <summary>
    /// Lưu thay đổi (async).
    /// </summary>
    public async System.Threading.Tasks.Task<System.Int32> SaveChangesAsync(
        System.Threading.CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    #endregion Modification APIs

    #region Extra APIs

    /// <summary>
    /// Tìm kiếm entity đầu tiên phù hợp với predicate.
    /// </summary>
    public async System.Threading.Tasks.Task<T> GetFirstOrDefaultAsync(
        System.Linq.Expressions.Expression<System.Func<T, System.Boolean>> predicate,
        System.Threading.CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate, cancellationToken);

    #endregion Extra APIs
}