// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Application.Abstractions.Persistence;
using AutoX.Gara.Domain.Entities.Customers;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Vehicles;
using Nalix.Common.Networking;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Networking.Protocols;
using Nalix.Common.Security;
using Nalix.Framework.Serialization;
using Nalix.Runtime.Extensions;

namespace AutoX.Gara.Application.Vehicles;

/// <summary>
/// Packet controller xử lý các nghiệp vụ liên quan đến Vehicle.
/// <list type="bullet">
///   <item>VEHICLE_GET  — VehicleId != null → lấy 1 xe; VehicleId == null → lấy danh sách theo CustomerId</item>
///   <item>VEHICLE_CREATE — tạo mới xe</item>
///   <item>VEHICLE_UPDATE — cập nhật xe (opcode 0x152)</item>
///   <item>VEHICLE_DELETE — xóa mềm (chỉ SUPERVISOR)</item>
/// </list>
/// </summary>
[PacketController]
public sealed class VehicleOps(IDataSessionFactory dataSessionFactory)
{
    private readonly IDataSessionFactory _dataSessionFactory = dataSessionFactory
        ?? throw new System.ArgumentNullException(nameof(dataSessionFactory));

    private const System.Int32 DefaultPageSize = 10;

    // ─── GET (single hoặc list theo CustomerId) ───────────────────────────────

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.VEHICLE_GET)]
    public async System.Threading.Tasks.Task GetAsync(
        IPacket p,
        IConnection connection)
    {
        if (p is not VehicleDto packet)
        {
            System.UInt32 fallbackSeq = p.SequenceId;
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, fallbackSeq, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        if (packet.VehicleId is not null)
        {
            await GetSingleAsync(packet, connection).ConfigureAwait(false);
            return;
        }

        if (packet.CustomerId <= 0)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.FIX_AND_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        await GetListByCustomerAsync(packet, connection).ConfigureAwait(false);
    }

    // ─── CREATE ───────────────────────────────────────────────────────────────

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.VEHICLE_CREATE)]
    public async System.Threading.Tasks.Task CreateAsync(
        IPacket p,
        IConnection connection)
    {
        if (p is not VehicleDto packet)
        {
            System.UInt32 fallbackSeq = p.SequenceId;
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, fallbackSeq, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        if (System.String.IsNullOrWhiteSpace(packet.LicensePlate))
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.FIX_AND_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        try
        {
            await using var session = _dataSessionFactory.Create();
            var vehicles = session.Vehicles;

            System.Boolean existed = await vehicles.ExistsAsync(
                packet.LicensePlate,
                packet.EngineNumber,
                packet.FrameNumber).ConfigureAwait(false);

            if (existed)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.ALREADY_EXISTS,
                    ProtocolAdvice.FIX_AND_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
                return;
            }

            var now = System.DateTime.UtcNow;
            var vehicle = new Vehicle
            {
                CustomerId = packet.CustomerId,
                Type = packet.Type,
                Color = packet.Color,
                Brand = packet.Brand,
                Year = packet.Year,
                Mileage = packet.Mileage,
                Model = packet.Model,
                LicensePlate = packet.LicensePlate,
                EngineNumber = packet.EngineNumber,
                FrameNumber = packet.FrameNumber,
                RegistrationDate = packet.RegistrationDate == default ? now : packet.RegistrationDate,
                InsuranceExpiryDate = packet.InsuranceExpiryDate,
                DeletedAt = null,
            };

            await vehicles.AddAsync(vehicle).ConfigureAwait(false);
            await vehicles.SaveChangesAsync().ConfigureAwait(false);

            var confirmed = MapToPacket(vehicle, packet.SequenceId);
            await connection.TCP.SendAsync(
                LiteSerializer.Serialize(confirmed)).ConfigureAwait(false);

        }
        catch (System.Exception)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
        }
    }

    // ─── UPDATE ───────────────────────────────────────────────────────────────

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.VEHICLE_UPDATE)]
    public async System.Threading.Tasks.Task UpdateAsync(
        IPacket p,
        IConnection connection)
    {
        if (p is not VehicleDto packet || packet.VehicleId is null)
        {
            System.UInt32 fallbackSeq = p.SequenceId;
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, fallbackSeq, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        try
        {
            await using var session = _dataSessionFactory.Create();
            var vehicles = session.Vehicles;

            Vehicle existing = await vehicles.GetByIdAsync(packet.VehicleId.Value).ConfigureAwait(false);

            if (existing is null || existing.DeletedAt != null)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.NOT_FOUND,
                    ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
                return;
            }

            existing.CustomerId = packet.CustomerId;
            existing.Type = packet.Type;
            existing.Color = packet.Color;
            existing.Brand = packet.Brand;
            existing.Year = packet.Year;
            existing.Mileage = packet.Mileage;
            existing.Model = packet.Model;
            existing.LicensePlate = packet.LicensePlate;
            existing.EngineNumber = packet.EngineNumber;
            existing.FrameNumber = packet.FrameNumber;
            existing.RegistrationDate = packet.RegistrationDate;
            existing.InsuranceExpiryDate = packet.InsuranceExpiryDate;

            vehicles.Update(existing);
            await vehicles.SaveChangesAsync().ConfigureAwait(false);

            var confirmed = MapToPacket(existing, packet.SequenceId);
            await connection.TCP.SendAsync(
                LiteSerializer.Serialize(confirmed)).ConfigureAwait(false);

        }
        catch (System.Exception)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
        }
    }

    // ─── DELETE (SOFT) ────────────────────────────────────────────────────────

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.SUPERVISOR)]
    [PacketOpcode((System.UInt16)OpCommand.VEHICLE_DELETE)]
    public async System.Threading.Tasks.Task DeleteAsync(
        IPacket p,
        IConnection connection)
    {
        if (p is not VehicleDto packet || packet.VehicleId is null)
        {
            System.UInt32 fallbackSeq = p.SequenceId;
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, fallbackSeq, 0u, 0u, 0)).ConfigureAwait(false);
            return;
        }

        try
        {
            await using var session = _dataSessionFactory.Create();
            var vehicles = session.Vehicles;

            Vehicle existing = await vehicles.GetByIdAsync(packet.VehicleId.Value).ConfigureAwait(false);

            if (existing is null || existing.DeletedAt != null)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.NOT_FOUND,
                    ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
                return;
            }

            existing.DeletedAt = System.DateTime.UtcNow;
            vehicles.Update(existing);
            await vehicles.SaveChangesAsync().ConfigureAwait(false);

            await connection.SendAsync(
                ControlType.NONE,
                ProtocolReason.NONE,
                ProtocolAdvice.NONE, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
        }
        catch (System.Exception)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
        }
    }

    // ─── Private: Get single ──────────────────────────────────────────────────

    private async System.Threading.Tasks.Task GetSingleAsync(
        VehicleDto packet,
        IConnection connection)
    {
        try
        {
            await using var session = _dataSessionFactory.Create();
            var vehicles = session.Vehicles;

            Vehicle vehicle = await vehicles.GetByIdAsync(
                packet.VehicleId!.Value).ConfigureAwait(false);

            if (vehicle is null || vehicle.DeletedAt != null)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.NOT_FOUND,
                    ProtocolAdvice.DO_NOT_RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
                return;
            }

            var response = MapToPacket(vehicle, packet.SequenceId);
            await connection.TCP.SendAsync(
                LiteSerializer.Serialize(response)).ConfigureAwait(false);

        }
        catch (System.Exception)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
        }
    }

    // ─── Private: Get list by CustomerId ─────────────────────────────────────

    private async System.Threading.Tasks.Task GetListByCustomerAsync(
        VehicleDto packet,
        IConnection connection)
    {
        try
        {
            await using var session = _dataSessionFactory.Create();
            var vehicles = session.Vehicles;

            System.Int32 page = packet.Year > 0 ? packet.Year : 1;

            (System.Collections.Generic.List<Vehicle> items, System.Int32 total) =
                await vehicles.GetByCustomerIdAsync(
                    packet.CustomerId,
                    page,
                    DefaultPageSize).ConfigureAwait(false);

            var response = new VehiclesQueryResponse
            {
                SequenceId = packet.SequenceId,
                TotalCount = total,
                Vehicles = []
            };

            foreach (Vehicle v in items)
            {
                response.Vehicles.Add(MapToPacket(v, 0));
            }

            await connection.TCP.SendAsync(
                LiteSerializer.Serialize(response)).ConfigureAwait(false);

        }
        catch (System.Exception)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.RETRY, new ControlDirectiveOptions(ControlFlags.NONE, packet.SequenceId, 0u, 0u, 0)).ConfigureAwait(false);
        }
    }

    // ─── Private Helpers ─────────────────────────────────────────────────────

    private static VehicleDto MapToPacket(Vehicle v, System.UInt32 sequenceId) => new()
    {
        SequenceId = sequenceId,
        VehicleId = v.Id,
        CustomerId = v.CustomerId,
        Type = v.Type,
        Color = v.Color,
        Brand = v.Brand,
        Year = v.Year,
        Model = v.Model,
        LicensePlate = v.LicensePlate,
        EngineNumber = v.EngineNumber,
        FrameNumber = v.FrameNumber,
        RegistrationDate = v.RegistrationDate,
        InsuranceExpiryDate = v.InsuranceExpiryDate,
        Mileage = v.Mileage
    };
}





