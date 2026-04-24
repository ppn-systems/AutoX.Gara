// Copyright (c) 2026 PPN Corporation. All rights reserved.



using AutoX.Gara.Application.Abstractions.Repositories;using AutoX.Gara.Domain.Entities.Customers;using AutoX.Gara.Infrastructure.Database;using Microsoft.EntityFrameworkCore;using System.Collections.Generic;using System.Linq;using System.Threading;using System.Threading.Tasks;



namespace AutoX.Gara.Infrastructure.Repositories;



/// <summary>

/// EF Core implementation c?a <see cref="IVehicleRepository"/>.

/// Ch? noi n�y m?i được thao t�c DbContext tr?c ti?p cho Vehicle.

/// </summary>

public sealed class VehicleRepository(AutoXDbContext context) : IVehicleRepository

{

    private readonly AutoXDbContext _context = context ?? throw new System.ArgumentNullException(nameof(context));



    /// <inheritdoc/>

    public Task<Vehicle> GetByIdAsync(int id, CancellationToken ct = default)

        => _context.Vehicles

            .AsNoTracking()

            .FirstOrDefaultAsync(v => v.Id == id && v.DeletedAt == null, ct);



    /// <inheritdoc/>

    public async Task<(List<Vehicle> Items, int TotalCount)> GetByCustomerIdAsync(

        int customerId,

        int page,

        int pageSize,

        CancellationToken ct = default)

    {

        IQueryable<Vehicle> query = _context.Vehicles

            .AsNoTracking()

            .Where(v => v.CustomerId == customerId && v.DeletedAt == null)

            .OrderByDescending(v => v.RegistrationDate);



        int total = await query.CountAsync(ct).ConfigureAwait(false);



        List<Vehicle> items = await query

            .Skip((page - 1) * pageSize)

            .Take(pageSize)

            .ToListAsync(ct)

            .ConfigureAwait(false);



        return (items, total);

    }



    /// <inheritdoc/>

    public Task<bool> ExistsAsync(

        string licensePlate,

        string engineNumber = null,

        string frameNumber = null,

        CancellationToken ct = default)

    {

        IQueryable<Vehicle> q = _context.Vehicles.Where(v => v.DeletedAt == null);



        if (!string.IsNullOrWhiteSpace(licensePlate))

        {

            q = q.Where(v => v.LicensePlate == licensePlate);

        }



        if (!string.IsNullOrWhiteSpace(engineNumber))

        {

            q = q.Where(v => v.EngineNumber == engineNumber);

        }



        if (!string.IsNullOrWhiteSpace(frameNumber))

        {

            q = q.Where(v => v.FrameNumber == frameNumber);

        }



        return q.AnyAsync(ct);

    }



    /// <inheritdoc/>

    public Task AddAsync(Vehicle vehicle, CancellationToken ct = default)

        => _context.Vehicles.AddAsync(vehicle, ct).AsTask();



    /// <inheritdoc/>

    public void Update(Vehicle vehicle) => _context.Vehicles.Update(vehicle);

    public void Delete(Vehicle vehicle) => _context.Vehicles.Remove(vehicle);



    /// <inheritdoc/>

    public Task SaveChangesAsync(CancellationToken ct = default)

        => _context.SaveChangesAsync(ct);

}

