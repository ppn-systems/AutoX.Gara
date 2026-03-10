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

    private readonly AutoXDbContext _context = context
        ?? throw new System.ArgumentNullException(nameof(context));

    private readonly DbSet<T> _dbSet = context.Set<T>();

    #endregion Fields

    #region Query APIs

    /// <summary>
    /// Trả về <see cref="IQueryable{T}"/> của DbSet để caller tự compose
    /// thêm filter / sort / projection trước khi thực thi.
    /// <para>
    /// Dùng <see cref="AsNoTracking"/> để tránh tracking overhead trên read-only queries.
    /// </para>
    /// </summary>
    public IQueryable<T> AsQueryable() => _dbSet.AsNoTracking().AsQueryable();

    /// <summary>
    /// Lấy tất cả entity theo trang (không filter, không sort).
    /// </summary>
    /// <param name="pageNumber">Số trang (bắt đầu từ 1).</param>
    /// <param name="pageSize">Số phần tử trên mỗi trang.</param>
    /// <param name="cancellationToken">Token hủy thực thi.</param>
    public System.Threading.Tasks.Task<System.Collections.Generic.List<T>> GetAllAsync(
        System.Int32 pageNumber = 1,
        System.Int32 pageSize = 10,
        System.Threading.CancellationToken cancellationToken = default)
        => _dbSet.AsNoTracking()
                 .Skip((pageNumber - 1) * pageSize)
                 .Take(pageSize)
                 .ToListAsync(cancellationToken);

    /// <summary>
    /// Thực thi phân trang trên một <see cref="IQueryable{T}"/> đã được compose sẵn
    /// (filter + sort áp dụng bên ngoài).
    /// Dùng kết hợp với <see cref="AsQueryable"/> để tách biệt concern.
    /// </summary>
    /// <param name="query">Query đã có filter và sort.</param>
    /// <param name="pageNumber">Số trang (bắt đầu từ 1).</param>
    /// <param name="pageSize">Số phần tử trên mỗi trang.</param>
    /// <param name="cancellationToken">Token hủy.</param>
    /// <returns>Trang dữ liệu tương ứng.</returns>
    /// <example>
    /// <code>
    /// IQueryable&lt;Customer&gt; q = _repo.AsQueryable()
    ///     .Where(c => c.Name.Contains(term))
    ///     .OrderByDescending(c => c.CreatedAt);
    ///
    /// int total   = await _repo.CountAsync(q);
    /// var page    = await _repo.GetPagedAsync(q, page: 2, pageSize: 20);
    /// </code>
    /// </example>
    public System.Threading.Tasks.Task<System.Collections.Generic.List<T>> GetPagedAsync(
        IQueryable<T> query,
        System.Int32 pageNumber = 1,
        System.Int32 pageSize = 10,
        System.Threading.CancellationToken cancellationToken = default)
        => query.Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

    /// <summary>
    /// Đếm tổng số entity trong toàn bộ bảng (không filter).
    /// </summary>
    public System.Threading.Tasks.Task<System.Int32> CountAsync(
        System.Threading.CancellationToken cancellationToken = default)
        => _dbSet.CountAsync(cancellationToken);

    /// <summary>
    /// Đếm số entity khớp với một <see cref="IQueryable{T}"/> đã được compose sẵn.
    /// Dùng để lấy <c>TotalCount</c> trước khi phân trang.
    /// </summary>
    /// <param name="query">Query đã có filter (chưa Skip/Take).</param>
    /// <param name="cancellationToken">Token hủy.</param>
    public System.Threading.Tasks.Task<System.Int32> CountAsync(
        IQueryable<T> query,
        System.Threading.CancellationToken cancellationToken = default)
        => query.CountAsync(cancellationToken);

    /// <summary>
    /// Kiểm tra có entity nào thỏa mãn điều kiện không.
    /// </summary>
    public System.Threading.Tasks.Task<System.Boolean> AnyAsync(
        System.Linq.Expressions.Expression<System.Func<T, System.Boolean>> predicate,
        System.Threading.CancellationToken cancellationToken = default)
        => _dbSet.AnyAsync(predicate, cancellationToken);

    /// <summary>
    /// Lấy entity theo khóa chính.
    /// </summary>
    /// <param name="id">Giá trị khóa chính.</param>
    /// <param name="cancellationToken">Token hủy.</param>
    /// <returns>Entity tìm thấy, hoặc <c>null</c> nếu không có.</returns>
    public System.Threading.Tasks.Task<T> GetByIdAsync(
        System.Object id,
        System.Threading.CancellationToken cancellationToken = default)
        => _dbSet.FindAsync([id], cancellationToken).AsTask();
    // ↑ Fix bug: cancellationToken KHÔNG được nhét vào mảng keyValues.
    //   FindAsync([id, ct]) khiến EF dùng ct như một composite key — runtime error.

    /// <summary>
    /// Lấy danh sách entity với filter + sort + include + phân trang tùy chọn.
    /// </summary>
    /// <param name="filter">Biểu thức lọc (nullable).</param>
    /// <param name="orderBy">Biểu thức sắp xếp (nullable).</param>
    /// <param name="includeProperties">Tên navigation property, phân cách bằng dấu phẩy.</param>
    /// <param name="pageNumber">Số trang (bắt đầu từ 1).</param>
    /// <param name="pageSize">Số phần tử/trang.</param>
    /// <param name="cancellationToken">Token hủy.</param>
    public System.Threading.Tasks.Task<System.Collections.Generic.List<T>> GetAsync(
        System.Linq.Expressions.Expression<System.Func<T, System.Boolean>> filter = null,
        System.Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        System.String includeProperties = "",
        System.Int32 pageNumber = 1,
        System.Int32 pageSize = 10,
        System.Threading.CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet.AsQueryable();

        if (filter is not null)
        {
            query = query.Where(filter);
        }

        foreach (System.String prop in includeProperties.Split(
            ',', System.StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(prop.Trim());
        }

        if (orderBy is not null)
        {
            query = orderBy(query);
        }

        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Tìm entity đầu tiên thỏa mãn predicate, hoặc <c>null</c>.
    /// </summary>
    public System.Threading.Tasks.Task<T> GetFirstOrDefaultAsync(
        System.Linq.Expressions.Expression<System.Func<T, System.Boolean>> predicate,
        System.Threading.CancellationToken cancellationToken = default)
        => _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate, cancellationToken);

    #endregion Query APIs

    #region Modification APIs

    /// <summary>Thêm mới một entity.</summary>
    public System.Threading.Tasks.Task AddAsync(
        T entity,
        System.Threading.CancellationToken cancellationToken = default)
        => _dbSet.AddAsync(entity, cancellationToken).AsTask();

    /// <summary>Thêm mới nhiều entity.</summary>
    public System.Threading.Tasks.Task AddRangeAsync(
        System.Collections.Generic.IEnumerable<T> entities,
        System.Threading.CancellationToken cancellationToken = default)
        => _dbSet.AddRangeAsync(entities, cancellationToken);

    /// <summary>Cập nhật một entity (đưa vào trạng thái Modified).</summary>
    public void Update(T entity) => _dbSet.Update(entity);

    /// <summary>Xóa entity theo khóa chính.</summary>
    public async System.Threading.Tasks.Task DeleteAsync(
        System.Object id,
        System.Threading.CancellationToken cancellationToken = default)
    {
        // Fix bug: cancellationToken không được nhét vào keyValues array
        T entity = await _dbSet.FindAsync([id], cancellationToken);
        if (entity is not null)
        {
            _dbSet.Remove(entity);
        }
    }

    /// <summary>Xóa một entity đã được tracked.</summary>
    public void Delete(T entity) => _dbSet.Remove(entity);

    /// <summary>Xóa nhiều entity.</summary>
    public void DeleteRange(System.Collections.Generic.IEnumerable<T> entities)
        => _dbSet.RemoveRange(entities);

    /// <summary>Lưu tất cả thay đổi vào database.</summary>
    public System.Threading.Tasks.Task<System.Int32> SaveChangesAsync(
        System.Threading.CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);

    #endregion Modification APIs
}