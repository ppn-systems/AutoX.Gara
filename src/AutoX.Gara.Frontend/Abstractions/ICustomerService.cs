// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Customers;
using AutoX.Gara.Frontend.ViewModels.Results;
using AutoX.Gara.Shared.Packets.Customers;

namespace AutoX.Gara.Frontend.Abstractions;

/// <summary>
/// Abstraction cho tất cả network operations liên quan đến Customer.
/// ViewModel chỉ phụ thuộc vào interface này — không biết về <c>ReliableClient</c>.
/// </summary>
public interface ICustomerService
{
    /// <summary>
    /// Lấy trang danh sách khách hàng.
    /// Cache 30 giây — không gửi request nếu đã có kết quả còn hạn.
    /// </summary>
    System.Threading.Tasks.Task<CustomerListResult> GetListAsync(
        System.Int32 page,
        System.Int32 pageSize,
        System.String? searchTerm = null,
        CustomerSortField sortBy = CustomerSortField.CreatedAt,
        System.Boolean sortDescending = true,
        CustomerType filterType = CustomerType.None,
        MembershipLevel filterMembership = MembershipLevel.None,
        System.Threading.CancellationToken ct = default);

    /// <summary>Tạo mới khách hàng. Server echo lại entity đã lưu trong <c>UpdatedEntity</c>.</summary>
    System.Threading.Tasks.Task<CustomerWriteResult> CreateAsync(
        CustomerDataPacket data,
        System.Threading.CancellationToken ct = default);

    /// <summary>Cập nhật khách hàng. Server echo lại entity đã lưu trong <c>UpdatedEntity</c>.</summary>
    System.Threading.Tasks.Task<CustomerWriteResult> UpdateAsync(
        CustomerDataPacket data,
        System.Threading.CancellationToken ct = default);

    /// <summary>Xóa mềm khách hàng. Server trả về Directive NONE khi thành công.</summary>
    System.Threading.Tasks.Task<CustomerWriteResult> DeleteAsync(
        CustomerDataPacket data,
        System.Threading.CancellationToken ct = default);
}