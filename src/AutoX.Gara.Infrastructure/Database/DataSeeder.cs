// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Billings;
using AutoX.Gara.Domain.Entities.Customers;
using AutoX.Gara.Domain.Entities.Identity;
using AutoX.Gara.Domain.Entities.Inventory;
using AutoX.Gara.Domain.Entities.Invoices;
using AutoX.Gara.Domain.Entities.Repairs;
using AutoX.Gara.Domain.Entities.Suppliers;
using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Cars;
using AutoX.Gara.Domain.Enums.Customers;
using AutoX.Gara.Domain.Enums.Employees;
using AutoX.Gara.Domain.Enums.Parts;
using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Domain.Enums.Repairs;
using AutoX.Gara.Domain.Enums.Transactions;
using Microsoft.EntityFrameworkCore;
using Nalix.Common.Security.Enums;
using Nalix.Shared.Security.Credentials;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoX.Gara.Infrastructure.Database;

/// <summary>
/// Class chịu trách nhiệm đổ dữ liệu mẫu (seed data) vào cơ sở dữ liệu.
/// Chỉ chạy khi database chưa có dữ liệu để tránh trùng lặp.
/// </summary>
public static class DataSeeder
{
    /// <summary>
    /// Entry point chính: gọi hàm này từ Program.cs khi khởi động ứng dụng.
    /// </summary>
    /// <example>
    /// // Trong Program.cs, gọi sau khi build app và trước app.Run():
    /// using (var scope = app.Services.CreateScope())
    /// {
    ///     var db = scope.ServiceProvider.GetRequiredService&lt;AutoXDbContext&gt;();
    ///     await DataSeeder.SeedAsync(db);
    /// }
    /// </example>
    public static async System.Threading.Tasks.Task SeedAsync(AutoXDbContext context)
    {
        await SeedAccountsAsync(context);
        await SeedCustomersAsync(context);
        await SeedEmployeesAsync(context);
        await SeedVehiclesAsync(context);
        await SeedSuppliersAsync(context);
        await SeedPartsAsync(context);
        await SeedServiceItemsAsync(context);
        await SeedRepairTasksAsync(context);
        await SeedRepairOrdersAsync(context);
        await SeedInvoicesAsync(context);
        await SeedTransactionsAsync(context);
    }

    // =========================================================================
    // ACCOUNT — 1 admin + 2 staff
    // =========================================================================

    /// <summary>
    /// Tài khoản mặc định:
    ///   admin     / Abcd1234@  — ADMINISTRATOR
    ///   nhanvien1 / Abcd1234@  — STAFF
    ///   nhanvien2 / Abcd1234@  — STAFF
    /// </summary>
    private static async System.Threading.Tasks.Task SeedAccountsAsync(AutoXDbContext context)
    {
        if (await context.Accounts.AnyAsync())
        {
            return;
        }

        static Account MakeAccount(String username, String password, PermissionLevel role, Boolean active = false)
        {
            // Pbkdf2.Hash là helper từ Nalix — phải khớp với logic xác thực trong hệ thống
            Pbkdf2.Hash(password, out System.Byte[] salt, out System.Byte[] hash);
            var acc = new Account
            {
                Username = username,
                Salt = salt,
                Hash = hash,
                Role = role,
                FailedLoginAttempts = 0,
                CreatedAt = DateTime.UtcNow,
            };

            return acc;
        }

        context.Accounts.AddRange(
            MakeAccount("admin", "Abcd1234@", PermissionLevel.ADMINISTRATOR),
            MakeAccount("nhanvien1", "Abcd1234@", PermissionLevel.USER),
            MakeAccount("nhanvien2", "Abcd1234@", PermissionLevel.USER)
        );

        await context.SaveChangesAsync();
    }

    // =========================================================================
    // CUSTOMER — 10 khách hàng
    // =========================================================================

    private static async System.Threading.Tasks.Task SeedCustomersAsync(AutoXDbContext context)
    {
        if (await context.Customers.AnyAsync())
        {
            return;
        }

        var customers = new List<Customer>
        {
            // --- Khách cá nhân ---
            new()
            {
                Name        = "Nguyễn Văn An",
                PhoneNumber = "0901234567",
                Email       = "an.nguyen@email.com",
                Address     = "123 Lý Thường Kiệt, Q.10, TP.HCM",
                Gender      = Gender.Male,
                DateOfBirth = new DateTime(1990, 5, 15, 0, 0, 0, DateTimeKind.Utc),
                TaxCode     = "0123456789",
                Type        = CustomerType.Individual,
                Membership  = MembershipLevel.Standard,
                Debt        = 0,
                Notes       = "Khách thân thiện, hay đến vào cuối tuần.",
            },
            new()
            {
                Name        = "Trần Thị Bình",
                PhoneNumber = "0912345678",
                Email       = "binh.tran@email.com",
                Address     = "456 Nguyễn Trãi, Q.5, TP.HCM",
                Gender      = Gender.Female,
                DateOfBirth = new DateTime(1985, 8, 22, 0, 0, 0, DateTimeKind.Utc),
                Type        = CustomerType.Individual,
                Membership  = MembershipLevel.Silver,
                Debt        = 500_000,
                Notes       = "Thường yêu cầu thay dầu định kỳ mỗi 3 tháng.",
            },
            new()
            {
                Name        = "Lê Hoàng Phúc",
                PhoneNumber = "0933456789",
                Email       = "phuc.le@email.com",
                Address     = "789 Cách Mạng Tháng 8, Q.3, TP.HCM",
                Gender      = Gender.Male,
                DateOfBirth = new DateTime(1995, 12, 1, 0, 0, 0, DateTimeKind.Utc),
                Type        = CustomerType.Individual,
                Membership  = MembershipLevel.Gold,
                Debt        = 1_200_000,
                Notes       = "Khách VIP, thường mang BMW đến bảo dưỡng.",
            },
            new()
            {
                Name        = "Phạm Thị Kim Chi",
                PhoneNumber = "0944567890",
                Email       = "chi.pham@email.com",
                Address     = "321 Võ Văn Tần, Q.3, TP.HCM",
                Gender      = Gender.Female,
                DateOfBirth = new DateTime(2000, 3, 8, 0, 0, 0, DateTimeKind.Utc),
                Type        = CustomerType.Individual,
                Membership  = MembershipLevel.Standard,
                Debt        = 0,
                Notes       = "Xe hay bị xịt lốp, cần kiểm tra áp suất định kỳ.",
            },
            new()
            {
                Name        = "Đặng Quốc Hùng",
                PhoneNumber = "0955678901",
                Email       = "hung.dang@email.com",
                Address     = "654 Phan Xích Long, Q.Phú Nhuận, TP.HCM",
                Gender      = Gender.Male,
                DateOfBirth = new DateTime(1978, 7, 19, 0, 0, 0, DateTimeKind.Utc),
                Type        = CustomerType.Fleet,
                Membership  = MembershipLevel.Platinum,
                Debt        = 5_000_000,
                Notes       = "Sở hữu đội xe 5 chiếc, ký hợp đồng bảo dưỡng hàng tháng.",
            },
            // --- Khách doanh nghiệp ---
            new()
            {
                Name        = "Công ty TNHH Vận Tải Phú Thịnh",
                PhoneNumber = "0283456789",
                Email       = "phuthinh.transport@company.vn",
                Address     = "789 Điện Biên Phủ, Q.Bình Thạnh, TP.HCM",
                Gender      = Gender.None,
                TaxCode     = "0312345678901",
                Type        = CustomerType.Business,
                Membership  = MembershipLevel.Gold,
                Debt        = 2_000_000,
                Notes       = "Đội xe tải, bảo dưỡng định kỳ hàng tháng.",
            },
            new()
            {
                Name        = "Công ty CP Grab Việt Nam",
                PhoneNumber = "0284567890",
                Email       = "fleet@grab.vn",
                Address     = "Tòa nhà Viettel, Mễ Trì, Hà Nội",
                Gender      = Gender.None,
                TaxCode     = "0106139890001",
                Type        = CustomerType.Fleet,
                Membership  = MembershipLevel.Platinum,
                Debt        = 0,
                Notes       = "Hợp đồng dài hạn, đội xe GrabCar 200 chiếc.",
            },
            new()
            {
                Name        = "Nguyễn Văn Xe Tăng",
                PhoneNumber = "0969696969",
                Email       = "xetang.t54@quandoi.vn",
                Address     = "Bộ Quốc Phòng, 7 Nguyễn Tri Phương, Hà Nội",
                Gender      = Gender.Male,
                DateOfBirth = new DateTime(1975, 4, 30, 0, 0, 0, DateTimeKind.Utc),
                Type        = CustomerType.Government,
                Membership  = MembershipLevel.Diamond,
                Debt        = 0,
                Notes       = "Mang xe tăng T-54 vào thay nhớt. Thợ bỏ chạy hết. Lần sau báo trước.",
            },
            new()
            {
                Name        = "Tỉ Phú ElonUsk",
                PhoneNumber = "0123456789",
                Email       = "elon.usk@spacex-gara.vn",
                Address     = "SpaceX HQ, Boca Chica, Texas (chi nhánh TP.HCM)",
                Gender      = Gender.Male,
                DateOfBirth = new DateTime(1971, 6, 28, 0, 0, 0, DateTimeKind.Utc),
                Type        = CustomerType.VIP,
                Membership  = MembershipLevel.Diamond,
                Debt        = 999_999_999,
                Notes       = "Mang Tesla Roadster đang bay quanh Mặt Trời về gara bảo dưỡng. Hỏi thợ có thể bay lên sửa không.",
            },
            new()
            {
                Name        = "Marty McFly",
                PhoneNumber = "0888888888",
                Email       = "marty@delorean-garage.vn",
                Address     = "Hill Valley, California (Năm 1985)",
                Gender      = Gender.Male,
                DateOfBirth = new DateTime(1968, 6, 9, 0, 0, 0, DateTimeKind.Utc),
                Type        = CustomerType.Individual,
                Membership  = MembershipLevel.Diamond,
                Debt        = 0,
                Notes       = "Kim tốc độ bị kẹt ở 88mph. Nghi hỏng flux capacitor. Phải sửa trước ngày 26/10.",
            },
        };

        context.Customers.AddRange(customers);
        await context.SaveChangesAsync();
    }

