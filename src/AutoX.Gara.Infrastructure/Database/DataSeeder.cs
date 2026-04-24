// Copyright (c) 2026 PPN Corporation. All rights reserved.



using AutoX.Gara.Domain.Entities.Billings;
using AutoX.Gara.Domain.Entities.Customers;
using AutoX.Gara.Domain.Entities.Identity;
using AutoX.Gara.Domain.Entities.Inventory;
using AutoX.Gara.Domain.Entities.Suppliers;
using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Cars;
using AutoX.Gara.Domain.Enums.Customers;
using AutoX.Gara.Domain.Enums.Employees;
using AutoX.Gara.Domain.Enums.Parts;
using AutoX.Gara.Domain.Enums.Payments;
using Microsoft.EntityFrameworkCore;
using Nalix.Common.Security;
using Nalix.Framework.Security.Hashing;
using System;
using System.Collections.Generic;

using System.Linq;



namespace AutoX.Gara.Infrastructure.Database;



/// <summary>

/// Class ch?u tr?ch nhi?m d? dữ liệu m?u (seed data) v?o co s? dữ liệu.

/// Ch? ch?y khi database chua c? dữ liệu d? tr?nh tr?ng l?p.

/// </summary>

public static class DataSeeder

{

    /// <summary>

    /// Entry point ch?nh: g?i h?m n?y t? Program.cs khi kh?i d?ng ?ng d?ng.

    /// </summary>

    /// <example>

