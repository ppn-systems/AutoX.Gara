// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Billings;
using AutoX.Gara.Domain.Entities.Customers;
using AutoX.Gara.Domain.Entities.Identity;
using AutoX.Gara.Domain.Entities.Inventory;
using AutoX.Gara.Domain.Entities.Invoices;
using AutoX.Gara.Domain.Entities.Repairs;
using AutoX.Gara.Domain.Entities.Suppliers;
using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Infrastructure.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AutoX.Gara.Infrastructure.Database;

/// <summary>
/// DbContext cho ứng dụng quản lý gara ô tô.
/// </summary>
public sealed class AutoXDbContext(DbContextOptions<AutoXDbContext> options) : DbContext(options), IAutoXDbContext
{
    #region Properties

    /// <inheritdoc/>
    public DbSet<Part> Parts { get; set; }

    /// <inheritdoc/>
    public DbSet<Vehicle> Vehicles { get; set; }

    /// <inheritdoc/>
    public DbSet<Account> Accounts { get; set; }

    /// <inheritdoc/>
    public DbSet<Invoice> Invoices { get; set; }

    /// <inheritdoc/>
    public DbSet<Customer> Customers { get; set; }

    /// <inheritdoc/>
    public DbSet<Employee> Employees { get; set; }

    /// <inheritdoc/>
    public DbSet<Supplier> Suppliers { get; set; }

    /// <inheritdoc/>
    public DbSet<RepairTask> RepairTasks { get; set; }

    /// <inheritdoc/>
    public DbSet<ServiceItem> ServiceItems { get; set; }

    /// <inheritdoc/>
    public DbSet<RepairOrder> RepairOrders { get; set; }

    /// <inheritdoc/>
    public DbSet<Transaction> Transactions { get; set; }

    /// <inheritdoc/>
    public DbSet<RepairOrderItem> RepairOrderItems { get; set; }

    /// <inheritdoc/>
    public DbSet<SupplierContactPhone> SupplierContactPhones { get; set; }

    #endregion Properties

    #region APIs

    /// <inheritdoc/>
    public new System.Int32 SaveChanges() => base.SaveChanges();

    #endregion APIs

