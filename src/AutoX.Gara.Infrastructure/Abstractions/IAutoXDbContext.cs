// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Billing;
using AutoX.Gara.Domain.Entities.Customers;
using AutoX.Gara.Domain.Entities.Identity;
using AutoX.Gara.Domain.Entities.Inventory;
using AutoX.Gara.Domain.Entities.Repairs;
using Microsoft.EntityFrameworkCore;

namespace AutoX.Gara.Infrastructure.Abstractions;

/// <summary>
/// Định nghĩa abstraction cho DbContext của hệ thống gara AutoX.
/// </summary>
public interface IAutoXDbContext
{
    /// <summary>
    /// Tập hợp các phương tiện (xe) trong hệ thống.
    /// </summary>
    DbSet<Vehicle> Vehicles { get; set; }

    /// <summary>
    /// Tập hợp các tài khoản người dùng (đăng nhập, phân quyền).
    /// </summary>
    DbSet<Account> Accounts { get; set; }

    /// <summary>
    /// Tập hợp các hóa đơn thanh toán.
    /// </summary>
    DbSet<Invoice> Invoices { get; set; }

    /// <summary>
    /// Tập hợp thông tin khách hàng.
    /// </summary>
    DbSet<Customer> Customers { get; set; }

    /// <summary>
    /// Tập hợp nhân viên làm việc tại gara.
    /// </summary>
    DbSet<Employee> Employees { get; set; }

    /// <summary>
    /// Tập hợp nhà cung cấp phụ tùng / dịch vụ.
    /// </summary>
    DbSet<Supplier> Suppliers { get; set; }

    /// <summary>
    /// Tập hợp các phụ tùng trong kho.
    /// </summary>
    DbSet<SparePart> SpareParts { get; set; }

    /// <summary>
    /// Tập hợp các công việc sửa chữa (task).
    /// </summary>
    DbSet<RepairTask> RepairTasks { get; set; }

    /// <summary>
    /// Tập hợp các hạng mục dịch vụ (công, phí, dịch vụ).
    /// </summary>
    DbSet<ServiceItem> ServiceItems { get; set; }

    /// <summary>
    /// Tập hợp các lệnh sửa chữa (Repair Order).
    /// </summary>
    DbSet<RepairOrder> RepairOrders { get; set; }

    /// <summary>
    /// Tập hợp các giao dịch tài chính.
    /// </summary>
    DbSet<Transaction> Transactions { get; set; }

    /// <summary>
    /// Tập hợp các phụ tùng được thay thế trong quá trình sửa chữa.
    /// </summary>
    DbSet<ReplacementPart> ReplacementParts { get; set; }

    /// <summary>
    /// Tập hợp các dòng chi tiết trong lệnh sửa chữa.
    /// </summary>
    DbSet<RepairOrderItem> RepairOrderItems { get; set; }

    /// <summary>
    /// Tập hợp số điện thoại liên hệ của nhà cung cấp.
    /// </summary>
    DbSet<SupplierContactPhone> SupplierContactPhones { get; set; }

    /// <summary>
    /// Lưu tất cả các thay đổi được theo dõi vào cơ sở dữ liệu.
    /// </summary>
    /// <returns>
    /// Số lượng bản ghi bị ảnh hưởng.
    /// </returns>
    System.Int32 SaveChanges();
}