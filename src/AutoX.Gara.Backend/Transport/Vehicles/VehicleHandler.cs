using AutoX.Gara.Application.Vehicles;
using AutoX.Gara.Backend.Transport.Common;
// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Entities.Customers;
using AutoX.Gara.Contracts.Enums;
using AutoX.Gara.Contracts.Vehicles;
using Nalix.Common.Networking;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Networking.Protocols;
using Nalix.Common.Security;
using Nalix.Framework.DataFrames.Pooling;
using System;
using System.Threading.Tasks;
namespace AutoX.Gara.Backend.Transport.Vehicles;
/// <summary>
/// Packet Handler for vehicle related operations.
/// </summary>
[PacketController]
public sealed class VehicleHandler(VehicleAppService vehicleService)
{
    private readonly VehicleAppService _vehicleService = vehicleService ?? throw new ArgumentNullException(nameof(vehicleService));
    private const int DefaultPageSize = 10;
    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((ushort)OpCommand.VEHICLE_GET)]
    public async ValueTask GetAsync(IPacketContext<VehicleDto> context)
    {
        VehicleDto packet = context.Packet;
        IConnection connection = context.Connection;
        if (packet.VehicleId != null)
        {
            var result = await _vehicleService.GetByIdAsync(packet.VehicleId.Value).ConfigureAwait(false);
            if (!result.IsSuccess)
            {
                await context.FailAsync(result.Reason).ConfigureAwait(false);
                return;
            }
            await connection.TCP.SendAsync(MapToPacket(result.Data!, packet.SequenceId)).ConfigureAwait(false);
        }
        else
        {
            if (packet.CustomerId <= 0)
            {
                await context.FailAsync(ProtocolReason.MALFORMED_PACKET).ConfigureAwait(false);
                return;
            }
            int page = packet.Year > 0 ? packet.Year : 1; // Hacky way to pass page if needed, or update DTO
            var result = await _vehicleService.GetByCustomerIdAsync(packet.CustomerId, page, DefaultPageSize).ConfigureAwait(false);
            if (!result.IsSuccess)
            {
                await context.FailAsync(result.Reason).ConfigureAwait(false);
                return;
            }
            using var lease = PacketPool<VehiclesQueryResponse>.Rent();
            var response = lease.Value;
            response.SequenceId = packet.SequenceId;
            response.TotalCount = result.Data!.totalCount;
            response.Vehicles = result.Data.items.ConvertAll(v => MapToPacket(v, 0));
            await connection.TCP.SendAsync(response).ConfigureAwait(false);
        }
    }
    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((ushort)OpCommand.VEHICLE_CREATE)]
    public async ValueTask CreateAsync(IPacketContext<VehicleDto> context)
    {
        VehicleDto packet = context.Packet;
        IConnection connection = context.Connection;
        if (string.IsNullOrWhiteSpace(packet.LicensePlate))
        {
            await context.FailAsync(ProtocolReason.MALFORMED_PACKET).ConfigureAwait(false);
            return;
        }
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
            RegistrationDate = packet.RegistrationDate == default ? DateTime.UtcNow : packet.RegistrationDate,
            InsuranceExpiryDate = packet.InsuranceExpiryDate
        };
        var result = await _vehicleService.CreateAsync(vehicle).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }
        await connection.TCP.SendAsync(MapToPacket(result.Data!, packet.SequenceId)).ConfigureAwait(false);
    }
    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.USER)]
    [PacketOpcode((ushort)OpCommand.VEHICLE_UPDATE)]
    public async ValueTask UpdateAsync(IPacketContext<VehicleDto> context)
    {
        VehicleDto packet = context.Packet;
        IConnection connection = context.Connection;
        if (packet.VehicleId == null)
        {
            await context.FailAsync(ProtocolReason.MALFORMED_PACKET).ConfigureAwait(false);
            return;
        }
        var vehicle = new Vehicle
        {
            Id = packet.VehicleId.Value,
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
            RegistrationDate = packet.RegistrationDate,
            InsuranceExpiryDate = packet.InsuranceExpiryDate
        };
        var result = await _vehicleService.UpdateAsync(vehicle).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }
        await connection.TCP.SendAsync(MapToPacket(result.Data!, packet.SequenceId)).ConfigureAwait(false);
    }
    [PacketEncryption(true)]
    [PacketPermission(PermissionLevel.SUPERVISOR)]
    [PacketOpcode((ushort)OpCommand.VEHICLE_DELETE)]
    public async ValueTask DeleteAsync(IPacketContext<VehicleDto> context)
    {
        VehicleDto packet = context.Packet;
        IConnection connection = context.Connection;
        if (packet.VehicleId == null)
        {
            await context.FailAsync(ProtocolReason.MALFORMED_PACKET).ConfigureAwait(false);
            return;
        }
        var result = await _vehicleService.DeleteAsync(packet.VehicleId.Value).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await context.FailAsync(result.Reason).ConfigureAwait(false);
            return;
        }
        await context.OkAsync().ConfigureAwait(false);
    }
    private static VehicleDto MapToPacket(Vehicle v, ushort sequenceId) => new()
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


