// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Customers;
using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Infrastructure.Abstractions;
using AutoX.Gara.Shared.Packets.Vehicles;
using Nalix.Common.Networking.Abstractions;
using Nalix.Common.Networking.Packets.Abstractions;
using Nalix.Common.Networking.Packets.Attributes;
using Nalix.Common.Networking.Protocols;
using Nalix.Common.Security.Enums;
using Nalix.Network.Connections;
using Nalix.Shared.Serialization;

namespace AutoX.Gara.Application.Customers;

/// <summary>
/// Packet controller xử lý các nghiệp vụ liên quan đến Vehicle.
/// GET (lấy detail), UPDATE, DELETE (xóa mềm).
/// </summary>
[PacketController]
public sealed class VehicleOps(IVehicleRepository vehicles)
{
    private readonly IVehicleRepository _vehicles = vehicles ?? throw new System.ArgumentNullException(nameof(vehicles));

    // ─── GET BY ID ───────────────────────────────────────────────────────────

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.VEHICLE_GET)]
    public async System.Threading.Tasks.Task GetAsync(
        IPacket p,
        IConnection connection)
    {
        if (p is not VehicleDataPacket packet || packet.VehicleId is null)
        {
            System.UInt32 fallbackSeq = p is IPacketSequenced ps ? ps.SequenceId : 0;
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, fallbackSeq).ConfigureAwait(false);
            return;
        }

        try
        {
            Vehicle vehicle = await _vehicles.GetByIdAsync(packet.VehicleId.Value).ConfigureAwait(false);
            if (vehicle is null || vehicle.DeletedAt != null)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.NOT_FOUND,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
                return;
            }

            var response = MapToPacket(vehicle, packet.SequenceId);
            System.Boolean sent = await connection.TCP.SendAsync(LiteSerializer.Serialize(response)).ConfigureAwait(false);

            if (!sent)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.INTERNAL_ERROR,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
                return;
            }
        }
        catch (System.Exception)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.RETRY, packet.SequenceId).ConfigureAwait(false);
        }
    }

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((System.UInt16)OpCommand.VEHICLE_CREATE)]
    public async System.Threading.Tasks.Task CreateAsync(
        IPacket p,
        IConnection connection)
    {
        if (p is not VehicleDataPacket packet)
        {
            System.UInt32 fallbackSeq = p is IPacketSequenced ps ? ps.SequenceId : 0;
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, fallbackSeq).ConfigureAwait(false);
            return;
        }

        // Simple check: Biển số không được rỗng
        if (System.String.IsNullOrWhiteSpace(packet.LicensePlate))
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);
            return;
        }

        // Kiểm tra trùng biển số hoặc số khung/máy nếu cần
        System.Boolean existed = await _vehicles.ExistsAsync(
            packet.LicensePlate,
            packet.EngineNumber,
            packet.FrameNumber
        ).ConfigureAwait(false);
        if (existed)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.ALREADY_EXISTS,
                ProtocolAdvice.FIX_AND_RETRY, packet.SequenceId).ConfigureAwait(false);
            return;
        }

        try
        {
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
            await _vehicles.AddAsync(vehicle).ConfigureAwait(false);
            await _vehicles.SaveChangesAsync().ConfigureAwait(false);

            var confirmed = MapToPacket(vehicle, packet.SequenceId);
            System.Boolean sent = await connection.TCP.SendAsync(LiteSerializer.Serialize(confirmed)).ConfigureAwait(false);

            if (!sent)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.INTERNAL_ERROR,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
                return;
            }
        }
        catch (System.Exception)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
        }
    }

    // ─── UPDATE ──────────────────────────────────────────────────────────────

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode(0x4302)] // Cập nhật lại theo hệ thống
    public async System.Threading.Tasks.Task UpdateAsync(
        IPacket p,
        IConnection connection)
    {
        if (p is not VehicleDataPacket packet || packet.VehicleId is null)
        {
            System.UInt32 fallbackSeq = p is IPacketSequenced ps ? ps.SequenceId : 0;
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, fallbackSeq).ConfigureAwait(false);
            return;
        }

        try
        {
            Vehicle existing = await _vehicles.GetByIdAsync(packet.VehicleId.Value).ConfigureAwait(false);
            if (existing is null || existing.DeletedAt != null)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.NOT_FOUND,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
                return;
            }

            // Update fields
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

            _vehicles.Update(existing);
            await _vehicles.SaveChangesAsync().ConfigureAwait(false);

            var confirmed = MapToPacket(existing, packet.SequenceId);
            System.Boolean sent = await connection.TCP.SendAsync(LiteSerializer.Serialize(confirmed)).ConfigureAwait(false);
            if (!sent)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.INTERNAL_ERROR,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
                return;
            }
        }
        catch (System.Exception)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
        }
    }

    // ─── DELETE (SOFT) ───────────────────────────────────────────────────────

    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.SUPERVISOR)]
    [PacketOpcode((System.UInt16)OpCommand.VEHICLE_DELETE)]
    public async System.Threading.Tasks.Task DeleteAsync(
        IPacket p,
        IConnection connection)
    {
        if (p is not VehicleDataPacket packet || packet.VehicleId is null)
        {
            System.UInt32 fallbackSeq = p is IPacketSequenced ps0 ? ps0.SequenceId : 0;
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.MALFORMED_PACKET,
                ProtocolAdvice.DO_NOT_RETRY, fallbackSeq).ConfigureAwait(false);
            return;
        }

        try
        {
            Vehicle existing = await _vehicles.GetByIdAsync(packet.VehicleId.Value).ConfigureAwait(false);

            if (existing is null || existing.DeletedAt != null)
            {
                await connection.SendAsync(
                    ControlType.ERROR,
                    ProtocolReason.NOT_FOUND,
                    ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
                return;
            }

            var now = System.DateTime.UtcNow;
            existing.DeletedAt = now;

            _vehicles.Update(existing);
            await _vehicles.SaveChangesAsync().ConfigureAwait(false);

            await connection.SendAsync(
                ControlType.NONE,
                ProtocolReason.NONE,
                ProtocolAdvice.NONE, packet.SequenceId).ConfigureAwait(false);
        }
        catch (System.Exception)
        {
            await connection.SendAsync(
                ControlType.ERROR,
                ProtocolReason.INTERNAL_ERROR,
                ProtocolAdvice.DO_NOT_RETRY, packet.SequenceId).ConfigureAwait(false);
        }
    }

    // ─── Private Helpers ─────────────────────────────────────────────────────

    private static VehicleDataPacket MapToPacket(Vehicle v, System.UInt32 sequenceId) => new()
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