    /// // Trong Program.cs, g?i sau khi build app v? tru?c app.Run():

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
        await SeedEmployeeSalariesAsync(context);
        await SeedVehiclesAsync(context);
        await SeedSuppliersAsync(context);
        await SeedPartsAsync(context);
        await SeedServiceItemsAsync(context);
    }


    // =========================================================================

    // ACCOUNT ? 1 admin + 2 staff

    // =========================================================================



    /// <summary>

    /// T?i kho?n m?c d?nh:

    ///   admin     / Abcd1234@  ? ADMINISTRATOR

    ///   nhanvien1 / Abcd1234@  ? STAFF

    ///   nhanvien2 / Abcd1234@  ? STAFF

    /// </summary>

    private static async System.Threading.Tasks.Task SeedAccountsAsync(AutoXDbContext context)

    {

        if (await context.Accounts.AnyAsync())

        {

            return;

        }



        static Account MakeAccount(String username, String password, PermissionLevel role, Boolean active = false)

        {

            // Pbkdf2.Hash l? helper t? Nalix ? ph?i kh?p v?i logic x?c th?c trong hệ thống

            Pbkdf2.Hash(password, out byte[] salt, out byte[] hash);
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

            MakeAccount("admin", "Abcd1234@", PermissionLevel.OWNER),

            MakeAccount("nhanvien1", "Abcd1234@", PermissionLevel.USER),

            MakeAccount("nhanvien2", "Abcd1234@", PermissionLevel.USER)

        );



        await context.SaveChangesAsync();

    }



    // =========================================================================

    // CUSTOMER ? 10 kh?ch h?ng

    // =========================================================================



    private static async System.Threading.Tasks.Task SeedCustomersAsync(AutoXDbContext context)

    {

        if (await context.Customers.AnyAsync())

        {

            return;

        }



        var customers = new List<Customer>

        {

            // --- Kh?ch c? nh?n ---

            new()

            {

                Name        = "Nguy?n Van An",

                PhoneNumber = "0901234567",

                Email       = "an.nguyen@email.com",

                Address     = "123 L? Thu?ng Ki?t, Q.10, TP.HCM",

                Gender      = Gender.Male,

                DateOfBirth = new DateTime(1990, 5, 15, 0, 0, 0, DateTimeKind.Utc),

                TaxCode     = "0123456789",

                Type        = CustomerType.Individual,

                Membership  = MembershipLevel.Standard,

                Debt        = 0,

                Notes       = "Kh?ch th?n thi?n, hay d?n v?o cu?i tu?n.",

            },

            new()

            {

                Name        = "Tr?n Th? B?nh",

                PhoneNumber = "0912345678",

                Email       = "binh.tran@email.com",

                Address     = "456 Nguy?n Tr?i, Q.5, TP.HCM",

                Gender      = Gender.Female,

                DateOfBirth = new DateTime(1985, 8, 22, 0, 0, 0, DateTimeKind.Utc),

                Type        = CustomerType.Individual,

                Membership  = MembershipLevel.Silver,

                Debt        = 500_000,

                Notes       = "Thu?ng y?u c?u thay d?u d?nh k? m?i 3 th?ng.",

            },

            new()

            {

                Name        = "L? Ho?ng Ph?c",

                PhoneNumber = "0933456789",

                Email       = "phuc.le@email.com",

                Address     = "789 C?ch M?ng Th?ng 8, Q.3, TP.HCM",

                Gender      = Gender.Male,

                DateOfBirth = new DateTime(1995, 12, 1, 0, 0, 0, DateTimeKind.Utc),

                Type        = CustomerType.Individual,

                Membership  = MembershipLevel.Gold,

                Debt        = 1_200_000,

                Notes       = "Kh?ch VIP, thu?ng mang BMW d?n b?o du?ng.",

            },

            new()

            {

                Name        = "Ph?m Th? Kim Chi",

                PhoneNumber = "0944567890",

                Email       = "chi.pham@email.com",

                Address     = "321 V? Van T?n, Q.3, TP.HCM",

                Gender      = Gender.Female,

                DateOfBirth = new DateTime(2000, 3, 8, 0, 0, 0, DateTimeKind.Utc),

                Type        = CustomerType.Individual,

                Membership  = MembershipLevel.Standard,

                Debt        = 0,

                Notes       = "Xe hay b? x?t l?p, c?n ki?m tra ?p su?t d?nh k?.",

            },

            new()

            {

                Name        = "??ng Qu?c H?ng",

                PhoneNumber = "0955678901",

                Email       = "hung.dang@email.com",

                Address     = "654 Phan X?ch Long, Q.Ph? Nhu?n, TP.HCM",

                Gender      = Gender.Male,

                DateOfBirth = new DateTime(1978, 7, 19, 0, 0, 0, DateTimeKind.Utc),

                Type        = CustomerType.Fleet,

                Membership  = MembershipLevel.Platinum,

                Debt        = 5_000_000,

                Notes       = "S? h?u d?i xe 5 chi?c, k? h?p d?ng b?o du?ng h?ng th?ng.",

            },

            // --- Kh?ch doanh nghi?p ---

            new()

            {

                Name        = "C?ng ty TNHH V?n T?i Ph? Th?nh",

                PhoneNumber = "0283456789",

                Email       = "phuthinh.transport@company.vn",

                Address     = "789 ?i?n Bi?n Ph?, Q.B?nh Th?nh, TP.HCM",

                Gender      = Gender.None,

                TaxCode     = "0312345678901",

                Type        = CustomerType.Business,

                Membership  = MembershipLevel.Gold,

                Debt        = 2_000_000,

                Notes       = "??i xe t?i, b?o du?ng d?nh k? h?ng th?ng.",

            },

            new()

            {

                Name        = "C?ng ty CP Grab Vi?t Nam",

                PhoneNumber = "0284567890",

                Email       = "fleet@grab.vn",

                Address     = "T?a nh? Viettel, M? Tr?, H? N?i",

                Gender      = Gender.None,

                TaxCode     = "0106139890001",

                Type        = CustomerType.Fleet,

                Membership  = MembershipLevel.Platinum,

                Debt        = 0,

                Notes       = "H?p d?ng d?i h?n, d?i xe GrabCar 200 chi?c.",

            },

            new()

            {

                Name        = "Nguy?n Van Xe Tang",

                PhoneNumber = "0969696969",

                Email       = "xetang.t54@quandoi.vn",

                Address     = "B? Qu?c Ph?ng, 7 Nguy?n Tri Phuong, H? N?i",

                Gender      = Gender.Male,

                DateOfBirth = new DateTime(1975, 4, 30, 0, 0, 0, DateTimeKind.Utc),

                Type        = CustomerType.Government,

                Membership  = MembershipLevel.Diamond,

                Debt        = 0,

                Notes       = "Mang xe tang T-54 v?o thay nh?t. Th? b? ch?y h?t. L?n sau b?o tru?c.",

            },

            new()

            {

                Name        = "T? Ph? ElonUsk",

                PhoneNumber = "0123456789",

                Email       = "elon.usk@spacex-gara.vn",

                Address     = "SpaceX HQ, Boca Chica, Texas (chi nh?nh TP.HCM)",

                Gender      = Gender.Male,

                DateOfBirth = new DateTime(1971, 6, 28, 0, 0, 0, DateTimeKind.Utc),

                Type        = CustomerType.VIP,

                Membership  = MembershipLevel.Diamond,

                Debt        = 999_999_999,

                Notes       = "Mang Tesla Roadster dang bay quanh M?t Tr?i v? gara b?o du?ng. H?i th? c? th? bay l?n s?a kh?ng.",

            },

            new()

            {

                Name        = "Marty McFly",

                PhoneNumber = "0888888888",

                Email       = "marty@delorean-garage.vn",

                Address     = "Hill Valley, California (Nam 1985)",

                Gender      = Gender.Male,

                DateOfBirth = new DateTime(1968, 6, 9, 0, 0, 0, DateTimeKind.Utc),

                Type        = CustomerType.Individual,

                Membership  = MembershipLevel.Diamond,

                Debt        = 0,

                Notes       = "Kim t?c d? b? k?t ? 88mph. Nghi h?ng flux capacitor. Ph?i s?a tru?c ng?y 26/10.",

            },

        };



        context.Customers.AddRange(customers);

        await context.SaveChangesAsync();

    }



    // =========================================================================

    // EMPLOYEE ? 10 nh?n vi?n v?i c?c v? tr? kh?c nhau

    // =========================================================================



    private static async System.Threading.Tasks.Task SeedEmployeesAsync(AutoXDbContext context)
    {
        if (await context.Employees.AnyAsync())
        {
            return;
        }


        var employees = new List<Employee>

    {

        // --- K? thu?t vi?n co kh? co b?n ---

        new()

        {

            Name = "Tr?n Minh H?ng",

            Gender = Gender.Male,

            DateOfBirth = new DateTime(1990, 3, 15, 0, 0, 0, DateTimeKind.Utc),

            Address = "234 Nguy?n Hu?, Q.1, TP.HCM",

            PhoneNumber = "0901234567",

            Email = "hung.tran@autox-gara.vn",

            Position = Position.Technician,

            Status = EmploymentStatus.Active,

            StartDate = new DateTime(2020, 1, 15, 0, 0, 0, DateTimeKind.Utc),

            EndDate = null,

        },



        // --- Th? m?y g?m ---

        new()

        {

            Name = "L? Van Ph?t",

            Gender = Gender.Male,

            DateOfBirth = new DateTime(1992, 7, 22, 0, 0, 0, DateTimeKind.Utc),

            Address = "567 T? K?, Q.12, TP.HCM",

            PhoneNumber = "0912345678",

            Email = "phat.le@autox-gara.vn",

            Position = Position.UnderCarMechanic,

            Status = EmploymentStatus.Active,

            StartDate = new DateTime(2019, 6, 1, 0, 0, 0, DateTimeKind.Utc),

            EndDate = null,

        },



        // --- Chuy?n vi?n ch?n do?n ---

        new()

        {

            Name = "Nguy?n Qu?c Vinh",

            Gender = Gender.Male,

            DateOfBirth = new DateTime(1995, 11, 8, 0, 0, 0, DateTimeKind.Utc),

            Address = "890 C?ch M?ng Th?ng 8, Q.3, TP.HCM",

            PhoneNumber = "0933456789",

            Email = "vinh.nguyen@autox-gara.vn",

            Position = Position.DiagnosticSpecialist,

            Status = EmploymentStatus.Active,

            StartDate = new DateTime(2021, 3, 20, 0, 0, 0, DateTimeKind.Utc),

            EndDate = null,

        },



        // --- Th? di?n ? t? ---

        new()

        {

            Name = "Ho?ng Van ?o?n",

            Gender = Gender.Male,

            DateOfBirth = new DateTime(1988, 5, 10, 0, 0, 0, DateTimeKind.Utc),

            Address = "123 L? Thu?ng Ki?t, Q.10, TP.HCM",

            PhoneNumber = "0944567890",

            Email = "doan.hoang@autox-gara.vn",

            Position = Position.AutoElectrician,

            Status = EmploymentStatus.Active,

            StartDate = new DateTime(2018, 9, 1, 0, 0, 0, DateTimeKind.Utc),

            EndDate = null,

        },



        // --- Nh?n vi?n tu v?n d?ch v? ---

        new()

        {

            Name = "Truong Th? Ng?",

            Gender = Gender.Female,

            DateOfBirth = new DateTime(1991, 2, 28, 0, 0, 0, DateTimeKind.Utc),

            Address = "456 Nguy?n Tr?i, Q.5, TP.HCM",

            PhoneNumber = "0955678901",

            Email = "nga.truong@autox-gara.vn",

            Position = Position.Advisor,

            Status = EmploymentStatus.Active,

            StartDate = new DateTime(2020, 7, 15, 0, 0, 0, DateTimeKind.Utc),

            EndDate = null,

        },



        // --- Qu?n l? ca / Tru?ng ca ---

        new()

        {

            Name = "Phan Minh Nh?t",

            Gender = Gender.Male,

            DateOfBirth = new DateTime(1985, 9, 3, 0, 0, 0, DateTimeKind.Utc),

            Address = "789 ?i?n Bi?n Ph?, Q.B?nh Th?nh, TP.HCM",

            PhoneNumber = "0966789012",

            Email = "nhat.phan@autox-gara.vn",

            Position = Position.ShiftSupervisor,

            Status = EmploymentStatus.Active,

            StartDate = new DateTime(2017, 1, 10, 0, 0, 0, DateTimeKind.Utc),

            EndDate = null,

        },



        // --- Chuy?n vi?n son xe ---

        new()

        {

            Name = "?? Ti?n Dung",

            Gender = Gender.Male,

            DateOfBirth = new DateTime(1996, 4, 20, 0, 0, 0, DateTimeKind.Utc),

            Address = "321 V? Van T?n, Q.3, TP.HCM",

            PhoneNumber = "0977890123",

            Email = "dung.do@autox-gara.vn",

            Position = Position.Painter,

            Status = EmploymentStatus.Active,

            StartDate = new DateTime(2022, 2, 1, 0, 0, 0, DateTimeKind.Utc),

            EndDate = null,

        },



        // --- Chuy?n vi?n s?a ch?a d?ng co ---

        new()

        {

            Name = "V? Thanh Son",

            Gender = Gender.Male,

            DateOfBirth = new DateTime(1994, 8, 15, 0, 0, 0, DateTimeKind.Utc),

            Address = "654 Phan X?ch Long, Q.Ph? Nhu?n, TP.HCM",

            PhoneNumber = "0988901234",

            Email = "son.vo@autox-gara.vn",

            Position = Position.EngineSpecialist,

            Status = EmploymentStatus.Inactive,

            StartDate = new DateTime(2021, 5, 1, 0, 0, 0, DateTimeKind.Utc),

            EndDate = new DateTime(2024, 12, 31, 0, 0, 0, DateTimeKind.Utc),

        },



        // --- Nh?n vi?n tu v?n ph? t?ng ---

        new()

        {

            Name = "B?i Th? Huong",

            Gender = Gender.Female,

            DateOfBirth = new DateTime(1993, 6, 12, 0, 0, 0, DateTimeKind.Utc),

            Address = "147 T?n K? T?n Qu?, Q.6, TP.HCM",

            PhoneNumber = "0999012345",

            Email = "huong.bui@autox-gara.vn",

            Position = Position.PartsConsultant,

            Status = EmploymentStatus.Active,

            StartDate = new DateTime(2021, 9, 1, 0, 0, 0, DateTimeKind.Utc),

            EndDate = null,

        },



        // --- Nh?n vi?n ti?p nh?n xe (Ch? b?t d?u) ---

        new()

        {

            Name = "Vu Minh Khoa",

            Gender = Gender.Male,

            DateOfBirth = new DateTime(1998, 10, 25, 0, 0, 0, DateTimeKind.Utc),

            Address = "963 Ng? Van Nam, Q.G? V?p, TP.HCM",

            PhoneNumber = "0910111213",

            Email = "khoa.vu@autox-gara.vn",

            Position = Position.Receptionist,

            Status = EmploymentStatus.Pending,

            StartDate = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),

            EndDate = null,

        },



        // --- Nh?n vi?n r?a xe ---

        new()

        {

            Name = "Tr?n C?ng Son",

            Gender = Gender.Male,

            DateOfBirth = new DateTime(2000, 1, 30, 0, 0, 0, DateTimeKind.Utc),

            Address = "258 Nguy?n Oanh, Q.G? V?p, TP.HCM",

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
    // EMPLOYEE SALARY ? m?i nh?n vi?n ?t nh?t m?t m?c luong m?u
    // =========================================================================

    private static async System.Threading.Tasks.Task SeedEmployeeSalariesAsync(AutoXDbContext context)
    {
        if (await context.EmployeeSalaries.AnyAsync())
        {
            return;
        }

        var employees = await context.Employees
            .Where(e => e.Status == EmploymentStatus.Active)
            .OrderBy(e => e.Id)
            .Take(4)
            .ToListAsync();

        if (employees.Count == 0)
        {
            return;
        }

        var salaries = new List<EmployeeSalary>
        {
            new()
            {
                EmployeeId = employees[0].Id,
                Salary = 13_000_000m,
                SalaryType = SalaryType.Monthly,
                SalaryUnit = 1,
                EffectiveFrom = DateTime.UtcNow.AddMonths(-6),
                Note = "C? v?n c?p cao, luong ?n d?nh."
            },
            new()
            {
                EmployeeId = employees[1].Id,
                Salary = 120_000m,
                SalaryType = SalaryType.Daily,
                SalaryUnit = 22,
                EffectiveFrom = DateTime.UtcNow.AddMonths(-3),
                Note = "K? thu?t vi?n thay th? m?i ng?y."
            },
            new()
            {
                EmployeeId = employees[2].Id,
                Salary = 80_000m,
                SalaryType = SalaryType.Hourly,
                SalaryUnit = 8,
                EffectiveFrom = DateTime.UtcNow.AddMonths(-1),
                EffectiveTo = DateTime.UtcNow.AddMonths(2),
                Note = "Th?c t?p sinh b?o du?ng."
            },
            new()
            {
                EmployeeId = employees.Count > 3 ? employees[3].Id : employees[0].Id,
                Salary = 15_000_000m,
                SalaryType = SalaryType.Monthly,
                SalaryUnit = 1,
                EffectiveFrom = DateTime.UtcNow.AddYears(-1),
                Note = "Tru?ng ca k? thu?t."
            },
        };

        await context.EmployeeSalaries.AddRangeAsync(salaries);
        await context.SaveChangesAsync();
    }


    // =========================================================================

    // VEHICLE ? nhi?u xe

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

            ?? throw new InvalidOperationException($"Customer '{name}' kh?ng t?n t?i.");



        var vehicles = new List<Vehicle>

        {

            // Nguy?n Van An

            new()

            {

                CustomerId          = IdOf("Nguy?n Van An"),

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

            // Tr?n Th? B?nh

            new()

            {

                CustomerId          = IdOf("Tr?n Th? B?nh"),

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

            // L? Ho?ng Ph?c ? 2 xe BMW

            new()

            {

                CustomerId          = IdOf("L? Ho?ng Ph?c"),

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

                CustomerId          = IdOf("L? Ho?ng Ph?c"),

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

            // Ph?m Th? Kim Chi

            new()

            {

                CustomerId          = IdOf("Ph?m Th? Kim Chi"),

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

            // ??ng Qu?c H?ng ? d?i 3 xe

            new()

            {

                CustomerId          = IdOf("??ng Qu?c H?ng"),

                LicensePlate        = "51E-00001",

                Brand               = CarBrand.Ford,

                Model               = "Transit 16 ch?",

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

                CustomerId          = IdOf("??ng Qu?c H?ng"),

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

                CustomerId          = IdOf("??ng Qu?c H?ng"),

                LicensePlate        = "51E-00003",

                Brand               = CarBrand.Hyundai,

                Model               = "Solati 16 ch?",

                Type                = CarType.Minivan,

                Color               = CarColor.White,

                Year                = 2022,

                FrameNumber         = "KMHBU81TENU000003",

                EngineNumber        = "D4CB000003",

                Mileage             = 55_000,

                RegistrationDate    = new DateTime(2022, 11, 1, 0, 0, 0, DateTimeKind.Utc),

                InsuranceExpiryDate = new DateTime(2027, 11, 1, 0, 0, 0, DateTimeKind.Utc),

            },

            // C?ng ty V?n T?i Ph? Th?nh

            new()

            {

                CustomerId          = IdOf("C?ng ty TNHH V?n T?i Ph? Th?nh"),

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

            // Grab Vi?t Nam

            new()

            {

                CustomerId          = IdOf("C?ng ty CP Grab Vi?t Nam"),

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

                CustomerId          = IdOf("Nguy?n Van Xe Tang"),

                LicensePlate        = "QD-54321",

                Brand               = CarBrand.Other,

                Model               = "T-54 MBT (Xe Tang Chi?n ??u Ch? L?c)",

                Type                = CarType.Other,

                Color               = CarColor.Green,

                Year                = 1954,

                FrameNumber         = "T54QUANTM0000001",

                EngineNumber        = "V54500HP000001",

                Mileage             = 999_999,

                RegistrationDate    = new DateTime(1975, 4, 30, 0, 0, 0, DateTimeKind.Utc),

                InsuranceExpiryDate = null, // Ai d?m d?m v?o xe tang m? c?n b?o hi?m

            },

            new()

            {

                CustomerId          = IdOf("T? Ph? ElonUsk"),

                LicensePlate        = "SX-00001",

                Brand               = CarBrand.Tesla,

                Model               = "Roadster Starman Edition ? dang bay quanh M?t Tr?i",

                Type                = CarType.Coupe,

                Color               = CarColor.Red,

                Year                = 2018,

                FrameNumber         = "5YJ3E1EAXJF000001",

                EngineNumber        = "ELECTRICMOTOR001",

                Mileage             = 999_999, // d? di được hon 1 t? km, capped t?i max

                RegistrationDate    = new DateTime(2018, 2, 6, 0, 0, 0, DateTimeKind.Utc),

                InsuranceExpiryDate = new DateTime(9999, 12, 31, 0, 0, 0, DateTimeKind.Utc),

            },

            new()

            {

                CustomerId          = IdOf("Marty McFly"),

                LicensePlate        = "OUTATIME",  // 8 k? t?, h?p l? v?i MaxLength(9)

                Brand               = CarBrand.Other,

                Model               = "DeLorean DMC-12 Time Machine",

                Type                = CarType.Coupe,

                Color               = CarColor.Silver,

                Year                = 1985,

                FrameNumber         = "KNEELBFORE0ZOD01",  // 17 k? t?

                EngineNumber        = "FLUXCAPACITOR088",  // 17 k? t?

                Mileage             = 88,

                RegistrationDate    = new DateTime(1985, 10, 26, 0, 0, 0, DateTimeKind.Utc),

                InsuranceExpiryDate = new DateTime(1885, 9, 5, 0, 0, 0, DateTimeKind.Utc), // h?t h?n t?... qu? kh?

            },

        };



        context.Vehicles.AddRange(vehicles);

        await context.SaveChangesAsync();

    }



    // =========================================================================

    // SUPPLIER ? 4 nh? cung c?p

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

                Name              = "C?ng ty CP Ph? T?ng Toyota Vi?t Nam",

                Email             = "contact@toyotaparts.vn",

                Address           = "Khu CN M? Phu?c, B?nh Duong",

                TaxCode           = "0300123456789",

                BankAccount       = "19033123456789",

                PaymentTerms      = PaymentTerms.Net30,

                Status            = SupplierStatus.Active,

                ContractStartDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                Notes             = "Nh? cung c?p ch?nh th?c ph? t?ng Toyota, uu ti?n d?t h?ng.",

                PhoneNumbers      =

                [

                    new SupplierContactPhone { PhoneNumber = "02713456789" },

                    new SupplierContactPhone { PhoneNumber = "0901111222" },

                ],

            },

            new()

            {

                Name              = "C?ng ty TNHH Ph? T?ng Honda Vi?t Nam",

                Email             = "sales@hondaparts.vn",

                Address           = "KCX T?n Thu?n, Q.7, TP.HCM",

                TaxCode           = "0300987654321",

                BankAccount       = "19039876543210",

                PaymentTerms      = PaymentTerms.Net15,

                Status            = SupplierStatus.Active,

                ContractStartDate = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc),

                Notes             = "Chuy?n cung c?p ph? t?ng Honda ch?nh h?ng.",

                PhoneNumbers      =

                [

                    new SupplierContactPhone { PhoneNumber = "02838888999" },

                    new SupplierContactPhone { PhoneNumber = "0912222333" },

                ],

            },

            new()

            {

                Name              = "C?ng ty CP Bosch Vi?t Nam",

                Email             = "info@bosch.vn",

                Address           = "T?a nh? Bitexco Financial Tower, Q.1, TP.HCM",

                TaxCode           = "0300111223344",

                BankAccount       = "00101234567890",

                PaymentTerms      = PaymentTerms.Net30,

                Status            = SupplierStatus.Active,

                ContractStartDate = new DateTime(2022, 6, 1, 0, 0, 0, DateTimeKind.Utc),

                Notes             = "Cung c?p bugi, c?m bi?n, linh ki?n di?n t? da h?ng.",

                PhoneNumbers      =

                [

                    new SupplierContactPhone { PhoneNumber = "02839999000" },

                ],

            },

            new()

            {

                Name              = "T?p ?o?n Stark Industries VN",

                Email             = "tony@starkindustries-vn.com",

                Address           = "99 Nguy?n Hu?, Q.1, TP.HCM (VP d?i di?n)",

                TaxCode           = "9999999999999",

                BankAccount       = "99999999999999",

                PaymentTerms      = PaymentTerms.DueOnReceipt,

                Status            = SupplierStatus.Active,

                ContractStartDate = new DateTime(2008, 5, 2, 0, 0, 0, DateTimeKind.Utc),

                Notes             = "Chuy?n cung c?p: d?ng co t?n l?a thu nh?, radar ph?t hi?n d?ch, " +

                                    "arc reactor, b? gi?p Iron Man Mk.85. " +

                                    "LUU ?: KH?NG b?n Infinity Gauntlet ? d? ki?m tra, h?ng b? h?ng sau snap.",

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

    // SPARE PART ? 17 ph? t?ng

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

            ?? throw new InvalidOperationException($"Supplier c? t? kh?a '{keyword}' kh?ng t?n t?i.");



        Int32 toyotaId = IdOf("Toyota");

        Int32 hondaId = IdOf("Honda");

        Int32 boschId = IdOf("Bosch");

        Int32 starkId = IdOf("Stark");



        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var parts = new List<Part>();



        #region Spare Parts - Toyota (Ph? t?ng b?n)



        parts.AddRange(

        [

            // --- Toyota Spare Parts ---

            new()

            {

                SupplierId = toyotaId,

                PartCode = "SP_TOYOTA_001",

                PartName = "Gi?m x?c tru?c Toyota Fortuner (KYB OEM)",

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

                PartName = "Thanh c?n b?ng Toyota Camry 2.5",

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

                PartName = "K?t di?u h?a Toyota Innova 2.0",

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

                PartName = "L?c d?u Toyota Camry",

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

                PartName = "M? phanh tru?c Toyota Camry",

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

                PartName = "B?nh ?c quy Toyota Vios 45Ah",

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

                PartName = "D?y curoa cam Toyota Innova 2.0",

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

                PartName = "Bom nu?c Toyota Fortuner 2.7",

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



        #region Spare Parts - Honda (Ph? t?ng b?n)



        parts.AddRange(

        [

            // --- Honda Spare Parts ---

            new()

            {

                SupplierId = hondaId,

                PartCode = "SP_HONDA_001",

                PartName = "??n pha LED Honda CR-V 2023 (tr?i)",

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

                PartName = "K?nh ch?n gi? Honda City (ch?ng UV)",

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

                PartName = "L?p xe Honda HR-V 215/60R16",

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

                PartName = "D?u d?ng co Honda Ultra 5W-30 (4L)",

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

                PartName = "L?c gi? Honda City",

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

                PartName = "Cao su g?t mua Honda HR-V (c?p)",

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

                PartName = "B? piston v? x?c mang Honda Civic 1.8",

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



        #region Spare Parts - Bosch (Ph? t?ng b?n)



        parts.AddRange(

        [

            // --- Bosch Spare Parts ---

            new()

            {

                SupplierId = boschId,

                PartCode = "SP_BOSCH_001",

                PartName = "C?m bi?n ABS Bosch b?nh tru?c (universal)",

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

                PartName = "M?y ph?t di?n Bosch 14V 90A",

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

                PartName = "Kim phun nhi?n li?u Bosch (universal)",

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

                PartName = "C?m bi?n oxy Bosch O2 Sensor",

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

                PartName = "Bugi Bosch Iridium da h?ng",

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



        #region Spare Parts - Stark Industries (Ph? t?ng b?n - Fantasy)



        parts.AddRange(

        [

            // --- Stark Industries Fantasy Spare Parts ---

            new()

            {

                SupplierId = starkId,

                PartCode = "SP_STARK_001",

                PartName = "HUD Holographic Stark ? Hi?n Th? 3D To?n C?nh 360?",

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

                PartName = "C?nh Gi? ?i?u Khi?n T? ??ng Stark Aerodynamics Kit",

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

                PartName = "??ng Co T?n L?a Thu Nh? Stark Mk.1",

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

                PartName = "Radar Ph?t Hi?n ??ch Stark (Car Edition)",

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

                PartName = "Arc Reactor Mini (Thay B?nh ?c Quy Th?ng Thu?ng)",

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

                PartName = "Flux Capacitor (H?ng Ch?nh H?ng 1885 ? D?nh Cho DeLorean)",

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



        #region Replacement Parts - Generic (Ph? t?ng thay th?)



        parts.AddRange(

        [

            // --- Generic Replacement Parts (Th?c t?) ---

            new()

            {

                SupplierId = boschId,

                PartCode = "RP001",

                PartName = "L?c gi? d?ng co da nang",

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

                PartName = "D?y curoa d?n d?ng",

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

                PartName = "M? phanh sau da h?ng",

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

                PartName = "B?nh ch?a d?u phanh DOT4 500ml",

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

                PartName = "Nu?c l?m m?t Ready-to-Use 1L",

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

                PartName = "B? gioang n?p m?y da d?ng",

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

                PartName = "Cao su ch?n b?n b? 4 b?nh",

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

                PartName = "Bugi NGK Iridium IX h?p 4 c?i",

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

                PartName = "L?c nhi?n li?u da h?ng",

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

                PartName = "D?u d?ng co t?ng h?p 5W-30 4L",

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

                PartName = "?c quy kh? 12V 45Ah",

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

                PartName = "L?c nh?t da h?ng",

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

                PartName = "G?t mua m?m 24 inch",

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

                PartName = "Gi?m x?c tru?c thay th?",

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

                PartName = "B? l?c cabin kh? m?i",

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



        #region Replacement Parts - Fantasy (Ph? t?ng thay th? - Fantasy)



        parts.AddRange(

        [

            // --- Fantasy Replacement Parts ---

            new()

            {

                SupplierId = starkId,

                PartCode = "RP010",

                PartName = "B? Gi?p Iron Man Mk.85 (Thay C?n Tru?c Xe)",

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

                PartName = "??u ??n T?n L?a RPG Thay Th? C?n Sau Xe Tang T-54",

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

                PartName = "Xang Hypersonic Pha L? Nhi?n Li?u Cho ??ng Co T?n L?a",

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

                PartName = "L?p Xe Graphene Si?u D?n Nhi?t Ch?ng N? C?p Qu?n S?",

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

                PartName = "B? Pin Hydrogen Fuel Cell 150kW Thay Th? ??ng Co ??t Trong",

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

                PartName = "C?m Bi?n LiDAR 128-Layer T? L?i C?p ?? 5",

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

    // SERVICE ITEM ? 15+ d?ch v?

    // =========================================================================



    private static async System.Threading.Tasks.Task SeedServiceItemsAsync(AutoXDbContext context)

    {

        if (await context.ServiceItems.AnyAsync())

        {

            return;

        }



        var serviceItems = new List<ServiceItem>

    {

        // --- B?o du?ng d?nh k? ---

        new()

        {

            Description = "B?o du?ng d?nh k? 10,000km (Thay d?u, l?c d?u, ki?m tra chung)",

            Type = ServiceType.Maintenance,

            UnitPrice = 450_000,

        },

        new()

        {

            Description = "B?o du?ng d?nh k? 20,000km (Ki?m tra to?n b?, b?o du?ng hệ thống)",

            Type = ServiceType.Maintenance,

            UnitPrice = 850_000,

        },

        new()

        {

            Description = "B?o du?ng d?nh k? 40,000km (Ki?m tra s?u, thay th? linh ki?n)",

            Type = ServiceType.Maintenance,

            UnitPrice = 1_200_000,

        },

        new()

        {

            Description = "Ki?m tra xe d?nh k? (Ki?m tra an to?n, hi?u su?t)",

            Type = ServiceType.Inspection,

            UnitPrice = 300_000,

        },



        // --- Thay d?u & b? l?c ---

        new()

        {

            Description = "Thay d?u d?ng co + l?c d?u + l?c gi?",

            Type = ServiceType.OilChange,

            UnitPrice = 400_000,

        },

        new()

        {

            Description = "Thay l?c cabin + kh? m?i",

            Type = ServiceType.OilChange,

            UnitPrice = 250_000,

        },

        new()

        {

            Description = "Thay l?c nhi?n li?u",

            Type = ServiceType.OilChange,

            UnitPrice = 180_000,

        },



        // --- D?ch v? l?p xe ---

        new()

        {

            Description = "Thay l?p xe (1 c?i)",

            Type = ServiceType.TireService,

            UnitPrice = 200_000,

        },

        new()

        {

            Description = "V? l?p xe",

            Type = ServiceType.TireService,

            UnitPrice = 50_000,

        },

        new()

        {

            Description = "C?n b?ng l?p xe (b? 4 c?i)",

            Type = ServiceType.TireService,

            UnitPrice = 300_000,

        },

        new()

        {

            Description = "C?n ch?nh g?c d?t b?nh xe (Alignment)",

            Type = ServiceType.WheelAlignment,

            UnitPrice = 350_000,

        },



        // --- D?ch v? di?u h?a ---

        new()

        {

            Description = "B?o du?ng di?u h?a (V? sinh, kh? m?i)",

            Type = ServiceType.ACService,

            UnitPrice = 400_000,

        },

        new()

        {

            Description = "N?p gas di?u h?a (R134a)",

            Type = ServiceType.ACService,

            UnitPrice = 300_000,

        },

        new()

        {

            Description = "Thay d?u di?u h?a",

            Type = ServiceType.ACService,

            UnitPrice = 250_000,

        },



        // --- S?a ch?a chung ---

        new()

        {

            Description = "S?a ch?a d?ng co (theo y?u c?u)",

            Type = ServiceType.EngineRepair,

            UnitPrice = 1_500_000,

        },

        new()

        {

            Description = "S?a ch?a h?p s? t? d?ng",

            Type = ServiceType.TransmissionRepair,

            UnitPrice = 2_000_000,

        },

        new()

        {

            Description = "S?a ch?a hệ thống phanh (Ki?m tra & b?o du?ng)",

            Type = ServiceType.BrakeRepair,

            UnitPrice = 600_000,

        },

        new()

        {

            Description = "S?a ch?a hệ thống l?i & treo",

            Type = ServiceType.SuspensionRepair,

            UnitPrice = 800_000,

        },

        new()

        {

            Description = "S?a ch?a hệ thống nhi?n li?u",

            Type = ServiceType.FuelSystemRepair,

            UnitPrice = 700_000,

        },

        new()

        {

            Description = "S?a ch?a hệ thống di?n (?i?u tra l?i)",

            Type = ServiceType.ElectricalService,

            UnitPrice = 400_000,

        },

        new()

        {

            Description = "Thay b?nh ?c quy",

            Type = ServiceType.ElectricalService,

            UnitPrice = 500_000,

        },

        new()

        {

            Description = "S?a ch?a hệ thống d?nh l?a",

            Type = ServiceType.IgnitionRepair,

            UnitPrice = 300_000,

        },



        // --- L?m d?p ---

        new()

        {

            Description = "R?a xe b?n tay",

            Type = ServiceType.CarWashAndDetailing,

            UnitPrice = 150_000,

        },

        new()

        {

            Description = "Cham s?c n?i th?t (V? sinh, kh? m?i, b?o du?ng)",

            Type = ServiceType.CarWashAndDetailing,

            UnitPrice = 300_000,

        },

        new()

        {

            Description = "??nh b?ng & son ph?c h?i",

            Type = ServiceType.Painting,

            UnitPrice = 1_000_000,

        },

        new()

        {

            Description = "Ph?c h?i d?n pha (Polishing & coating)",

            Type = ServiceType.HeadlightRestoration,

            UnitPrice = 400_000,

        },

        new()

        {

            Description = "D?n phim c?ch nhi?t to?n k?nh",

            Type = ServiceType.WindowTintingAndPPF,

            UnitPrice = 2_000_000,

        },

        new()

        {

            Description = "Ph? son b?o v? (Paint Protection Film)",

            Type = ServiceType.WindowTintingAndPPF,

            UnitPrice = 3_000_000,

        },

        new()

        {

            Description = "Ph? Ceramic coating 9H",

            Type = ServiceType.CeramicCoating,

            UnitPrice = 2_500_000,

        },



        // --- An to?n & ki?m d?nh ---

        new()

        {

            Description = "D?ch v? ki?m d?nh xe (?ang ki?m)",

            Type = ServiceType.VehicleInspection,

            UnitPrice = 500_000,

        },

        new()

        {

            Description = "L?p d?t camera h?nh tr?nh",

            Type = ServiceType.DashcamInstallation,

            UnitPrice = 800_000,

        },

        new()

        {

            Description = "L?p d?t c?m bi?n d? xe (4 c?m bi?n)",

            Type = ServiceType.ParkingSensorAndADAS,

            UnitPrice = 600_000,

        },

        new()

        {

            Description = "L?p d?t hệ thống h? tr? l?i (ADAS)",

            Type = ServiceType.ParkingSensorAndADAS,

            UnitPrice = 1_500_000,

        },



        // --- D?ch v? kh?n c?p ---

        new()

        {

            Description = "D?ch v? c?u h? xe kh?n c?p (K?o xe + ch?n do?n)",

            Type = ServiceType.EmergencyRoadsideAssistance,

            UnitPrice = 2_000_000,

        },

        new()

        {

            Description = "D?ch v? k?o xe d?n gara",

            Type = ServiceType.TowingService,

            UnitPrice = 1_500_000,

        },

        new()

        {

            Description = "H? tr? kh?i d?ng xe (Nh?y b?nh)",

            Type = ServiceType.JumpStartService,

            UnitPrice = 200_000,

        },

        new()

        {

            Description = "H? tr? m? kh?a xe",

            Type = ServiceType.LockoutAssistance,

            UnitPrice = 150_000,

        },

        new()

        {

            Description = "Cung c?p nhi?n li?u kh?n c?p",

            Type = ServiceType.EmergencyFuelDelivery,

            UnitPrice = 100_000,

        },

    };



        context.ServiceItems.AddRange(serviceItems);

        await context.SaveChangesAsync();

    }

}
