// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Customers;
using AutoX.Gara.Domain.Entities.Identity;
using AutoX.Gara.Domain.Entities.Inventory;
using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Cars;
using AutoX.Gara.Domain.Enums.Customers;
using AutoX.Gara.Domain.Enums.Parts;
using AutoX.Gara.Domain.Enums.Payments;
using Microsoft.EntityFrameworkCore;
using Nalix.Common.Security.Enums;
using Nalix.Shared.Security.Credentials;
using System;
using System.Collections.Generic;

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
        await SeedVehiclesAsync(context);
        await SeedSuppliersAsync(context);
        await SeedSparePartsAsync(context);
        await SeedReplacementPartsAsync(context);
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

    private static async System.Threading.Tasks.Task SeedSparePartsAsync(AutoXDbContext context)
    {
        if (await context.SpareParts.AnyAsync())
        {
            return;
        }

        var suppliers = await context.Suppliers.ToListAsync();
        if (suppliers.Count == 0)
        {
            return;
        }

        Int32 IdOf(String keyword) => suppliers.Find(s => s.Name.Contains(keyword))?.Id
            ?? throw new InvalidOperationException($"Supplier có từ khóa '{keyword}' không tồn tại.");

        Int32 toyotaId = IdOf("Toyota");
        Int32 hondaId = IdOf("Honda");
        Int32 boschId = IdOf("Bosch");
        Int32 starkId = IdOf("Stark");

        var spareParts = new List<SparePart>
        {
            // --- Toyota ---
            new()
            {
                SupplierId        = toyotaId,
                PartName          = "Giảm xóc trước Toyota Fortuner (KYB OEM)",
                PartCategory      = PartCategory.Suspension,
                PurchasePrice     = 1_400_000,
                SellingPrice      = 2_000_000,
                InventoryQuantity = 10,
                IsDiscontinued    = false,
            },
            new()
            {
                SupplierId        = toyotaId,
                PartName          = "Thanh cân bằng Toyota Camry 2.5",
                PartCategory      = PartCategory.Steering,
                PurchasePrice     = 620_000,
                SellingPrice      = 920_000,
                InventoryQuantity = 8,
                IsDiscontinued    = false,
            },
            new()
            {
                SupplierId        = toyotaId,
                PartName          = "Két điều hòa Toyota Innova 2.0",
                PartCategory      = PartCategory.AirConditioning,
                PurchasePrice     = 1_850_000,
                SellingPrice      = 2_700_000,
                InventoryQuantity = 5,
                IsDiscontinued    = false,
            },
            new()
            {
                SupplierId        = toyotaId,
                PartName          = "Lọc dầu Toyota Camry",
                PartCategory      = PartCategory.Filter,
                PurchasePrice     = 85_000,
                SellingPrice      = 120_000,
                InventoryQuantity = 50,
                IsDiscontinued    = false,
            },
            new()
            {
                SupplierId        = toyotaId,
                PartName          = "Má phanh trước Toyota Camry",
                PartCategory      = PartCategory.Brake,
                PurchasePrice     = 450_000,
                SellingPrice      = 650_000,
                InventoryQuantity = 20,
                IsDiscontinued    = false,
            },
            new()
            {
                SupplierId        = toyotaId,
                PartName          = "Bình ắc quy Toyota Vios 45Ah",
                PartCategory      = PartCategory.Electrical,
                PurchasePrice     = 1_100_000,
                SellingPrice      = 1_550_000,
                InventoryQuantity = 15,
                IsDiscontinued    = false,
            },
            new()
            {
                SupplierId        = toyotaId,
                PartName          = "Dây curoa cam Toyota Innova 2.0",
                PartCategory      = PartCategory.Engine,
                PurchasePrice     = 320_000,
                SellingPrice      = 480_000,
                InventoryQuantity = 30,
                IsDiscontinued    = false,
            },
            new()
            {
                SupplierId        = toyotaId,
                PartName          = "Bơm nước Toyota Fortuner 2.7",
                PartCategory      = PartCategory.Cooling,
                PurchasePrice     = 850_000,
                SellingPrice      = 1_250_000,
                InventoryQuantity = 8,
                IsDiscontinued    = false,
            },
            // --- Honda ---
            new()
            {
                SupplierId        = hondaId,
                PartName          = "Đèn pha LED Honda CR-V 2023 (trái)",
                PartCategory      = PartCategory.Lighting,
                PurchasePrice     = 3_500_000,
                SellingPrice      = 5_200_000,
                InventoryQuantity = 4,
                IsDiscontinued    = false,
            },
            new()
            {
                SupplierId        = hondaId,
                PartName          = "Kính chắn gió Honda City (chống UV)",
                PartCategory      = PartCategory.UVGlass,
                PurchasePrice     = 2_200_000,
                SellingPrice      = 3_100_000,
                InventoryQuantity = 3,
                IsDiscontinued    = false,
            },
            new()
            {
                SupplierId        = hondaId,
                PartName          = "Lốp xe Honda HR-V 215/60R16",
                PartCategory      = PartCategory.WheelAndTire,
                PurchasePrice     = 1_600_000,
                SellingPrice      = 2_200_000,
                InventoryQuantity = 16,
                IsDiscontinued    = false,
            },
            // --- Stark Industries (tiếp theo) ---
            new()
            {
                SupplierId        = starkId,
                PartName          = "HUD Holographic Stark — Hiển Thị 3D Toàn Cảnh 360°",
                PartCategory      = PartCategory.HUD,
                PurchasePrice     = 75_000_000,
                SellingPrice      = 120_000_000,
                InventoryQuantity = 2,
                IsDiscontinued    = false,
            },
            new()
            {
                SupplierId        = starkId,
                PartName          = "Cánh Gió Điều Khiển Tự Động Stark Aerodynamics Kit",
                PartCategory      = PartCategory.Aerodynamics,
                PurchasePrice     = 200_000_000,
                SellingPrice      = 350_000_000,
                InventoryQuantity = 1,
                IsDiscontinued    = false,
            },
            new()
            {
                SupplierId        = hondaId,
                PartName          = "Bugi Honda CR-V 1.5T (NGK)",
                PartCategory      = PartCategory.Ignition,
                PurchasePrice     = 65_000,
                SellingPrice      = 95_000,
                InventoryQuantity = 100,
                IsDiscontinued    = false,
            },
            new()
            {
                SupplierId        = hondaId,
                PartName          = "Dầu động cơ Honda Ultra 5W-30 (4L)",
                PartCategory      = PartCategory.Lubrication,
                PurchasePrice     = 280_000,
                SellingPrice      = 390_000,
                InventoryQuantity = 60,
                IsDiscontinued    = false,
            },
            new()
            {
                SupplierId        = hondaId,
                PartName          = "Lọc gió Honda City",
                PartCategory      = PartCategory.Filter,
                PurchasePrice     = 70_000,
                SellingPrice      = 110_000,
                InventoryQuantity = 45,
                IsDiscontinued    = false,
            },
            new()
            {
                SupplierId        = hondaId,
                PartName          = "Cao su gạt mưa Honda HR-V (cặp)",
                PartCategory      = PartCategory.Maintenance,
                PurchasePrice     = 120_000,
                SellingPrice      = 180_000,
                InventoryQuantity = 35,
                IsDiscontinued    = false,
            },
            new()
            {
                SupplierId        = hondaId,
                PartName          = "Bộ piston và xéc măng Honda Civic 1.8",
                PartCategory      = PartCategory.Engine,
                PurchasePrice     = 3_200_000,
                SellingPrice      = 4_500_000,
                InventoryQuantity = 5,
                IsDiscontinued    = false,
            },
            // --- Bosch ---
            new()
            {
                SupplierId        = boschId,
                PartName          = "Cảm biến ABS Bosch bánh trước (universal)",
                PartCategory      = PartCategory.ABS,
                PurchasePrice     = 480_000,
                SellingPrice      = 720_000,
                InventoryQuantity = 12,
                IsDiscontinued    = false,
            },
            new()
            {
                SupplierId        = boschId,
                PartName          = "Máy phát điện Bosch 14V 90A",
                PartCategory      = PartCategory.Electrical,
                PurchasePrice     = 2_800_000,
                SellingPrice      = 3_900_000,
                InventoryQuantity = 6,
                IsDiscontinued    = false,
            },
            new()
            {
                SupplierId        = boschId,
                PartName          = "Kim phun nhiên liệu Bosch (universal)",
                PartCategory      = PartCategory.FuelInjection,
                PurchasePrice     = 750_000,
                SellingPrice      = 1_100_000,
                InventoryQuantity = 20,
                IsDiscontinued    = false,
            },
            new()
            {
                SupplierId        = boschId,
                PartName          = "Cảm biến oxy Bosch O2 Sensor",
                PartCategory      = PartCategory.SensorsAndModules,
                PurchasePrice     = 550_000,
                SellingPrice      = 820_000,
                InventoryQuantity = 18,
                IsDiscontinued    = false,
            },
            new()
            {
                SupplierId        = boschId,
                PartName          = "Bugi Bosch Iridium đa hãng",
                PartCategory      = PartCategory.Ignition,
                PurchasePrice     = 95_000,
                SellingPrice      = 145_000,
                InventoryQuantity = 80,
                IsDiscontinued    = false,
            },
            // --- STARK INDUSTRIES ---
            new()
            {
                SupplierId        = starkId,
                PartName          = "Động Cơ Tên Lửa Thu Nhỏ Stark Mk.1",
                PartCategory      = PartCategory.Engine,
                PurchasePrice     = 500_000_000,
                SellingPrice      = 999_000_000,
                InventoryQuantity = 2,
                IsDiscontinued    = false,
            },
            new()
            {
                SupplierId        = starkId,
                PartName          = "Radar Phát Hiện Địch Stark (Car Edition)",
                PartCategory      = PartCategory.SensorsAndModules,
                PurchasePrice     = 150_000_000,
                SellingPrice      = 250_000_000,
                InventoryQuantity = 3,
                IsDiscontinued    = false,
            },
            new()
            {
                SupplierId        = starkId,
                PartName          = "Arc Reactor Mini (Thay Bình Ắc Quy Thông Thường)",
                PartCategory      = PartCategory.BatteryAndModules,
                PurchasePrice     = 1_000_000_000,
                SellingPrice      = 2_000_000_000,
                InventoryQuantity = 1,
                IsDiscontinued    = false,
            },
            new()
            {
                SupplierId        = starkId,
                PartName          = "Flux Capacitor (Hàng Chính Hãng 1885 — Dành Cho DeLorean)",
                PartCategory      = PartCategory.Electrical,
                PurchasePrice     = 88_000_000,
                SellingPrice      = 88_888_888,
                InventoryQuantity = 1,
                IsDiscontinued    = false,
            },
        };

        context.SpareParts.AddRange(spareParts);
        await context.SaveChangesAsync();
    }

    // =========================================================================
    // REPLACEMENT PART — 12 phụ tùng thay thế
    // =========================================================================

    private static async System.Threading.Tasks.Task SeedReplacementPartsAsync(AutoXDbContext context)
    {
        if (await context.ReplacementParts.AnyAsync())
        {
            return;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var parts = new List<ReplacementPart>
        {
            // --- Thực tế ---
            new()
            {
                PartCode     = "RP001",
                PartName     = "Lọc gió động cơ đa năng",
                Manufacturer = "Denso",
                Quantity     = 40,
                UnitPrice    = 150_000,
                DateAdded    = today,
                ExpiryDate   = today.AddYears(3),
            },
            new()
            {
                PartCode     = "RP002",
                PartName     = "Dây curoa dẫn động",
                Manufacturer = "Gates",
                Quantity     = 25,
                UnitPrice    = 320_000,
                DateAdded    = today,
                ExpiryDate   = today.AddYears(5),
            },
            new()
            {
                PartCode     = "RP003",
                PartName     = "Má phanh sau đa hãng",
                Manufacturer = "Brembo",
                Quantity     = 16,
                UnitPrice    = 580_000,
                DateAdded    = today,
                ExpiryDate   = null,
            },
            new()
            {
                PartCode     = "RP004",
                PartName     = "Bình chứa dầu phanh DOT4 500ml",
                Manufacturer = "Castrol",
                Quantity     = 30,
                UnitPrice    = 95_000,
                DateAdded    = today,
                ExpiryDate   = today.AddYears(2),
            },
            new()
            {
                PartCode     = "RP005",
                PartName     = "Nước làm mát Ready-to-Use 1L",
                Manufacturer = "Prestone",
                Quantity     = 50,
                UnitPrice    = 75_000,
                DateAdded    = today,
                ExpiryDate   = today.AddYears(4),
            },
            new()
            {
                PartCode     = "RP006",
                PartName     = "Bộ gioăng nắp máy đa dụng",
                Manufacturer = "Fel-Pro",
                Quantity     = 10,
                UnitPrice    = 420_000,
                DateAdded    = today,
                ExpiryDate   = null,
            },
            new()
            {
                PartCode     = "RP007",
                PartName     = "Cao su chắn bùn bộ 4 bánh",
                Manufacturer = "WeatherTech",
                Quantity     = 20,
                UnitPrice    = 380_000,
                DateAdded    = today,
                ExpiryDate   = null,
            },
            new()
            {
                PartCode     = "RP008",
                PartName     = "Bugi NGK Iridium IX hộp 4 cái",
                Manufacturer = "NGK",
                Quantity     = 60,
                UnitPrice    = 380_000,
                DateAdded    = today,
                ExpiryDate   = today.AddYears(5),
            },
            new()
            {
                PartCode     = "RP009",
                PartName     = "Lọc nhiên liệu đa hãng",
                Manufacturer = "Bosch",
                Quantity     = 35,
                UnitPrice    = 175_000,
                DateAdded    = today,
                ExpiryDate   = today.AddYears(3),
            },
            new()
            {
                PartCode     = "RP013",
                PartName     = "Dầu động cơ tổng hợp 5W-30 4L",
                Manufacturer = "Mobil 1",
                Quantity     = 45,
                UnitPrice    = 420_000,
                DateAdded    = today,
                ExpiryDate   = today.AddYears(5),
            },
            new()
            {
                PartCode     = "RP014",
                PartName     = "Ắc quy khô 12V 45Ah",
                Manufacturer = "Bosch",
                Quantity     = 12,
                UnitPrice    = 1_350_000,
                DateAdded    = today,
                ExpiryDate   = today.AddYears(3),
            },
            new()
            {
                PartCode     = "RP015",
                PartName     = "Lọc nhớt đa hãng",
                Manufacturer = "Mann-Filter",
                Quantity     = 80,
                UnitPrice    = 85_000,
                DateAdded    = today,
                ExpiryDate   = today.AddYears(4),
            },
            new()
            {
                PartCode     = "RP016",
                PartName     = "Gạt mưa mềm 24 inch",
                Manufacturer = "Bosch",
                Quantity     = 55,
                UnitPrice    = 120_000,
                DateAdded    = today,
                ExpiryDate   = today.AddYears(2),
            },
            new()
            {
                PartCode     = "RP017",
                PartName     = "Giảm xóc trước thay thế",
                Manufacturer = "KYB",
                Quantity     = 18,
                UnitPrice    = 1_200_000,
                DateAdded    = today,
                ExpiryDate   = null,
            },
            new()
            {
                PartCode     = "RP018",
                PartName     = "Bộ lọc cabin khử mùi",
                Manufacturer = "3M",
                Quantity     = 40,
                UnitPrice    = 185_000,
                DateAdded    = today,
                ExpiryDate   = today.AddYears(2),
            },
            new()
            {
                PartCode     = "RP010",
                PartName     = "Bộ Giáp Iron Man Mk.85 (Thay Cản Trước Xe)",
                Manufacturer = "Stark Industries",
                Quantity     = 1,
                UnitPrice    = 9_999_999_999m,
                DateAdded    = today,
                ExpiryDate   = null,
            },
            new()
            {
                PartCode     = "RP011",
                PartName     = "Đầu Đạn Tên Lửa RPG Thay Thế Cản Sau Xe Tăng T-54",
                Manufacturer = "Unknown",
                Quantity     = 5,
                UnitPrice    = 50_000_000,
                DateAdded    = today,
                ExpiryDate   = today.AddYears(99),
            },
            new()
            {
                PartCode     = "RP012",
                PartName     = "Xăng Hypersonic Pha Lê Nhiên Liệu Cho Động Cơ Tên Lửa",
                Manufacturer = "Area 51 Fuel Co.",
                Quantity     = 0, // hết hàng — đặt trước cho NASA
                UnitPrice    = 500_000_000,
                DateAdded    = today,
                ExpiryDate   = today.AddDays(30), // dễ bay hơi, hạn ngắn
            },
            new()
            {
                PartCode     = "RP019",
                PartName     = "Lốp Xe Graphene Siêu Dẫn Nhiệt Chống Nổ Cấp Quân Sự",
                Manufacturer = "MIT Advanced Materials Lab",
                Quantity     = 4,
                UnitPrice    = 45_000_000,
                DateAdded    = today,
                ExpiryDate   = today.AddYears(50),
            },
            new()
            {
                PartCode     = "RP020",
                PartName     = "Bộ Pin Hydrogen Fuel Cell 150kW Thay Thế Động Cơ Đốt Trong",
                Manufacturer = "Toyota Research Institute",
                Quantity     = 2,
                UnitPrice    = 320_000_000,
                DateAdded    = today,
                ExpiryDate   = today.AddYears(10),
            },
            new()
            {
                PartCode     = "RP021",
                PartName     = "Cảm Biến LiDAR 128-Layer Tự Lái Cấp Độ 5",
                Manufacturer = "Waymo",
                Quantity     = 1,
                UnitPrice    = 180_000_000,
                DateAdded    = today,
                ExpiryDate   = today.AddYears(5),
            },
        };

        context.ReplacementParts.AddRange(parts);
        await context.SaveChangesAsync();
    }
}