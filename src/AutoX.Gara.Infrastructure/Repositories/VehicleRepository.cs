// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Customers;
using AutoX.Gara.Infrastructure.Abstractions;
using AutoX.Gara.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AutoX.Gara.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation của <see cref="IVehicleRepository"/>.
/// Chỉ nơi này mới được thao tác DbContext trực tiếp cho Vehicle.
/// </summary>
public sealed class VehicleRepository(AutoXDbContext context) : IVehicleRepository
{
    private readonly AutoXDbContext _context = context ?? throw new System.ArgumentNullException(nameof(context));

    /// <inheritdoc/>
    public Task<Vehicle> GetByIdAsync(System.Int32 id, CancellationToken ct = default)
        => _context.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id && v.DeletedAt == null, ct);

    /// <inheritdoc/>
    public Task<System.Boolean> ExistsAsync(System.String licensePlate, System.String engineNumber = null, System.String frameNumber = null, CancellationToken ct = default)
    {
        IQueryable<Vehicle> q = _context.Vehicles.Where(v => v.DeletedAt == null);

        if (!System.String.IsNullOrWhiteSpace(licensePlate))
        {
            q = q.Where(v => v.LicensePlate == licensePlate);
        }

        if (!System.String.IsNullOrWhiteSpace(engineNumber))
        {
            q = q.Where(v => v.EngineNumber == engineNumber);
        }

        if (!System.String.IsNullOrWhiteSpace(frameNumber))
        {
            q = q.Where(v => v.FrameNumber == frameNumber);
        }

        return q.AnyAsync(ct);
    }

    /// <inheritdoc/>
    public Task AddAsync(Vehicle vehicle, CancellationToken ct = default) => _context.Vehicles.AddAsync(vehicle, ct).AsTask();

    /// <inheritdoc/>
    public void Update(Vehicle vehicle) => _context.Vehicles.Update(vehicle);

    /// <inheritdoc/>
    public Task SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}