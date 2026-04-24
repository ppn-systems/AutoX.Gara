// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Abstractions;
using AutoX.Gara.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoX.Gara.Infrastructure.Repositories;

/// <summary>
/// Repository chung cho t?t c? entity.
/// S? d?ng Entity Framework Core.
/// </summary>
/// <typeparam name="T">Ki?u entity.</typeparam>
public class DataRepository<T>(AutoXDbContext context) where T : class
{
    #region Fields

    private readonly AutoXDbContext _context = context
        ?? throw new System.ArgumentNullException(nameof(context));

    private readonly DbSet<T> _dbSet = context.Set<T>();

    #endregion Fields

    #region Query APIs

    /// <summary>
    /// Tr? v? <see cref="IQueryable{T}"/> c?a DbSet d? caller t? compose
    /// th�m filter / sort / projection tru?c khi th?c thi.
    /// <para>
    /// D�ng <see cref="AsNoTracking"/> d? tr�nh tracking overhead tr�n read-only queries.
    /// </para>
    /// </summary>
    public IQueryable<T> AsQueryable() => _dbSet.AsNoTracking().AsQueryable();

    /// <summary>
    /// L?y t?t c? entity theo trang (kh�ng filter, kh�ng sort).
    /// </summary>
    /// <param name="pageNumber">S? trang (b?t d?u t? 1).</param>
    /// <param name="pageSize">S? ph?n t? tr�n m?i trang.</param>
    /// <param name="cancellationToken">Token h?y th?c thi.</param>
    public System.Threading.Tasks.Task<List<T>> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 10,
        System.Threading.CancellationToken cancellationToken = default)
        => _dbSet.AsNoTracking()
                 .Skip((pageNumber - 1) * pageSize)
                 .Take(pageSize)
                 .ToListAsync(cancellationToken);

    /// <summary>
    /// Th?c thi ph�n trang tr�n m?t <see cref="IQueryable{T}"/> d� được compose s?n
    /// (filter + sort �p d?ng b�n ngo�i).
    /// D�ng k?t h?p v?i <see cref="AsQueryable"/> d? t�ch bi?t concern.
    /// </summary>
    /// <param name="query">Query d� c� filter v� sort.</param>
    /// <param name="pageNumber">S? trang (b?t d?u t? 1).</param>
    /// <param name="pageSize">S? ph?n t? tr�n m?i trang.</param>
    /// <param name="cancellationToken">Token h?y.</param>
    /// <returns>Trang dữ liệu tuong ?ng.</returns>
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
    public System.Threading.Tasks.Task<List<T>> GetPagedAsync(
        IQueryable<T> query,
        int pageNumber = 1,
        int pageSize = 10,
        System.Threading.CancellationToken cancellationToken = default)
        => query.Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

    /// <summary>
    /// �?m t?ng s? entity trong to�n b? b?ng (kh�ng filter).
    /// </summary>
    public System.Threading.Tasks.Task<int> CountAsync(
        System.Threading.CancellationToken cancellationToken = default)
        => _dbSet.CountAsync(cancellationToken);

    /// <summary>
    /// �?m s? entity kh?p v?i m?t <see cref="IQueryable{T}"/> d� được compose s?n.
    /// D�ng d? l?y <c>TotalCount</c> tru?c khi ph�n trang.
    /// </summary>
    /// <param name="query">Query d� c� filter (chua Skip/Take).</param>
    /// <param name="cancellationToken">Token h?y.</param>
    public System.Threading.Tasks.Task<int> CountAsync(
        IQueryable<T> query,
        System.Threading.CancellationToken cancellationToken = default)
        => query.CountAsync(cancellationToken);

    /// <summary>
    /// Ki?m tra c� entity n�o th?a m�n di?u ki?n kh�ng.
    /// </summary>
    public System.Threading.Tasks.Task<bool> AnyAsync(
        System.Linq.Expressions.Expression<System.Func<T, bool>> predicate,
        System.Threading.CancellationToken cancellationToken = default)
        => _dbSet.AnyAsync(predicate, cancellationToken);

    /// <summary>
    /// L?y entity theo kh�a ch�nh.
    /// </summary>
    /// <param name="id">Gi� tr? kh�a ch�nh.</param>
    /// <param name="cancellationToken">Token h?y.</param>
    /// <returns>Entity t�m th?y, ho?c <c>null</c> n?u kh�ng c�.</returns>
    public System.Threading.Tasks.Task<T> GetByIdAsync(
        System.Object id,
        System.Threading.CancellationToken cancellationToken = default)
        => _dbSet.FindAsync([id], cancellationToken).AsTask();
    // ? Fix bug: cancellationToken KH�NG được nh�t v�o m?ng keyValues.
    //   FindAsync([id, ct]) khi?n EF d�ng ct nhu m?t composite key � runtime error.

    /// <summary>
    /// L?y danh s�ch entity v?i filter + sort + include + ph�n trang t�y ch?n.
    /// </summary>
    /// <param name="filter">Bi?u th?c l?c (nullable).</param>
    /// <param name="orderBy">Bi?u th?c s?p x?p (nullable).</param>
    /// <param name="includeProperties">T�n navigation property, ph�n c�ch b?ng d?u ph?y.</param>
    /// <param name="pageNumber">S? trang (b?t d?u t? 1).</param>
    /// <param name="pageSize">S? ph?n t?/trang.</param>
    /// <param name="cancellationToken">Token h?y.</param>
    public System.Threading.Tasks.Task<List<T>> GetAsync(
        System.Linq.Expressions.Expression<System.Func<T, bool>> filter = null,
        System.Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        string includeProperties = "",
        int pageNumber = 1,
        int pageSize = 10,
        System.Threading.CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet.AsQueryable();

        if (filter is not null)
        {
            query = query.Where(filter);
        }

        foreach (string prop in includeProperties.Split(
            ',', StringSplitOptions.RemoveEmptyEntries))
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
    /// T�m entity d?u ti�n th?a m�n predicate, ho?c <c>null</c>.
    /// </summary>
    public System.Threading.Tasks.Task<T> GetFirstOrDefaultAsync(
        System.Linq.Expressions.Expression<System.Func<T, bool>> predicate,
        System.Threading.CancellationToken cancellationToken = default)
        => _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate, cancellationToken);

    #endregion Query APIs

    #region Modification APIs

    /// <summary>Th�m m?i m?t entity.</summary>
    public System.Threading.Tasks.Task AddAsync(
        T entity,
        System.Threading.CancellationToken cancellationToken = default)
        => _dbSet.AddAsync(entity, cancellationToken).AsTask();

    /// <summary>Th�m m?i nhi?u entity.</summary>
    public System.Threading.Tasks.Task AddRangeAsync(
        IEnumerable<T> entities,
        System.Threading.CancellationToken cancellationToken = default)
        => _dbSet.AddRangeAsync(entities, cancellationToken);

    /// <summary>C?p nh?t m?t entity (dua v�o tr?ng th�i Modified).</summary>
    public void Update(T entity) => _dbSet.Update(entity);

    /// <summary>X�a entity theo kh�a ch�nh.</summary>
    public async System.Threading.Tasks.Task DeleteAsync(
        System.Object id,
        System.Threading.CancellationToken cancellationToken = default)
    {
        // Fix bug: cancellationToken kh�ng được nh�t v�o keyValues array
        T entity = await _dbSet.FindAsync([id], cancellationToken);
        if (entity is not null)
        {
            Delete(entity);
        }
    }

    /// <summary>X�a m?t entity d� được tracked.</summary>
    public void Delete(T entity)
    {
        if (entity is ISoftDelete softDeleteEntity)
        {
            softDeleteEntity.DeletedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
            return;
        }

        _dbSet.Remove(entity);
    }

    /// <summary>X�a nhi?u entity.</summary>
    public void DeleteRange(IEnumerable<T> entities)
    {
        foreach (T entity in entities)
        {
            Delete(entity);
        }
    }

    /// <summary>Luu t?t c? thay d?i v�o database.</summary>
    public System.Threading.Tasks.Task<int> SaveChangesAsync(
        System.Threading.CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);

    #endregion Modification APIs
}