    // =========================================================================
    // EMPLOYEE — 10 nhân viên với các vị trí khác nhau
    // =========================================================================

    private static async System.Threading.Tasks.Task SeedEmployeesAsync(AutoXDbContext context)
    {
        if (await context.Employees.AnyAsync())
        {
            return;
        }

        var employees = new List<Employee>
    {
        // --- Kỹ thuật viên cơ khí cơ bản ---
        new()
        {
            Name = "Trần Minh Hùng",
            Gender = Gender.Male,
            DateOfBirth = new DateTime(1990, 3, 15, 0, 0, 0, DateTimeKind.Utc),
            Address = "234 Nguyễn Huệ, Q.1, TP.HCM",
            PhoneNumber = "0901234567",
            Email = "hung.tran@autox-gara.vn",
            Position = Position.Technician,
            Status = EmploymentStatus.Active,
            StartDate = new DateTime(2020, 1, 15, 0, 0, 0, DateTimeKind.Utc),
            EndDate = null,
        },

        // --- Thợ máy gầm ---
        new()
        {
            Name = "Lê Văn Phát",
            Gender = Gender.Male,
            DateOfBirth = new DateTime(1992, 7, 22, 0, 0, 0, DateTimeKind.Utc),
            Address = "567 Tô Ký, Q.12, TP.HCM",
            PhoneNumber = "0912345678",
            Email = "phat.le@autox-gara.vn",
            Position = Position.UnderCarMechanic,
            Status = EmploymentStatus.Active,
            StartDate = new DateTime(2019, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = null,
        },

        // --- Chuyên viên chẩn đoán ---
        new()
        {
            Name = "Nguyễn Quốc Vinh",
            Gender = Gender.Male,
            DateOfBirth = new DateTime(1995, 11, 8, 0, 0, 0, DateTimeKind.Utc),
            Address = "890 Cách Mạng Tháng 8, Q.3, TP.HCM",
            PhoneNumber = "0933456789",
            Email = "vinh.nguyen@autox-gara.vn",
            Position = Position.DiagnosticSpecialist,
            Status = EmploymentStatus.Active,
            StartDate = new DateTime(2021, 3, 20, 0, 0, 0, DateTimeKind.Utc),
            EndDate = null,
        },

        // --- Thợ điện ô tô ---
        new()
        {
            Name = "Hoàng Văn Đoàn",
            Gender = Gender.Male,
            DateOfBirth = new DateTime(1988, 5, 10, 0, 0, 0, DateTimeKind.Utc),
            Address = "123 Lý Thường Kiệt, Q.10, TP.HCM",
            PhoneNumber = "0944567890",
            Email = "doan.hoang@autox-gara.vn",
            Position = Position.AutoElectrician,
            Status = EmploymentStatus.Active,
            StartDate = new DateTime(2018, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = null,
        },

        // --- Nhân viên tư vấn dịch vụ ---
        new()
        {
            Name = "Trương Thị Ngà",
            Gender = Gender.Female,
            DateOfBirth = new DateTime(1991, 2, 28, 0, 0, 0, DateTimeKind.Utc),
            Address = "456 Nguyễn Trãi, Q.5, TP.HCM",
            PhoneNumber = "0955678901",
            Email = "nga.truong@autox-gara.vn",
            Position = Position.Advisor,
            Status = EmploymentStatus.Active,
            StartDate = new DateTime(2020, 7, 15, 0, 0, 0, DateTimeKind.Utc),
            EndDate = null,
        },

        // --- Quản lý ca / Trưởng ca ---
        new()
        {
            Name = "Phan Minh Nhật",
            Gender = Gender.Male,
            DateOfBirth = new DateTime(1985, 9, 3, 0, 0, 0, DateTimeKind.Utc),
            Address = "789 Điện Biên Phủ, Q.Bình Thạnh, TP.HCM",
            PhoneNumber = "0966789012",
            Email = "nhat.phan@autox-gara.vn",
            Position = Position.ShiftSupervisor,
            Status = EmploymentStatus.Active,
            StartDate = new DateTime(2017, 1, 10, 0, 0, 0, DateTimeKind.Utc),
            EndDate = null,
        },

        // --- Chuyên viên sơn xe ---
        new()
        {
            Name = "Đỗ Tiến Dũng",
            Gender = Gender.Male,
            DateOfBirth = new DateTime(1996, 4, 20, 0, 0, 0, DateTimeKind.Utc),
            Address = "321 Võ Văn Tần, Q.3, TP.HCM",
            PhoneNumber = "0977890123",
            Email = "dung.do@autox-gara.vn",
            Position = Position.Painter,
            Status = EmploymentStatus.Active,
            StartDate = new DateTime(2022, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = null,
        },

        // --- Chuyên viên sửa chữa động cơ ---
        new()
        {
            Name = "Võ Thanh Sơn",
            Gender = Gender.Male,
            DateOfBirth = new DateTime(1994, 8, 15, 0, 0, 0, DateTimeKind.Utc),
            Address = "654 Phan Xích Long, Q.Phú Nhuận, TP.HCM",
            PhoneNumber = "0988901234",
            Email = "son.vo@autox-gara.vn",
            Position = Position.EngineSpecialist,
            Status = EmploymentStatus.Inactive,
            StartDate = new DateTime(2021, 5, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2024, 12, 31, 0, 0, 0, DateTimeKind.Utc),
        },

        // --- Nhân viên tư vấn phụ tùng ---
        new()
        {
            Name = "Bùi Thị Hương",
            Gender = Gender.Female,
            DateOfBirth = new DateTime(1993, 6, 12, 0, 0, 0, DateTimeKind.Utc),
            Address = "147 Tân Kỳ Tân Quý, Q.6, TP.HCM",
            PhoneNumber = "0999012345",
            Email = "huong.bui@autox-gara.vn",
            Position = Position.PartsConsultant,
            Status = EmploymentStatus.Active,
            StartDate = new DateTime(2021, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = null,
        },

        // --- Nhân viên tiếp nhận xe (Chờ bắt đầu) ---
        new()
        {
            Name = "Vũ Minh Khoa",
            Gender = Gender.Male,
            DateOfBirth = new DateTime(1998, 10, 25, 0, 0, 0, DateTimeKind.Utc),
            Address = "963 Ngô Văn Năm, Q.Gò Vấp, TP.HCM",
            PhoneNumber = "0910111213",
            Email = "khoa.vu@autox-gara.vn",
            Position = Position.Receptionist,
            Status = EmploymentStatus.Pending,
            StartDate = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = null,
        },

        // --- Nhân viên rửa xe ---
        new()
        {
            Name = "Trần Công Sơn",
            Gender = Gender.Male,
            DateOfBirth = new DateTime(2000, 1, 30, 0, 0, 0, DateTimeKind.Utc),
            Address = "258 Nguyễn Oanh, Q.Gò Vấp, TP.HCM",
            PhoneNumber = "0911121314",
            Email = "son.tran.cs@autox-gara.vn",
            Position = Position.CarWasher,
            Status = EmploymentStatus.Active,
            StartDate = new DateTime(2023, 8, 15, 0, 0, 0, DateTimeKind.Utc),
            EndDate = null,
        },
    };

        context.Employees.AddRange(employees);
        await context.SaveChangesAsync();
    }

    // =========================================================================
    // VEHICLE — nhiều xe
    // =========================================================================

    private static async System.Threading.Tasks.Task SeedVehiclesAsync(AutoXDbContext context)
    {
        if (await context.Vehicles.AnyAsync())
        {
            return;
        }

        var customers = await context.Customers.ToListAsync();
        if (customers.Count == 0)
        {
            return;
        }

        Int32 IdOf(String name) => customers.Find(c => c.Name == name)?.Id
            ?? throw new InvalidOperationException($"Customer '{name}' không tồn tại.");

        var vehicles = new List<Vehicle>
        {
            // Nguyễn Văn An
            new()
            {
                CustomerId          = IdOf("Nguyễn Văn An"),
                LicensePlate        = "51A-12345",
                Brand               = CarBrand.Toyota,
                Model               = "Camry 2.5Q",
                Type                = CarType.Sedan,
                Color               = CarColor.White,
                Year                = 2020,
                FrameNumber         = "JTDBF3EK5A3012345",
                EngineNumber        = "2AZ1234567",
                Mileage             = 45_000,
                RegistrationDate    = new DateTime(2020, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                InsuranceExpiryDate = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            },
            // Trần Thị Bình
            new()
            {
                CustomerId          = IdOf("Trần Thị Bình"),
                LicensePlate        = "51B-67890",
                Brand               = CarBrand.Honda,
                Model               = "CR-V 1.5L Turbo",
                Type                = CarType.SUV,
                Color               = CarColor.Black,
                Year                = 2022,
                FrameNumber         = "JHMRW2H50NA067890",
                EngineNumber        = "L15B7067890",
                Mileage             = 18_500,
                RegistrationDate    = new DateTime(2022, 3, 15, 0, 0, 0, DateTimeKind.Utc),
                InsuranceExpiryDate = new DateTime(2027, 3, 15, 0, 0, 0, DateTimeKind.Utc),
            },
            // Lê Hoàng Phúc — 2 xe BMW
            new()
            {
                CustomerId          = IdOf("Lê Hoàng Phúc"),
                LicensePlate        = "51C-11223",
                Brand               = CarBrand.BMW,
                Model               = "3 Series 320i",
                Type                = CarType.Sedan,
                Color               = CarColor.Blue,
                Year                = 2021,
                FrameNumber         = "WBA8E9C51MFJ11223",
                EngineNumber        = "B48A20B11223",
                Mileage             = 32_000,
                RegistrationDate    = new DateTime(2021, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                InsuranceExpiryDate = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            },
            new()
            {
                CustomerId          = IdOf("Lê Hoàng Phúc"),
                LicensePlate        = "51C-44556",
                Brand               = CarBrand.BMW,
                Model               = "X5 xDrive40i",
                Type                = CarType.SUV,
                Color               = CarColor.Black,
                Year                = 2023,
                FrameNumber         = "5UXCR6C03P9R44556",
                EngineNumber        = "B58B30A44556",
                Mileage             = 8_000,
                RegistrationDate    = new DateTime(2023, 4, 20, 0, 0, 0, DateTimeKind.Utc),
                InsuranceExpiryDate = new DateTime(2028, 4, 20, 0, 0, 0, DateTimeKind.Utc),
            },
            // Phạm Thị Kim Chi
            new()
            {
                CustomerId          = IdOf("Phạm Thị Kim Chi"),
                LicensePlate        = "51D-77889",
                Brand               = CarBrand.KIA,
                Model               = "Morning 1.25 AT",
                Type                = CarType.Hatchback,
                Color               = CarColor.Pink,
                Year                = 2019,
                FrameNumber         = "KNABA2A20K6077889",
                EngineNumber        = "G4LA77889",
                Mileage             = 52_000,
                RegistrationDate    = new DateTime(2019, 7, 10, 0, 0, 0, DateTimeKind.Utc),
                InsuranceExpiryDate = new DateTime(2025, 7, 10, 0, 0, 0, DateTimeKind.Utc),
            },
            // Đặng Quốc Hùng — đội 3 xe
            new()
            {
                CustomerId          = IdOf("Đặng Quốc Hùng"),
                LicensePlate        = "51E-00001",
                Brand               = CarBrand.Ford,
                Model               = "Transit 16 chỗ",
                Type                = CarType.Minivan,
                Color               = CarColor.Silver,
                Year                = 2020,
                FrameNumber         = "WFOZXXTTGZJA00001",
                EngineNumber        = "DURATORQ00001",
                Mileage             = 98_000,
                RegistrationDate    = new DateTime(2020, 1, 5, 0, 0, 0, DateTimeKind.Utc),
                InsuranceExpiryDate = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc),
            },
            new()
            {
                CustomerId          = IdOf("Đặng Quốc Hùng"),
                LicensePlate        = "51E-00002",
                Brand               = CarBrand.Toyota,
                Model               = "Innova 2.0G",
                Type                = CarType.Minivan,
                Color               = CarColor.Gray,
                Year                = 2021,
                FrameNumber         = "MHF0FR3340P000002",
                EngineNumber        = "1TR0000002",
                Mileage             = 74_000,
                RegistrationDate    = new DateTime(2021, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                InsuranceExpiryDate = new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc),
            },
            new()
            {
                CustomerId          = IdOf("Đặng Quốc Hùng"),
                LicensePlate        = "51E-00003",
                Brand               = CarBrand.Hyundai,
                Model               = "Solati 16 chỗ",
                Type                = CarType.Minivan,
                Color               = CarColor.White,
                Year                = 2022,
                FrameNumber         = "KMHBU81TENU000003",
                EngineNumber        = "D4CB000003",
                Mileage             = 55_000,
                RegistrationDate    = new DateTime(2022, 11, 1, 0, 0, 0, DateTimeKind.Utc),
                InsuranceExpiryDate = new DateTime(2027, 11, 1, 0, 0, 0, DateTimeKind.Utc),
            },
            // Công ty Vận Tải Phú Thịnh
            new()
            {
                CustomerId          = IdOf("Công ty TNHH Vận Tải Phú Thịnh"),
                LicensePlate        = "51F-11111",
                Brand               = CarBrand.Ford,
                Model               = "Transit Cargo",
                Type                = CarType.Truck,
                Color               = CarColor.Silver,
                Year                = 2019,
                FrameNumber         = "WFOZXXTTGZJA11111",
                EngineNumber        = "DURATORQ11111",
                Mileage             = 120_000,
                RegistrationDate    = new DateTime(2019, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                InsuranceExpiryDate = new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc),
            },
            // Grab Việt Nam
            new()
            {
                CustomerId          = IdOf("Công ty CP Grab Việt Nam"),
                LicensePlate        = "51G-20240",
                Brand               = CarBrand.Toyota,
                Model               = "Vios 1.5G CVT",
                Type                = CarType.Sedan,
                Color               = CarColor.White,
                Year                = 2023,
                FrameNumber         = "MHF0AH3300P020240",
                EngineNumber        = "2NZ020240",
                Mileage             = 40_000,
                RegistrationDate    = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                InsuranceExpiryDate = new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            },
            new()
            {
                CustomerId          = IdOf("Nguyễn Văn Xe Tăng"),
                LicensePlate        = "QD-54321",
                Brand               = CarBrand.Other,
                Model               = "T-54 MBT (Xe Tăng Chiến Đấu Chủ Lực)",
                Type                = CarType.Other,
                Color               = CarColor.Green,
                Year                = 1954,
                FrameNumber         = "T54QUANTM0000001",
                EngineNumber        = "V54500HP000001",
                Mileage             = 999_999,
                RegistrationDate    = new DateTime(1975, 4, 30, 0, 0, 0, DateTimeKind.Utc),
                InsuranceExpiryDate = null, // Ai dám đâm vào xe tăng mà cần bảo hiểm
            },
            new()
            {
                CustomerId          = IdOf("Tỉ Phú ElonUsk"),
                LicensePlate        = "SX-00001",
                Brand               = CarBrand.Tesla,
                Model               = "Roadster Starman Edition — đang bay quanh Mặt Trời",
                Type                = CarType.Coupe,
                Color               = CarColor.Red,
                Year                = 2018,
                FrameNumber         = "5YJ3E1EAXJF000001",
                EngineNumber        = "ELECTRICMOTOR001",
                Mileage             = 999_999, // đã đi được hơn 1 tỷ km, capped tại max
                RegistrationDate    = new DateTime(2018, 2, 6, 0, 0, 0, DateTimeKind.Utc),
                InsuranceExpiryDate = new DateTime(9999, 12, 31, 0, 0, 0, DateTimeKind.Utc),
            },
            new()
            {
                CustomerId          = IdOf("Marty McFly"),
                LicensePlate        = "OUTATIME",  // 8 ký tự, hợp lệ với MaxLength(9)
                Brand               = CarBrand.Other,
                Model               = "DeLorean DMC-12 Time Machine",
                Type                = CarType.Coupe,
                Color               = CarColor.Silver,
                Year                = 1985,
                FrameNumber         = "KNEELBFORE0ZOD01",  // 17 ký tự
                EngineNumber        = "FLUXCAPACITOR088",  // 17 ký tự
                Mileage             = 88,
                RegistrationDate    = new DateTime(1985, 10, 26, 0, 0, 0, DateTimeKind.Utc),
                InsuranceExpiryDate = new DateTime(1885, 9, 5, 0, 0, 0, DateTimeKind.Utc), // hết hạn từ... quá khứ
            },
        };

        context.Vehicles.AddRange(vehicles);
        await context.SaveChangesAsync();
    }

    // =========================================================================
    // SUPPLIER — 4 nhà cung cấp
    // =========================================================================

    private static async System.Threading.Tasks.Task SeedSuppliersAsync(AutoXDbContext context)
    {
        if (await context.Suppliers.AnyAsync())
        {
            return;
        }

        var suppliers = new List<Supplier>
        {
            new()
            {
                Name              = "Công ty CP Phụ Tùng Toyota Việt Nam",
                Email             = "contact@toyotaparts.vn",
                Address           = "Khu CN Mỹ Phước, Bình Dương",
                TaxCode           = "0300123456789",
                BankAccount       = "19033123456789",
                PaymentTerms      = PaymentTerms.Net30,
                Status            = SupplierStatus.Active,
                ContractStartDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Notes             = "Nhà cung cấp chính thức phụ tùng Toyota, ưu tiên đặt hàng.",
                PhoneNumbers      =
                [
                    new SupplierContactPhone { PhoneNumber = "02713456789" },
                    new SupplierContactPhone { PhoneNumber = "0901111222" },
                ],
            },
            new()
            {
                Name              = "Công ty TNHH Phụ Tùng Honda Việt Nam",
                Email             = "sales@hondaparts.vn",
                Address           = "KCX Tân Thuận, Q.7, TP.HCM",
                TaxCode           = "0300987654321",
                BankAccount       = "19039876543210",
                PaymentTerms      = PaymentTerms.Net15,
                Status            = SupplierStatus.Active,
                ContractStartDate = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                Notes             = "Chuyên cung cấp phụ tùng Honda chính hãng.",
                PhoneNumbers      =
                [
                    new SupplierContactPhone { PhoneNumber = "02838888999" },
                    new SupplierContactPhone { PhoneNumber = "0912222333" },
                ],
            },
            new()
            {
                Name              = "Công ty CP Bosch Việt Nam",
                Email             = "info@bosch.vn",
                Address           = "Tòa nhà Bitexco Financial Tower, Q.1, TP.HCM",
                TaxCode           = "0300111223344",
                BankAccount       = "00101234567890",
                PaymentTerms      = PaymentTerms.Net30,
                Status            = SupplierStatus.Active,
                ContractStartDate = new DateTime(2022, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                Notes             = "Cung cấp bugi, cảm biến, linh kiện điện tử đa hãng.",
                PhoneNumbers      =
                [
                    new SupplierContactPhone { PhoneNumber = "02839999000" },
                ],
            },
            new()
            {
                Name              = "Tập Đoàn Stark Industries VN",
                Email             = "tony@starkindustries-vn.com",
                Address           = "99 Nguyễn Huệ, Q.1, TP.HCM (VP đại diện)",
                TaxCode           = "9999999999999",
                BankAccount       = "99999999999999",
                PaymentTerms      = PaymentTerms.DueOnReceipt,
                Status            = SupplierStatus.Active,
                ContractStartDate = new DateTime(2008, 5, 2, 0, 0, 0, DateTimeKind.Utc),
                Notes             = "Chuyên cung cấp: động cơ tên lửa thu nhỏ, radar phát hiện địch, " +
                                    "arc reactor, bộ giáp Iron Man Mk.85. " +
                                    "LƯU Ý: KHÔNG bán Infinity Gauntlet — đã kiểm tra, hàng bị hỏng sau snap.",
                PhoneNumbers      =
                [
                    new SupplierContactPhone { PhoneNumber = "08008008008" },
                ],
            },
        };

        context.Suppliers.AddRange(suppliers);
        await context.SaveChangesAsync();
    }

    // =========================================================================
    // SPARE PART — 17 phụ tùng
    // =========================================================================

    /// <summary>
    /// Seeds the database with part data.
    /// Includes both spare parts (for sale) and replacement parts (for inventory).
    /// </summary>
    public static async System.Threading.Tasks.Task SeedPartsAsync(AutoXDbContext context)
    {
        if (await context.Parts.AnyAsync())
        {
            return;
        }

        var suppliers = await context.Suppliers.ToListAsync();
        if (suppliers.Count == 0)
        {
            return;
        }

        /// <summary>
        /// Helper method to find supplier ID by keyword.
        /// </summary>
        Int32 IdOf(String keyword) => suppliers
            .FirstOrDefault(s => s.Name.Contains(keyword))?.Id
            ?? throw new InvalidOperationException($"Supplier có từ khóa '{keyword}' không tồn tại.");

        Int32 toyotaId = IdOf("Toyota");
        Int32 hondaId = IdOf("Honda");
        Int32 boschId = IdOf("Bosch");
        Int32 starkId = IdOf("Stark");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var parts = new List<Part>();

        #region Spare Parts - Toyota (Phụ tùng bán)

        parts.AddRange(
        [
            // --- Toyota Spare Parts ---
            new()
            {
                SupplierId = toyotaId,
                PartCode = "SP_TOYOTA_001",
                PartName = "Giảm xóc trước Toyota Fortuner (KYB OEM)",
                Manufacturer = "KYB",
                PartCategory = PartCategory.Suspension,
                PurchasePrice = 1_400_000,
                SellingPrice = 2_000_000,
                InventoryQuantity = 10,
                DateAdded = today,
                ExpiryDate = null,
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = toyotaId,
                PartCode = "SP_TOYOTA_002",
                PartName = "Thanh cân bằng Toyota Camry 2.5",
                Manufacturer = "Toyota",
                PartCategory = PartCategory.Steering,
                PurchasePrice = 620_000,
                SellingPrice = 920_000,
                InventoryQuantity = 8,
                DateAdded = today,
                ExpiryDate = null,
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = toyotaId,
                PartCode = "SP_TOYOTA_003",
                PartName = "Két điều hòa Toyota Innova 2.0",
                Manufacturer = "Toyota",
                PartCategory = PartCategory.AirConditioning,
                PurchasePrice = 1_850_000,
                SellingPrice = 2_700_000,
                InventoryQuantity = 5,
                DateAdded = today,
                ExpiryDate = null,
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = toyotaId,
                PartCode = "SP_TOYOTA_004",
                PartName = "Lọc dầu Toyota Camry",
                Manufacturer = "Toyota",
                PartCategory = PartCategory.Filter,
                PurchasePrice = 85_000,
                SellingPrice = 120_000,
                InventoryQuantity = 50,
                DateAdded = today,
                ExpiryDate = null,
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = toyotaId,
                PartCode = "SP_TOYOTA_005",
                PartName = "Má phanh trước Toyota Camry",
                Manufacturer = "Toyota",
                PartCategory = PartCategory.Brake,
                PurchasePrice = 450_000,
                SellingPrice = 650_000,
                InventoryQuantity = 20,
                DateAdded = today,
                ExpiryDate = null,
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = toyotaId,
                PartCode = "SP_TOYOTA_006",
                PartName = "Bình ắc quy Toyota Vios 45Ah",
                Manufacturer = "Toyota",
                PartCategory = PartCategory.Electrical,
                PurchasePrice = 1_100_000,
                SellingPrice = 1_550_000,
                InventoryQuantity = 15,
                DateAdded = today,
                ExpiryDate = null,
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = toyotaId,
                PartCode = "SP_TOYOTA_007",
                PartName = "Dây curoa cam Toyota Innova 2.0",
                Manufacturer = "Toyota",
                PartCategory = PartCategory.Engine,
                PurchasePrice = 320_000,
                SellingPrice = 480_000,
                InventoryQuantity = 30,
                DateAdded = today,
                ExpiryDate = null,
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = toyotaId,
                PartCode = "SP_TOYOTA_008",
                PartName = "Bơm nước Toyota Fortuner 2.7",
                Manufacturer = "Toyota",
                PartCategory = PartCategory.Cooling,
                PurchasePrice = 850_000,
                SellingPrice = 1_250_000,
                InventoryQuantity = 8,
                DateAdded = today,
                ExpiryDate = null,
                IsDefective = false,
                IsDiscontinued = false,
            },
        ]);

        #endregion

        #region Spare Parts - Honda (Phụ tùng bán)

        parts.AddRange(
        [
            // --- Honda Spare Parts ---
            new()
            {
                SupplierId = hondaId,
                PartCode = "SP_HONDA_001",
                PartName = "Đèn pha LED Honda CR-V 2023 (trái)",
                Manufacturer = "Honda",
                PartCategory = PartCategory.Lighting,
                PurchasePrice = 3_500_000,
                SellingPrice = 5_200_000,
                InventoryQuantity = 4,
                DateAdded = today,
                ExpiryDate = null,
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = hondaId,
                PartCode = "SP_HONDA_002",
                PartName = "Kính chắn gió Honda City (chống UV)",
                Manufacturer = "Honda",
                PartCategory = PartCategory.UVGlass,
                PurchasePrice = 2_200_000,
                SellingPrice = 3_100_000,
                InventoryQuantity = 3,
                DateAdded = today,
                ExpiryDate = null,
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = hondaId,
                PartCode = "SP_HONDA_003",
                PartName = "Lốp xe Honda HR-V 215/60R16",
                Manufacturer = "Michelin",
                PartCategory = PartCategory.WheelAndTire,
                PurchasePrice = 1_600_000,
                SellingPrice = 2_200_000,
                InventoryQuantity = 16,
                DateAdded = today,
                ExpiryDate = null,
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = hondaId,
                PartCode = "SP_HONDA_004",
                PartName = "Bugi Honda CR-V 1.5T (NGK)",
                Manufacturer = "NGK",
                PartCategory = PartCategory.Ignition,
                PurchasePrice = 65_000,
                SellingPrice = 95_000,
                InventoryQuantity = 100,
                DateAdded = today,
                ExpiryDate = null,
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = hondaId,
                PartCode = "SP_HONDA_005",
                PartName = "Dầu động cơ Honda Ultra 5W-30 (4L)",
                Manufacturer = "Honda",
                PartCategory = PartCategory.Lubrication,
                PurchasePrice = 280_000,
                SellingPrice = 390_000,
                InventoryQuantity = 60,
                DateAdded = today,
                ExpiryDate = today.AddYears(2),
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = hondaId,
                PartCode = "SP_HONDA_006",
                PartName = "Lọc gió Honda City",
                Manufacturer = "Honda",
                PartCategory = PartCategory.Filter,
                PurchasePrice = 70_000,
                SellingPrice = 110_000,
                InventoryQuantity = 45,
                DateAdded = today,
                ExpiryDate = null,
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = hondaId,
                PartCode = "SP_HONDA_007",
                PartName = "Cao su gạt mưa Honda HR-V (cặp)",
                Manufacturer = "Honda",
                PartCategory = PartCategory.Maintenance,
                PurchasePrice = 120_000,
                SellingPrice = 180_000,
                InventoryQuantity = 35,
                DateAdded = today,
                ExpiryDate = null,
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = hondaId,
                PartCode = "SP_HONDA_008",
                PartName = "Bộ piston và xéc măng Honda Civic 1.8",
                Manufacturer = "Honda",
                PartCategory = PartCategory.Engine,
                PurchasePrice = 3_200_000,
                SellingPrice = 4_500_000,
                InventoryQuantity = 5,
                DateAdded = today,
                ExpiryDate = null,
                IsDefective = false,
                IsDiscontinued = false,
            },
        ]);

        #endregion

        #region Spare Parts - Bosch (Phụ tùng bán)

        parts.AddRange(
        [
            // --- Bosch Spare Parts ---
            new()
            {
                SupplierId = boschId,
                PartCode = "SP_BOSCH_001",
                PartName = "Cảm biến ABS Bosch bánh trước (universal)",
                Manufacturer = "Bosch",
                PartCategory = PartCategory.ABS,
                PurchasePrice = 480_000,
                SellingPrice = 720_000,
                InventoryQuantity = 12,
                DateAdded = today,
                ExpiryDate = null,
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = boschId,
                PartCode = "SP_BOSCH_002",
                PartName = "Máy phát điện Bosch 14V 90A",
                Manufacturer = "Bosch",
                PartCategory = PartCategory.Electrical,
                PurchasePrice = 2_800_000,
                SellingPrice = 3_900_000,
                InventoryQuantity = 6,
                DateAdded = today,
                ExpiryDate = null,
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = boschId,
                PartCode = "SP_BOSCH_003",
                PartName = "Kim phun nhiên liệu Bosch (universal)",
                Manufacturer = "Bosch",
                PartCategory = PartCategory.FuelInjection,
                PurchasePrice = 750_000,
                SellingPrice = 1_100_000,
                InventoryQuantity = 20,
                DateAdded = today,
                ExpiryDate = null,
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = boschId,
                PartCode = "SP_BOSCH_004",
                PartName = "Cảm biến oxy Bosch O2 Sensor",
                Manufacturer = "Bosch",
                PartCategory = PartCategory.SensorsAndModules,
                PurchasePrice = 550_000,
                SellingPrice = 820_000,
                InventoryQuantity = 18,
                DateAdded = today,
                ExpiryDate = null,
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = boschId,
                PartCode = "SP_BOSCH_005",
                PartName = "Bugi Bosch Iridium đa hãng",
                Manufacturer = "Bosch",
                PartCategory = PartCategory.Ignition,
                PurchasePrice = 95_000,
                SellingPrice = 145_000,
                InventoryQuantity = 80,
                DateAdded = today,
                ExpiryDate = null,
                IsDefective = false,
                IsDiscontinued = false,
            },
        ]);

        #endregion

        #region Spare Parts - Stark Industries (Phụ tùng bán - Fantasy)

        parts.AddRange(
        [
            // --- Stark Industries Fantasy Spare Parts ---
            new()
            {
                SupplierId = starkId,
                PartCode = "SP_STARK_001",
                PartName = "HUD Holographic Stark — Hiển Thị 3D Toàn Cảnh 360°",
                Manufacturer = "Stark Industries",
                PartCategory = PartCategory.HUD,
                PurchasePrice = 75_000_000,
                SellingPrice = 120_000_000,
                InventoryQuantity = 2,
                DateAdded = today,
                ExpiryDate = null,
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = starkId,
                PartCode = "SP_STARK_002",
                PartName = "Cánh Gió Điều Khiển Tự Động Stark Aerodynamics Kit",
                Manufacturer = "Stark Industries",
                PartCategory = PartCategory.Aerodynamics,
                PurchasePrice = 200_000_000,
                SellingPrice = 350_000_000,
                InventoryQuantity = 1,
                DateAdded = today,
                ExpiryDate = null,
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = starkId,
                PartCode = "SP_STARK_003",
                PartName = "Động Cơ Tên Lửa Thu Nhỏ Stark Mk.1",
                Manufacturer = "Stark Industries",
                PartCategory = PartCategory.Engine,
                PurchasePrice = 500_000_000,
                SellingPrice = 999_000_000,
                InventoryQuantity = 2,
                DateAdded = today,
                ExpiryDate = null,
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = starkId,
                PartCode = "SP_STARK_004",
                PartName = "Radar Phát Hiện Địch Stark (Car Edition)",
                Manufacturer = "Stark Industries",
                PartCategory = PartCategory.SensorsAndModules,
                PurchasePrice = 150_000_000,
                SellingPrice = 250_000_000,
                InventoryQuantity = 3,
                DateAdded = today,
                ExpiryDate = null,
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = starkId,
                PartCode = "SP_STARK_005",
                PartName = "Arc Reactor Mini (Thay Bình Ắc Quy Thông Thường)",
                Manufacturer = "Stark Industries",
                PartCategory = PartCategory.BatteryAndModules,
                PurchasePrice = 1_000_000_000,
                SellingPrice = 2_000_000_000,
                InventoryQuantity = 1,
                DateAdded = today,
                ExpiryDate = null,
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = starkId,
                PartCode = "SP_STARK_006",
                PartName = "Flux Capacitor (Hàng Chính Hãng 1885 — Dành Cho DeLorean)",
                Manufacturer = "Stark Industries",
                PartCategory = PartCategory.Electrical,
                PurchasePrice = 88_000_000,
                SellingPrice = 88_888_888,
                InventoryQuantity = 1,
                DateAdded = today,
                ExpiryDate = null,
                IsDefective = false,
                IsDiscontinued = false,
            },
        ]);

        #endregion

        #region Replacement Parts - Generic (Phụ tùng thay thế)

        parts.AddRange(
        [
            // --- Generic Replacement Parts (Thực tế) ---
            new()
            {
                SupplierId = boschId,
                PartCode = "RP001",
                PartName = "Lọc gió động cơ đa năng",
                Manufacturer = "Denso",
                PartCategory = PartCategory.Filter,
                PurchasePrice = 150_000,
                SellingPrice = 225_000,
                InventoryQuantity = 40,
                DateAdded = today,
                ExpiryDate = today.AddYears(3),
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = boschId,
                PartCode = "RP002",
                PartName = "Dây curoa dẫn động",
                Manufacturer = "Gates",
                PartCategory = PartCategory.Engine,
                PurchasePrice = 320_000,
                SellingPrice = 480_000,
                InventoryQuantity = 25,
                DateAdded = today,
                ExpiryDate = today.AddYears(5),
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = boschId,
                PartCode = "RP003",
                PartName = "Má phanh sau đa hãng",
                Manufacturer = "Brembo",
                PartCategory = PartCategory.Brake,
                PurchasePrice = 580_000,
                SellingPrice = 870_000,
                InventoryQuantity = 16,
                DateAdded = today,
                ExpiryDate = null,
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = boschId,
                PartCode = "RP004",
                PartName = "Bình chứa dầu phanh DOT4 500ml",
                Manufacturer = "Castrol",
                PartCategory = PartCategory.Brake,
                PurchasePrice = 95_000,
                SellingPrice = 142_500,
                InventoryQuantity = 30,
                DateAdded = today,
                ExpiryDate = today.AddYears(2),
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = toyotaId,
                PartCode = "RP005",
                PartName = "Nước làm mát Ready-to-Use 1L",
                Manufacturer = "Prestone",
                PartCategory = PartCategory.Cooling,
                PurchasePrice = 75_000,
                SellingPrice = 112_500,
                InventoryQuantity = 50,
                DateAdded = today,
                ExpiryDate = today.AddYears(4),
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = boschId,
                PartCode = "RP006",
                PartName = "Bộ gioăng nắp máy đa dụng",
                Manufacturer = "Fel-Pro",
                PartCategory = PartCategory.Engine,
                PurchasePrice = 420_000,
                SellingPrice = 630_000,
                InventoryQuantity = 10,
                DateAdded = today,
                ExpiryDate = null,
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = boschId,
                PartCode = "RP007",
                PartName = "Cao su chắn bùn bộ 4 bánh",
                Manufacturer = "WeatherTech",
                PartCategory = PartCategory.Maintenance,
                PurchasePrice = 380_000,
                SellingPrice = 570_000,
                InventoryQuantity = 20,
                DateAdded = today,
                ExpiryDate = null,
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = boschId,
                PartCode = "RP008",
                PartName = "Bugi NGK Iridium IX hộp 4 cái",
                Manufacturer = "NGK",
                PartCategory = PartCategory.Ignition,
                PurchasePrice = 380_000,
                SellingPrice = 570_000,
                InventoryQuantity = 60,
                DateAdded = today,
                ExpiryDate = today.AddYears(5),
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = boschId,
                PartCode = "RP009",
                PartName = "Lọc nhiên liệu đa hãng",
                Manufacturer = "Bosch",
                PartCategory = PartCategory.Filter,
                PurchasePrice = 175_000,
                SellingPrice = 262_500,
                InventoryQuantity = 35,
                DateAdded = today,
                ExpiryDate = today.AddYears(3),
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = toyotaId,
                PartCode = "RP013",
                PartName = "Dầu động cơ tổng hợp 5W-30 4L",
                Manufacturer = "Mobil 1",
                PartCategory = PartCategory.Lubrication,
                PurchasePrice = 420_000,
                SellingPrice = 630_000,
                InventoryQuantity = 45,
                DateAdded = today,
                ExpiryDate = today.AddYears(5),
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = boschId,
                PartCode = "RP014",
                PartName = "Ắc quy khô 12V 45Ah",
                Manufacturer = "Bosch",
                PartCategory = PartCategory.Electrical,
                PurchasePrice = 1_350_000,
                SellingPrice = 2_025_000,
                InventoryQuantity = 12,
                DateAdded = today,
                ExpiryDate = today.AddYears(3),
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = boschId,
                PartCode = "RP015",
                PartName = "Lọc nhớt đa hãng",
                Manufacturer = "Mann-Filter",
                PartCategory = PartCategory.Filter,
                PurchasePrice = 85_000,
                SellingPrice = 127_500,
                InventoryQuantity = 80,
                DateAdded = today,
                ExpiryDate = today.AddYears(4),
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = boschId,
                PartCode = "RP016",
                PartName = "Gạt mưa mềm 24 inch",
                Manufacturer = "Bosch",
                PartCategory = PartCategory.Maintenance,
                PurchasePrice = 120_000,
                SellingPrice = 180_000,
                InventoryQuantity = 55,
                DateAdded = today,
                ExpiryDate = today.AddYears(2),
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = boschId,
                PartCode = "RP017",
                PartName = "Giảm xóc trước thay thế",
                Manufacturer = "KYB",
                PartCategory = PartCategory.Suspension,
                PurchasePrice = 1_200_000,
                SellingPrice = 1_800_000,
                InventoryQuantity = 18,
                DateAdded = today,
                ExpiryDate = null,
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = boschId,
                PartCode = "RP018",
                PartName = "Bộ lọc cabin khử mùi",
                Manufacturer = "3M",
                PartCategory = PartCategory.Filter,
                PurchasePrice = 185_000,
                SellingPrice = 277_500,
                InventoryQuantity = 40,
                DateAdded = today,
                ExpiryDate = today.AddYears(2),
                IsDefective = false,
                IsDiscontinued = false,
            },
        ]);

        #endregion

        #region Replacement Parts - Fantasy (Phụ tùng thay thế - Fantasy)

        parts.AddRange(
        [
            // --- Fantasy Replacement Parts ---
            new()
            {
                SupplierId = starkId,
                PartCode = "RP010",
                PartName = "Bộ Giáp Iron Man Mk.85 (Thay Cản Trước Xe)",
                Manufacturer = "Stark Industries",
                PartCategory = PartCategory.Engine,
                PurchasePrice = 9_999_999_999,
                SellingPrice = 15_000_000_000,
                InventoryQuantity = 1,
                DateAdded = today,
                ExpiryDate = null,
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = starkId,
                PartCode = "RP011",
                PartName = "Đầu Đạn Tên Lửa RPG Thay Thế Cản Sau Xe Tăng T-54",
                Manufacturer = "Unknown",
                PartCategory = PartCategory.Engine,
                PurchasePrice = 50_000_000,
                SellingPrice = 75_000_000,
                InventoryQuantity = 5,
                DateAdded = today,
                ExpiryDate = today.AddYears(99),
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = starkId,
                PartCode = "RP012",
                PartName = "Xăng Hypersonic Pha Lê Nhiên Liệu Cho Động Cơ Tên Lửa",
                Manufacturer = "Area 51 Fuel Co.",
                PartCategory = PartCategory.Lubrication,
                PurchasePrice = 500_000_000,
                SellingPrice = 750_000_000,
                InventoryQuantity = 0,
                DateAdded = today,
                ExpiryDate = today.AddDays(30),
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = starkId,
                PartCode = "RP019",
                PartName = "Lốp Xe Graphene Siêu Dẫn Nhiệt Chống Nổ Cấp Quân Sự",
                Manufacturer = "MIT Advanced Materials Lab",
                PartCategory = PartCategory.WheelAndTire,
                PurchasePrice = 45_000_000,
                SellingPrice = 67_500_000,
                InventoryQuantity = 4,
                DateAdded = today,
                ExpiryDate = today.AddYears(50),
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = starkId,
                PartCode = "RP020",
                PartName = "Bộ Pin Hydrogen Fuel Cell 150kW Thay Thế Động Cơ Đốt Trong",
                Manufacturer = "Toyota Research Institute",
                PartCategory = PartCategory.Electrical,
                PurchasePrice = 320_000_000,
                SellingPrice = 480_000_000,
                InventoryQuantity = 2,
                DateAdded = today,
                ExpiryDate = today.AddYears(10),
                IsDefective = false,
                IsDiscontinued = false,
            },
            new()
            {
                SupplierId = starkId,
                PartCode = "RP021",
                PartName = "Cảm Biến LiDAR 128-Layer Tự Lái Cấp Độ 5",
                Manufacturer = "Waymo",
                PartCategory = PartCategory.SensorsAndModules,
                PurchasePrice = 180_000_000,
                SellingPrice = 270_000_000,
                InventoryQuantity = 1,
                DateAdded = today,
                ExpiryDate = today.AddYears(5),
                IsDefective = false,
                IsDiscontinued = false,
            },
        ]);

        #endregion

        context.Parts.AddRange(parts);
        await context.SaveChangesAsync();
    }

    // =========================================================================
    // SERVICE ITEM — 15+ dịch vụ
    // =========================================================================

    private static async System.Threading.Tasks.Task SeedServiceItemsAsync(AutoXDbContext context)
    {
        if (await context.ServiceItems.AnyAsync())
        {
            return;
        }

        var serviceItems = new List<ServiceItem>
    {
        // --- Bảo dưỡng định kỳ ---
        new()
        {
            Description = "Bảo dưỡng định kỳ 10,000km (Thay dầu, lọc dầu, kiểm tra chung)",
            Type = ServiceType.Maintenance,
            UnitPrice = 450_000,
        },
        new()
        {
            Description = "Bảo dưỡng định kỳ 20,000km (Kiểm tra toàn bộ, bảo dưỡng hệ thống)",
            Type = ServiceType.Maintenance,
            UnitPrice = 850_000,
        },
        new()
        {
            Description = "Bảo dưỡng định kỳ 40,000km (Kiểm tra sâu, thay thế linh kiện)",
            Type = ServiceType.Maintenance,
            UnitPrice = 1_200_000,
        },
        new()
        {
            Description = "Kiểm tra xe định kỳ (Kiểm tra an toàn, hiệu suất)",
            Type = ServiceType.Inspection,
            UnitPrice = 300_000,
        },

        // --- Thay dầu & bộ lọc ---
        new()
        {
            Description = "Thay dầu động cơ + lọc dầu + lọc gió",
            Type = ServiceType.OilChange,
            UnitPrice = 400_000,
        },
        new()
        {
            Description = "Thay lọc cabin + khử mùi",
            Type = ServiceType.OilChange,
            UnitPrice = 250_000,
        },
        new()
        {
            Description = "Thay lọc nhiên liệu",
            Type = ServiceType.OilChange,
            UnitPrice = 180_000,
        },

        // --- Dịch vụ lốp xe ---
        new()
        {
            Description = "Thay lốp xe (1 cái)",
            Type = ServiceType.TireService,
            UnitPrice = 200_000,
        },
        new()
        {
            Description = "Vá lốp xe",
            Type = ServiceType.TireService,
            UnitPrice = 50_000,
        },
        new()
        {
            Description = "Cân bằng lốp xe (bộ 4 cái)",
            Type = ServiceType.TireService,
            UnitPrice = 300_000,
        },
        new()
        {
            Description = "Cân chỉnh góc đặt bánh xe (Alignment)",
            Type = ServiceType.WheelAlignment,
            UnitPrice = 350_000,
        },

        // --- Dịch vụ điều hòa ---
        new()
        {
            Description = "Bảo dưỡng điều hòa (Vệ sinh, khử mùi)",
            Type = ServiceType.ACService,
            UnitPrice = 400_000,
        },
        new()
        {
            Description = "Nạp gas điều hòa (R134a)",
            Type = ServiceType.ACService,
            UnitPrice = 300_000,
        },
        new()
        {
            Description = "Thay dầu điều hòa",
            Type = ServiceType.ACService,
            UnitPrice = 250_000,
        },

        // --- Sửa chữa chung ---
        new()
        {
            Description = "Sửa chữa động cơ (theo yêu cầu)",
            Type = ServiceType.EngineRepair,
            UnitPrice = 1_500_000,
        },
        new()
        {
            Description = "Sửa chữa hộp số tự động",
            Type = ServiceType.TransmissionRepair,
            UnitPrice = 2_000_000,
        },
        new()
        {
            Description = "Sửa chữa hệ thống phanh (Kiểm tra & bảo dưỡng)",
            Type = ServiceType.BrakeRepair,
            UnitPrice = 600_000,
        },
        new()
        {
            Description = "Sửa chữa hệ thống lái & treo",
            Type = ServiceType.SuspensionRepair,
            UnitPrice = 800_000,
        },
        new()
        {
            Description = "Sửa chữa hệ thống nhiên liệu",
            Type = ServiceType.FuelSystemRepair,
            UnitPrice = 700_000,
        },
        new()
        {
            Description = "Sửa chữa hệ thống điện (Điều tra lỗi)",
            Type = ServiceType.ElectricalService,
            UnitPrice = 400_000,
        },
        new()
        {
            Description = "Thay bình ắc quy",
            Type = ServiceType.ElectricalService,
            UnitPrice = 500_000,
        },
        new()
        {
            Description = "Sửa chữa hệ thống đánh lửa",
            Type = ServiceType.IgnitionRepair,
            UnitPrice = 300_000,
        },

        // --- Làm đẹp ---
        new()
        {
            Description = "Rửa xe bàn tay",
            Type = ServiceType.CarWashAndDetailing,
            UnitPrice = 150_000,
        },
        new()
        {
            Description = "Chăm sóc nội thất (Vệ sinh, khử mùi, bảo dưỡng)",
            Type = ServiceType.CarWashAndDetailing,
            UnitPrice = 300_000,
        },
        new()
        {
            Description = "Đánh bóng & sơn phục hồi",
            Type = ServiceType.Painting,
            UnitPrice = 1_000_000,
        },
        new()
        {
            Description = "Phục hồi đèn pha (Polishing & coating)",
            Type = ServiceType.HeadlightRestoration,
            UnitPrice = 400_000,
        },
        new()
        {
            Description = "Dán phim cách nhiệt toàn kính",
            Type = ServiceType.WindowTintingAndPPF,
            UnitPrice = 2_000_000,
        },
        new()
        {
            Description = "Phủ sơn bảo vệ (Paint Protection Film)",
            Type = ServiceType.WindowTintingAndPPF,
            UnitPrice = 3_000_000,
        },
        new()
        {
            Description = "Phủ Ceramic coating 9H",
            Type = ServiceType.CeramicCoating,
            UnitPrice = 2_500_000,
        },

        // --- An toàn & kiểm định ---
        new()
        {
            Description = "Dịch vụ kiểm định xe (Đăng kiểm)",
            Type = ServiceType.VehicleInspection,
            UnitPrice = 500_000,
        },
        new()
        {
            Description = "Lắp đặt camera hành trình",
            Type = ServiceType.DashcamInstallation,
            UnitPrice = 800_000,
        },
        new()
        {
            Description = "Lắp đặt cảm biến đỗ xe (4 cảm biến)",
            Type = ServiceType.ParkingSensorAndADAS,
            UnitPrice = 600_000,
        },
        new()
        {
            Description = "Lắp đặt hệ thống hỗ trợ lái (ADAS)",
            Type = ServiceType.ParkingSensorAndADAS,
            UnitPrice = 1_500_000,
        },

        // --- Dịch vụ khẩn cấp ---
        new()
        {
            Description = "Dịch vụ cứu hộ xe khẩn cấp (Kéo xe + chẩn đoán)",
            Type = ServiceType.EmergencyRoadsideAssistance,
            UnitPrice = 2_000_000,
        },
        new()
        {
            Description = "Dịch vụ kéo xe đến gara",
            Type = ServiceType.TowingService,
            UnitPrice = 1_500_000,
        },
        new()
        {
            Description = "Hỗ trợ khởi động xe (Nhảy bình)",
            Type = ServiceType.JumpStartService,
            UnitPrice = 200_000,
        },
        new()
        {
            Description = "Hỗ trợ mở khóa xe",
            Type = ServiceType.LockoutAssistance,
            UnitPrice = 150_000,
        },
        new()
        {
            Description = "Cung cấp nhiên liệu khẩn cấp",
            Type = ServiceType.EmergencyFuelDelivery,
            UnitPrice = 100_000,
        },
    };

        context.ServiceItems.AddRange(serviceItems);
        await context.SaveChangesAsync();
    }

    // =========================================================================
    // EMPLOYEE — 8 nhân viên
    // =========================================================================

    // =========================================================================
    // REPAIR TASK — 12+ công việc sửa chữa
    // =========================================================================

    private static async System.Threading.Tasks.Task SeedRepairTasksAsync(AutoXDbContext context)
    {
        if (await context.RepairTasks.AnyAsync())
        {
            return;
        }

        var employees = await context.Employees.Where(e => e.Status == EmploymentStatus.Active).ToListAsync();
        var serviceItems = await context.ServiceItems.ToListAsync();

        if (employees.Count == 0 || serviceItems.Count == 0)
        {
            return;
        }

        var repairTasks = new List<RepairTask>();
        var today = DateTime.UtcNow;

        // Công việc đang chờ xử lý
        repairTasks.Add(new()
        {
            EmployeeId = employees[0].Id,
            ServiceItemId = serviceItems.First(s => s.Description.Contains("Bảo dưỡng định kỳ 10,000km")).Id,
            RepairOrderId = 0, // Sẽ update sau khi có RepairOrder
            Status = RepairOrderStatus.Pending,
            StartDate = null,
            EstimatedDuration = 2.0,
            CompletionDate = null,
        });

        // Công việc đang thực hiện
        repairTasks.Add(new()
        {
            EmployeeId = employees[1].Id,
            ServiceItemId = serviceItems.First(s => s.Description.Contains("Sửa chữa động cơ")).Id,
            RepairOrderId = 0,
            Status = RepairOrderStatus.InProgress,
            StartDate = today.AddDays(-2),
            EstimatedDuration = 8.0,
            CompletionDate = null,
        });

        // Công việc đã hoàn thành
        repairTasks.Add(new()
        {
            EmployeeId = employees[2].Id,
            ServiceItemId = serviceItems.First(s => s.Description.Contains("Thay dầu động cơ")).Id,
            RepairOrderId = 0,
            Status = RepairOrderStatus.Completed,
            StartDate = today.AddDays(-5),
            EstimatedDuration = 1.5,
            CompletionDate = today.AddDays(-4),
        });

        repairTasks.Add(new()
        {
            EmployeeId = employees[3].Id,
            ServiceItemId = serviceItems.First(s => s.Description.Contains("Cân bằng lốp xe")).Id,
            RepairOrderId = 0,
            Status = RepairOrderStatus.Completed,
            StartDate = today.AddDays(-3),
            EstimatedDuration = 1.0,
            CompletionDate = today.AddDays(-3),
        });

        repairTasks.Add(new()
        {
            EmployeeId = employees[4].Id,
            ServiceItemId = serviceItems.First(s => s.Description.Contains("Nạp gas điều hòa")).Id,
            RepairOrderId = 0,
            Status = RepairOrderStatus.Completed,
            StartDate = today.AddDays(-7),
            EstimatedDuration = 1.0,
            CompletionDate = today.AddDays(-7),
        });

        repairTasks.Add(new()
        {
            EmployeeId = employees[0].Id,
            ServiceItemId = serviceItems.First(s => s.Description.Contains("Sửa chữa hệ thống phanh")).Id,
            RepairOrderId = 0,
            Status = RepairOrderStatus.InProgress,
            StartDate = today.AddDays(-1),
            EstimatedDuration = 4.0,
            CompletionDate = null,
        });

        repairTasks.Add(new()
        {
            EmployeeId = employees[2].Id,
            ServiceItemId = serviceItems.First(s => s.Description.Contains("Kiểm tra xe định kỳ")).Id,
            RepairOrderId = 0,
            Status = RepairOrderStatus.Pending,
            StartDate = null,
            EstimatedDuration = 1.5,
            CompletionDate = null,
        });

        repairTasks.Add(new()
        {
            EmployeeId = employees[1].Id,
            ServiceItemId = serviceItems.First(s => s.Description.Contains("Phủ Ceramic coating")).Id,
            RepairOrderId = 0,
            Status = RepairOrderStatus.InProgress,
            StartDate = today.AddDays(-1),
            EstimatedDuration = 3.0,
            CompletionDate = null,
        });

        context.RepairTasks.AddRange(repairTasks);
        await context.SaveChangesAsync();
    }

    // =========================================================================
    // REPAIR ORDER — 8+ đơn sửa chữa
    // =========================================================================

    private static async System.Threading.Tasks.Task SeedRepairOrdersAsync(AutoXDbContext context)
    {
        if (await context.RepairOrders.AnyAsync())
        {
            return;
        }

        var customers = await context.Customers.ToListAsync();
        var vehicles = await context.Vehicles.ToListAsync();

        if (customers.Count == 0 || vehicles.Count == 0)
        {
            return;
        }

        var repairOrders = new List<RepairOrder>();
        var today = DateTime.UtcNow;

        // Đơn sửa chữa chờ xác nhận
        repairOrders.Add(new()
        {
            CustomerId = customers[0].Id,
            VehicleId = vehicles.First(v => v.CustomerId == customers[0].Id).Id,
            InvoiceId = null,
            OrderDate = today.AddDays(-3),
            CompletionDate = null,
            Status = RepairOrderStatus.Pending,
        });

        // Đơn sửa chữa đang kiểm tra
        repairOrders.Add(new()
        {
            CustomerId = customers[1].Id,
            VehicleId = vehicles.First(v => v.CustomerId == customers[1].Id).Id,
            InvoiceId = null,
            OrderDate = today.AddDays(-2),
            CompletionDate = null,
            Status = RepairOrderStatus.Inspecting,
        });

        // Đơn sửa chữa chờ báo giá
        repairOrders.Add(new()
        {
            CustomerId = customers[2].Id,
            VehicleId = vehicles.First(v => v.CustomerId == customers[2].Id).Id,
            InvoiceId = null,
            OrderDate = today.AddDays(-1),
            CompletionDate = null,
            Status = RepairOrderStatus.QuotationPending,
        });

        // Đơn sửa chữa đang thực hiện
        repairOrders.Add(new()
        {
            CustomerId = customers[3].Id,
            VehicleId = vehicles.First(v => v.CustomerId == customers[3].Id).Id,
            InvoiceId = null,
            OrderDate = today.AddDays(-5),
            CompletionDate = null,
            Status = RepairOrderStatus.InProgress,
        });

        // Đơn sửa chữa chờ phụ tùng
        repairOrders.Add(new()
        {
            CustomerId = customers[4].Id,
            VehicleId = vehicles.First(v => v.CustomerId == customers[4].Id).Id,
            InvoiceId = null,
            OrderDate = today.AddDays(-7),
            CompletionDate = null,
            Status = RepairOrderStatus.WaitingForParts,
        });

        // Đơn sửa chữa đã hoàn thành
        repairOrders.Add(new()
        {
            CustomerId = customers[5].Id,
            VehicleId = vehicles.First(v => v.CustomerId == customers[5].Id).Id,
            InvoiceId = null,
            OrderDate = today.AddDays(-10),
            CompletionDate = today.AddDays(-1),
            Status = RepairOrderStatus.Completed,
        });

        // Đơn sửa chữa đã thanh toán
        repairOrders.Add(new()
        {
            CustomerId = customers[6].Id,
            VehicleId = vehicles.FirstOrDefault(v => v.CustomerId == customers[6].Id)?.Id,
            InvoiceId = null,
            OrderDate = today.AddDays(-15),
            CompletionDate = today.AddDays(-5),
            Status = RepairOrderStatus.Paid,
        });

        // Đơn sửa chữa bị khách từ chối
        repairOrders.Add(new()
        {
            CustomerId = customers[7].Id,
            VehicleId = vehicles.FirstOrDefault(v => v.CustomerId == customers[7].Id)?.Id,
            InvoiceId = null,
            OrderDate = today.AddDays(-4),
            CompletionDate = null,
            Status = RepairOrderStatus.RejectedByCustomer,
        });

        context.RepairOrders.AddRange(repairOrders);
        await context.SaveChangesAsync();
    }

    // =========================================================================
    // REPAIR ORDER ITEM — Liên kết phụ tùng với đơn sửa chữa
    // =========================================================================

    private static async System.Threading.Tasks.Task SeedRepairOrderItemsAsync(AutoXDbContext context)
    {
        if (await context.RepairOrderItems.AnyAsync())
        {
            return;
        }

        var repairOrders = await context.RepairOrders.ToListAsync();
        var parts = await context.Parts.ToListAsync();

        if (repairOrders.Count == 0 || parts.Count == 0)
        {
            return;
        }

        var repairOrderItems = new List<RepairOrderItem>();

        // Đơn 1: 2 phụ tùng
        if (repairOrders.Count > 0)
        {
            repairOrderItems.Add(new()
            {
                RepairOrderId = repairOrders[0].Id,
                PartId = parts.First(p => p.PartCode == "SP_TOYOTA_004").Id, // Lọc dầu
                Quantity = 1,
            });

            repairOrderItems.Add(new()
            {
                RepairOrderId = repairOrders[0].Id,
                PartId = parts.First(p => p.PartCode == "SP_HONDA_005").Id, // Dầu động cơ
                Quantity = 2,
            });
        }

        // Đơn 2: 3 phụ tùng
        if (repairOrders.Count > 1)
        {
            repairOrderItems.Add(new()
            {
                RepairOrderId = repairOrders[1].Id,
                PartId = parts.First(p => p.PartCode == "SP_HONDA_001").Id, // Đèn pha
                Quantity = 1,
            });

            repairOrderItems.Add(new()
            {
                RepairOrderId = repairOrders[1].Id,
                PartId = parts.First(p => p.PartCode == "SP_HONDA_003").Id, // Lốp xe
                Quantity = 4,
            });

            repairOrderItems.Add(new()
            {
                RepairOrderId = repairOrders[1].Id,
                PartId = parts.First(p => p.PartCode == "SP_BOSCH_001").Id, // Cảm biến ABS
                Quantity = 2,
            });
        }

        // Đơn 3: 2 phụ tùng
        if (repairOrders.Count > 2)
        {
            repairOrderItems.Add(new()
            {
                RepairOrderId = repairOrders[2].Id,
                PartId = parts.First(p => p.PartCode == "SP_TOYOTA_003").Id, // Két điều hòa
                Quantity = 1,
            });

            repairOrderItems.Add(new()
            {
                RepairOrderId = repairOrders[2].Id,
                PartId = parts.First(p => p.PartCode == "RP001").Id, // Lọc gió đa năng
                Quantity = 3,
            });
        }

        // Đơn 4: 1 phụ tùng
        if (repairOrders.Count > 3)
        {
            repairOrderItems.Add(new()
            {
                RepairOrderId = repairOrders[3].Id,
                PartId = parts.First(p => p.PartCode == "SP_TOYOTA_005").Id, // Má phanh
                Quantity = 2,
            });
        }

        // Đơn 5: 2 phụ tùng
        if (repairOrders.Count > 4)
        {
            repairOrderItems.Add(new()
            {
                RepairOrderId = repairOrders[4].Id,
                PartId = parts.First(p => p.PartCode == "SP_TOYOTA_008").Id, // Bơm nước
                Quantity = 1,
            });

            repairOrderItems.Add(new()
            {
                RepairOrderId = repairOrders[4].Id,
                PartId = parts.First(p => p.PartCode == "RP017").Id, // Giảm xóc
                Quantity = 2,
            });
        }

        context.RepairOrderItems.AddRange(repairOrderItems);
        await context.SaveChangesAsync();
    }

    // =========================================================================
    // INVOICE — 6+ hóa đơn
    // =========================================================================

    private static async System.Threading.Tasks.Task SeedInvoicesAsync(AutoXDbContext context)
    {
        if (await context.Invoices.AnyAsync())
        {
            return;
        }

        var customers = await context.Customers.ToListAsync();

        if (customers.Count == 0)
        {
            return;
        }

        var invoices = new List<Invoice>();
        var today = DateTime.UtcNow;

        // Hóa đơn 1: Chưa thanh toán
        invoices.Add(new()
        {
            CustomerId = customers[0].Id,
            InvoiceNumber = "INV-2026-00001",
            InvoiceDate = today.AddDays(-5),
            PaymentStatus = PaymentStatus.Unpaid,
            TaxRate = TaxRateType.VAT10,
            DiscountType = DiscountType.None,
            Discount = 0,
        });

        // Hóa đơn 2: Đã thanh toán
        invoices.Add(new()
        {
            CustomerId = customers[1].Id,
            InvoiceNumber = "INV-2026-00002",
            InvoiceDate = today.AddDays(-10),
            PaymentStatus = PaymentStatus.Paid,
            TaxRate = TaxRateType.VAT10,
            DiscountType = DiscountType.Percentage,
            Discount = 5,
        });

        // Hóa đơn 3: Thanh toán một phần
        invoices.Add(new()
        {
            CustomerId = customers[2].Id,
            InvoiceNumber = "INV-2026-00003",
            InvoiceDate = today.AddDays(-3),
            PaymentStatus = PaymentStatus.PartiallyPaid,
            TaxRate = TaxRateType.VAT10,
            DiscountType = DiscountType.Amount,
            Discount = 500_000,
        });

        // Hóa đơn 4: Quá hạn
        invoices.Add(new()
        {
            CustomerId = customers[3].Id,
            InvoiceNumber = "INV-2026-00004",
            InvoiceDate = today.AddDays(-45),
            PaymentStatus = PaymentStatus.Overdue,
            TaxRate = TaxRateType.VAT5,
            DiscountType = DiscountType.None,
            Discount = 0,
        });

        // Hóa đơn 5: Đã hoàn tiền
        invoices.Add(new()
        {
            CustomerId = customers[4].Id,
            InvoiceNumber = "INV-2026-00005",
            InvoiceDate = today.AddDays(-20),
            PaymentStatus = PaymentStatus.Refunded,
            TaxRate = TaxRateType.VAT10,
            DiscountType = DiscountType.None,
            Discount = 0,
        });

        // Hóa đơn 6: Mới tạo
        invoices.Add(new()
        {
            CustomerId = customers[5].Id,
            InvoiceNumber = "INV-2026-00006",
            InvoiceDate = today,
            PaymentStatus = PaymentStatus.Unpaid,
            TaxRate = TaxRateType.VAT10,
            DiscountType = DiscountType.Percentage,
            Discount = 10,
        });

        context.Invoices.AddRange(invoices);
        await context.SaveChangesAsync();
    }

    // =========================================================================
    // TRANSACTION — 10+ giao dịch
    // =========================================================================

    private static async System.Threading.Tasks.Task SeedTransactionsAsync(AutoXDbContext context)
    {
        if (await context.Transactions.AnyAsync())
        {
            return;
        }

        var invoices = await context.Invoices.ToListAsync();
        var employees = await context.Employees.ToListAsync();

        if (invoices.Count == 0 || employees.Count == 0)
        {
            return;
        }

        var transactions = new List<Transaction>();
        var today = DateTime.UtcNow;

        // Giao dịch 1: Thu tiền từ hóa đơn 2 (Đã thanh toán)
        transactions.Add(new()
        {
            InvoiceId = invoices[1].Id,
            Type = TransactionType.Revenue,
            PaymentMethod = PaymentMethod.BankTransfer,
            Status = TransactionStatus.Completed,
            Amount = 5_177_500,
            Description = "Thanh toán hóa đơn INV-2026-00002",
            TransactionDate = invoices[1].InvoiceDate.AddDays(3),
            CreatedBy = employees[0].Id,
            ModifiedBy = null,
            UpdatedAt = null,
            IsReversed = false,
        });

        // Giao dịch 2: Thu tiền từ hóa đơn 3 (Thanh toán một phần)
        transactions.Add(new()
        {
            InvoiceId = invoices[2].Id,
            Type = TransactionType.Revenue,
            PaymentMethod = PaymentMethod.Cash,
            Status = TransactionStatus.Completed,
            Amount = 4_000_000,
            Description = "Thanh toán một phần hóa đơn INV-2026-00003",
            TransactionDate = invoices[2].InvoiceDate.AddDays(2),
            CreatedBy = employees[1].Id,
            ModifiedBy = null,
            UpdatedAt = null,
            IsReversed = false,
        });

        // Giao dịch 3: Giao dịch chờ xử lý
        transactions.Add(new()
        {
            InvoiceId = invoices[0].Id,
            Type = TransactionType.Revenue,
            PaymentMethod = PaymentMethod.VNPay,
            Status = TransactionStatus.Pending,
            Amount = 2_750_000,
            Description = "Chờ xác nhận thanh toán hóa đơn INV-2026-00001",
            TransactionDate = today.AddDays(-1),
            CreatedBy = employees[2].Id,
            ModifiedBy = null,
            UpdatedAt = null,
            IsReversed = false,
        });

        // Giao dịch 4: Chi tiền mua phụ tùng
        transactions.Add(new()
        {
            InvoiceId = invoices[1].Id,
            Type = TransactionType.Expense,
            PaymentMethod = PaymentMethod.BankTransfer,
            Status = TransactionStatus.Completed,
            Amount = 1_500_000,
            Description = "Chi tiền mua phụ tùng từ nhà cung cấp",
            TransactionDate = today.AddDays(-15),
            CreatedBy = employees[3].Id,
            ModifiedBy = null,
            UpdatedAt = null,
            IsReversed = false,
        });

        // Giao dịch 5: Hoàn tiền cho khách
        transactions.Add(new()
        {
            InvoiceId = invoices[4].Id,
            Type = TransactionType.Refund,
            PaymentMethod = PaymentMethod.BankTransfer,
            Status = TransactionStatus.Completed,
            Amount = 1_650_000,
            Description = "Hoàn tiền do lỗi dịch vụ",
            TransactionDate = invoices[4].InvoiceDate.AddDays(10),
            CreatedBy = employees[4].Id,
            ModifiedBy = null,
            UpdatedAt = null,
            IsReversed = false,
        });

        // Giao dịch 6: Lỗi thanh toán (Failed)
        transactions.Add(new()
        {
            InvoiceId = invoices[3].Id,
            Type = TransactionType.Revenue,
            PaymentMethod = PaymentMethod.CreditCard,
            Status = TransactionStatus.Failed,
            Amount = 3_360_000,
            Description = "Thanh toán thất bại - Vui lòng thử lại",
            TransactionDate = today.AddDays(-2),
            CreatedBy = employees[0].Id,
            ModifiedBy = null,
            UpdatedAt = null,
            IsReversed = false,
        });

        // Giao dịch 7: Tạm ứng cho nhân viên
        transactions.Add(new()
        {
            InvoiceId = invoices[0].Id,
            Type = TransactionType.AdvancePayment,
            PaymentMethod = PaymentMethod.Cash,
            Status = TransactionStatus.Completed,
            Amount = 500_000,
            Description = "Tạm ứng lương cho Trần Minh Hùng",
            TransactionDate = today.AddDays(-20),
            CreatedBy = employees[5].Id,
            ModifiedBy = null,
            UpdatedAt = null,
            IsReversed = false,
        });

        // Giao dịch 8: Tiền đặt cọc
        transactions.Add(new()
        {
            InvoiceId = invoices[5].Id,
            Type = TransactionType.Deposit,
            PaymentMethod = PaymentMethod.Cash,
            Status = TransactionStatus.Completed,
            Amount = 1_000_000,
            Description = "Khách hàng đặt cọc cho dịch vụ sửa chữa",
            TransactionDate = invoices[5].InvoiceDate,
            CreatedBy = employees[1].Id,
            ModifiedBy = null,
            UpdatedAt = null,
            IsReversed = false,
        });

        // Giao dịch 9: Chuyển khoản nội bộ
        transactions.Add(new()
        {
            InvoiceId = invoices[1].Id,
            Type = TransactionType.InternalTransfer,
            PaymentMethod = PaymentMethod.BankTransfer,
            Status = TransactionStatus.Completed,
            Amount = 2_000_000,
            Description = "Chuyển tiền từ quỹ tiền mặt sang tài khoản ngân hàng",
            TransactionDate = today.AddDays(-8),
            CreatedBy = employees[5].Id,
            ModifiedBy = null,
            UpdatedAt = null,
            IsReversed = false,
        });

        // Giao dịch 10: Giao dịch bị đảo ngược
        transactions.Add(new()
        {
            InvoiceId = invoices[2].Id,
            Type = TransactionType.Revenue,
            PaymentMethod = PaymentMethod.Momo,
            Status = TransactionStatus.Completed,
            Amount = 1_500_000,
            Description = "Giao dịch đã hoàn tiền do yêu cầu khách hàng",
            TransactionDate = today.AddDays(-30),
            CreatedBy = employees[0].Id,
            ModifiedBy = employees[1].Id,
            UpdatedAt = today.AddDays(-5),
            IsReversed = true,
        });

        context.Transactions.AddRange(transactions);
        await context.SaveChangesAsync();
    }
}