    #region Override

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UsePropertyAccessMode(PropertyAccessMode.Field);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AutoXDbContext).Assembly);
        base.OnModelCreating(modelBuilder);

        CONFIGURE_ACCOUNT(modelBuilder);
        CONFIGURE_VEHICLE(modelBuilder);
        CONFIGURE_INVOICE(modelBuilder);
        CONFIGURE_CUSTOMER(modelBuilder);
        CONFIGURE_EMPLOYEE(modelBuilder);
        CONFIGURE_SUPPLIER(modelBuilder);
        CONFIGURE_PART(modelBuilder);
        CONFIGURE_REPAIR_TASK(modelBuilder);
        CONFIGURE_SERVICE_ITEM(modelBuilder);
        CONFIGURE_REPAIR_ORDER(modelBuilder);
        CONFIGURE_TRANSACTION(modelBuilder);
        CONFIGURE_REPAIR_ORDER_ITEMS(modelBuilder);
    }

    #endregion Override

    #region Private Methods

    private static void CONFIGURE_ACCOUNT(ModelBuilder modelBuilder)
    {
        // Đảm bảo Username là duy nhất để tránh trùng lặp tài khoản trong hệ thống
        modelBuilder.Entity<Account>()
                    .HasIndex(a => a.Username)
                    .IsUnique();

        // Chuyển đổi thuộc tính Role (kiểu enum) thành byte khi lưu vào cơ sở dữ liệu
        // Giúp giảm kích thước cột, tối ưu hiệu suất lưu trữ và truy vấn
        modelBuilder.Entity<Account>()
                    .Property(a => a.Role)
                    .HasConversion<System.Byte>();
    }

    private static void CONFIGURE_VEHICLE(ModelBuilder modelBuilder)
    {
        // Chuyển đổi enum thành byte để tiết kiệm dung lượng và tối ưu hiệu suất truy vấn
        modelBuilder.Entity<Vehicle>()
                    .Property(v => v.Brand)
                    .HasConversion<System.Byte>();

        modelBuilder.Entity<Vehicle>()
                    .Property(v => v.Type)
                    .HasConversion<System.Byte>();

        modelBuilder.Entity<Vehicle>()
                    .Property(v => v.Color)
                    .HasConversion<System.Byte>();

        // Đảm bảo mỗi biển số xe là duy nhất để tránh trùng lặp dữ liệu
        modelBuilder.Entity<Vehicle>()
                    .HasIndex(v => v.LicensePlate)
                    .IsUnique();

        // Index cho Id giúp tối ưu truy vấn khi tìm kiếm xe theo chủ sở hữu
        modelBuilder.Entity<Vehicle>()
                    .HasIndex(v => v.CustomerId);

        // Index cho CarBrand giúp tăng tốc tìm kiếm xe theo hãng
        modelBuilder.Entity<Vehicle>()
                    .HasIndex(v => v.Brand);

        // Index phức hợp giúp tối ưu truy vấn khi lọc theo nhiều tiêu chí
        modelBuilder.Entity<Vehicle>()
                    .HasIndex(v => new { v.Brand, v.Type, v.Color, v.Year });
    }

    private static void CONFIGURE_INVOICE(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Invoice>()
                        .Property(i => i.Discount)
                        .HasPrecision(18, 2);

        modelBuilder.Entity<Invoice>()
                    .Property(i => i.Subtotal)
                    .HasPrecision(18, 2);

        modelBuilder.Entity<Invoice>()
                    .Property(i => i.TaxAmount)
                    .HasPrecision(18, 2);

        modelBuilder.Entity<Invoice>()
                    .Property(i => i.TotalAmount)
                    .HasPrecision(18, 2);

        modelBuilder.Entity<Invoice>()
                    .Property(i => i.BalanceDue)
                    .HasPrecision(18, 2);

        modelBuilder.Entity<Invoice>()
                    .Property(i => i.PaymentStatus)
                    .HasConversion<System.Byte>();

        modelBuilder.Entity<Invoice>()
                    .Property(i => i.TaxRate)
                    .HasConversion<System.Byte>();

        modelBuilder.Entity<Invoice>()
                    .Property(i => i.DiscountType)
                    .HasConversion<System.Byte>();

        modelBuilder.Entity<Invoice>()
                    .HasIndex(i => i.InvoiceNumber)
                    .IsUnique();

        modelBuilder.Entity<Invoice>().HasIndex(i => i.CustomerId);
        modelBuilder.Entity<Invoice>().HasIndex(i => i.InvoiceDate);
    }

    private static void CONFIGURE_CUSTOMER(ModelBuilder modelBuilder)
    {
        // Đảm bảo email khách hàng là duy nhất
        modelBuilder.Entity<Customer>()
            .HasIndex(c => c.Email)
            .IsUnique();

        // Đảm bảo số điện thoại khách hàng là duy nhất
        modelBuilder.Entity<Customer>()
            .HasIndex(c => c.PhoneNumber)
            .IsUnique();

        // Index hỗ trợ truy vấn theo mã số thuế và theo tên
        modelBuilder.Entity<Customer>().HasIndex(c => c.TaxCode);
        modelBuilder.Entity<Customer>().HasIndex(c => c.Name);
    }

    private static void CONFIGURE_EMPLOYEE(ModelBuilder modelBuilder)
    {
        // Đảm bảo email nhân viên là duy nhất
        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.Email)
            .IsUnique();

        // Index giúp tối ưu truy vấn theo số điện thoại và trạng thái làm việc
        modelBuilder.Entity<Employee>().HasIndex(e => e.Name);
        modelBuilder.Entity<Employee>().HasIndex(e => e.Position);
        modelBuilder.Entity<Employee>().HasIndex(e => e.Status);
        modelBuilder.Entity<Employee>().HasIndex(e => e.StartDate);
        modelBuilder.Entity<Employee>().HasIndex(e => e.Gender);

        // Chuyển đổi enum thành byte để tối ưu lưu trữ
        modelBuilder.Entity<Employee>()
            .Property(e => e.Gender)
            .HasConversion<System.Byte>();

        modelBuilder.Entity<Employee>()
            .Property(e => e.Position)
            .HasConversion<System.Byte>();

        modelBuilder.Entity<Employee>()
            .Property(e => e.Status)
            .HasConversion<System.Byte>();
    }

    private static void CONFIGURE_SUPPLIER(ModelBuilder modelBuilder)
    {
        // Đảm bảo email và mã số thuế của nhà cung cấp là duy nhất
        modelBuilder.Entity<Supplier>()
            .HasIndex(s => s.Email)
            .IsUnique();

        modelBuilder.Entity<Supplier>()
            .HasIndex(s => s.TaxCode)
            .IsUnique();

        // Index giúp tối ưu truy vấn theo trạng thái nhà cung cấp
        modelBuilder.Entity<Supplier>()
            .HasIndex(s => s.Status);

        // Quan hệ 1-N giữa Supplier và PhoneNumbers
        modelBuilder.Entity<Supplier>()
            .HasMany(s => s.PhoneNumbers)
            .WithOne(sp => sp.Supplier)
            .HasForeignKey(sp => sp.SupplierId)
            .OnDelete(DeleteBehavior.Cascade); // Xóa Supplier sẽ xóa luôn danh sách số điện thoại

        // Tạo index cho (Id, PhoneNumber) giúp tối ưu tìm kiếm
        modelBuilder.Entity<SupplierContactPhone>().HasIndex(sp => new { sp.SupplierId, sp.PhoneNumber });

        modelBuilder.Entity<Supplier>()
            .HasQueryFilter(s => s.Status == SupplierStatus.Active);
    }

    private static void CONFIGURE_PART(ModelBuilder modelBuilder)
    {
        // Cấu hình độ chính xác cho giá
        modelBuilder.Entity<Part>()
            .Property(p => p.PurchasePrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Part>()
            .Property(p => p.SellingPrice)
            .HasPrecision(18, 2);

        // Đảm bảo mã phụ tùng là duy nhất
        modelBuilder.Entity<Part>()
            .HasIndex(p => p.PartCode)
            .IsUnique();

        // Index tối ưu tìm kiếm theo tên
        modelBuilder.Entity<Part>().HasIndex(p => p.PartName);

        // Index hỗ trợ tìm kiếm nhanh theo nhà sản xuất
        modelBuilder.Entity<Part>().HasIndex(p => p.Manufacturer);

        // Index hỗ trợ filter theo supplier
        modelBuilder.Entity<Part>().HasIndex(p => p.SupplierId);

        // Index phức hợp cho filter theo nhiều tiêu chí
        modelBuilder.Entity<Part>()
            .HasIndex(p => new { p.IsDiscontinued, p.IsDefective, p.InventoryQuantity });

        // Quan hệ 1-N với Supplier
        modelBuilder.Entity<Part>()
            .HasOne(p => p.Supplier)
            .WithMany(s => s.Parts)
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        // Convert DateOnly
        modelBuilder.Entity<Part>()
            .Property(p => p.DateAdded)
            .HasConversion(
                d => d.ToDateTime(System.TimeOnly.MinValue),
                dt => System.DateOnly.FromDateTime(dt));

        modelBuilder.Entity<Part>()
            .Property(p => p.ExpiryDate)
            .HasConversion(
                d => d.HasValue ? d.Value.ToDateTime(System.TimeOnly.MinValue) : default(System.DateTime?),
                dt => dt != null ? System.DateOnly.FromDateTime(dt.Value) : null);

        // Query filter: loại trừ các phụ tùng đã ngừng bán
        // Có thể bỏ comment nếu không muốn auto-filter
        // modelBuilder.Entity<Part>().HasQueryFilter(p => !p.IsDiscontinued);
    }

    private static void CONFIGURE_REPAIR_TASK(ModelBuilder modelBuilder)
    {
        // Quan hệ 1-N với ServiceItem
        modelBuilder.Entity<RepairTask>()
                    .HasOne(rt => rt.ServiceItem)
                    .WithMany()
                    .HasForeignKey(rt => rt.ServiceItemId)
                    .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RepairTask>()
                    .HasOne(rt => rt.Employee)
                    .WithMany()
                    .HasForeignKey(rt => rt.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

        // Index tối ưu tìm kiếm và truy vấn
        modelBuilder.Entity<RepairTask>().HasIndex(rt => rt.Status);
        modelBuilder.Entity<RepairTask>().HasIndex(rt => new { rt.StartDate, rt.CompletionDate });
        modelBuilder.Entity<RepairTask>().HasIndex(rt => rt.EmployeeId);
    }

    private static void CONFIGURE_SERVICE_ITEM(ModelBuilder modelBuilder)
    {
        // Index giúp tối ưu tìm kiếm theo mô tả dịch vụ và loại dịch vụ
        modelBuilder.Entity<ServiceItem>().HasIndex(si => si.Description);
        modelBuilder.Entity<ServiceItem>().HasIndex(si => si.Type);

        modelBuilder.Entity<ServiceItem>()
            .Property(si => si.UnitPrice)
            .HasPrecision(18, 2);
    }

    private static void CONFIGURE_REPAIR_ORDER_ITEMS(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RepairOrderItem>()
                    .HasOne(rsp => rsp.RepairOrder)
                    .WithMany(ro => ro.Parts)
                    .HasForeignKey(rsp => rsp.RepairOrderId)
                    .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RepairOrderItem>()
                    .HasOne(rsp => rsp.SparePart)
                    .WithMany()
                    .HasForeignKey(rsp => rsp.PartId)
                    .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RepairOrderItem>()
                    .HasIndex(rsp => rsp.RepairOrderId);

        modelBuilder.Entity<RepairOrderItem>()
                    .HasIndex(rsp => rsp.PartId);
    }

    private static void CONFIGURE_REPAIR_ORDER(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RepairOrder>()
                    .HasMany(ro => ro.Tasks)
                    .WithOne(rt => rt.RepairOrder) // Chỉ định rõ ràng quan hệ với RepairTask
                    .HasForeignKey(rt => rt.RepairOrderId) // Đảm bảo có thuộc tính khoá ngoại trong RepairTask
                    .OnDelete(DeleteBehavior.Cascade); // Xóa RepairOrder sẽ xóa luôn RepairTask

        modelBuilder.Entity<RepairOrder>()
                    .HasMany(ro => ro.Parts)
                    .WithOne(rsp => rsp.RepairOrder) // Chỉ định rõ ràng quan hệ với RepairOrderSparePart
                    .HasForeignKey(rsp => rsp.RepairOrderId) // Đảm bảo có thuộc tính khoá ngoại trong RepairOrderSparePart
                    .OnDelete(DeleteBehavior.Cascade); // Xóa RepairOrder sẽ xóa luôn RepairOrderSparePart

        modelBuilder.Entity<RepairOrder>()
                    .HasOne(ro => ro.Invoice)
                    .WithMany(i => i.RepairOrders)
                    .HasForeignKey(ro => ro.InvoiceId)
                    .OnDelete(DeleteBehavior.SetNull);

        // Tạo index tối ưu truy vấn
        modelBuilder.Entity<RepairOrder>().HasIndex(ro => ro.VehicleId);
        modelBuilder.Entity<RepairOrder>().HasIndex(ro => ro.InvoiceId);
        modelBuilder.Entity<RepairOrder>().HasIndex(ro => ro.CustomerId);
    }

    private static void CONFIGURE_TRANSACTION(ModelBuilder modelBuilder)
    {
        // Chuyển đổi enum Type thành byte để lưu trữ hiệu quả hơn
        modelBuilder.Entity<Transaction>()
            .Property(t => t.Type)
            .HasConversion<System.Byte>();

        modelBuilder.Entity<Transaction>()
            .Property(t => t.PaymentMethod)
            .HasConversion<System.Byte>();

        modelBuilder.Entity<Transaction>()
            .Property(t => t.Status)
            .HasConversion<System.Byte>();

        modelBuilder.Entity<Transaction>()
            .Property(t => t.Amount)
            .HasPrecision(18, 2);

        // Thiết lập quan hệ với Invoice
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Invoice)
            .WithMany(i => i.Transactions)
            .HasForeignKey(t => t.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Transaction>().HasIndex(t => t.InvoiceId);
        modelBuilder.Entity<Transaction>().HasIndex(t => t.Status);
        modelBuilder.Entity<Transaction>().HasIndex(t => t.Type);
        modelBuilder.Entity<Transaction>().HasIndex(t => t.TransactionDate);
    }

    #endregion Private Methods
}