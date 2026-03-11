// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Customers;
using AutoX.Gara.Shared.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AutoX.Gara.Infrastructure.Abstractions;

/// <summary>
/// Repository interface cho Customer domain.
/// Tách biệt domain/application logic khỏi EF Core chi tiết.
/// <para>
/// Nguyên tắc DDD: Application layer chỉ gọi interface này,
/// không import Microsoft.EntityFrameworkCore hay AutoXDbContext.
/// </para>
/// </summary>
public interface ICustomerRepository
{
    // ─── Query ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Lấy một trang khách hàng với filter / sort / phân trang.
    /// Trả về tuple gồm danh sách và tổng số bản ghi khớp filter (trước phân trang).
    /// </summary>
    Task<(List<Customer> Items, System.Int32 TotalCount)> GetPageAsync(
        CustomerListQuery query,
        CancellationToken ct = default);

    /// <summary>
    /// Lấy thông tin đầy đủ của một khách hàng theo Id.
    /// Trả về <c>null</c> nếu không tìm thấy hoặc đã bị soft-delete.
    /// </summary>
    Task<Customer> GetByIdAsync(System.Int32 id, CancellationToken ct = default);

    /// <summary>
    /// Kiểm tra xem email hoặc số điện thoại đã tồn tại trong DB chưa
    /// (chỉ xét bản ghi chưa bị soft-delete).
    /// </summary>
    Task<System.Boolean> ExistsByContactAsync(
        System.String email,
        System.String phoneNumber,
        CancellationToken ct = default);

    // ─── Write ────────────────────────────────────────────────────────────────

    /// <summary>Thêm mới entity vào DbSet (chưa SaveChanges).</summary>
    Task AddAsync(Customer customer, CancellationToken ct = default);

    /// <summary>Đánh dấu entity là Modified (chưa SaveChanges).</summary>
    void Update(Customer customer);

    /// <summary>Persist tất cả thay đổi đang chờ xuống DB.</summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}