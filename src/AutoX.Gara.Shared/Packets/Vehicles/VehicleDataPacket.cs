// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Cars;
using Nalix.Common.Networking.Packets.Abstractions;
using Nalix.Common.Networking.Packets.Enums;
using Nalix.Common.Security.Attributes;
using Nalix.Common.Security.Enums;
using Nalix.Common.Serialization;
using Nalix.Common.Serialization.Attributes;
using Nalix.Shared.Extensions;
using Nalix.Shared.Frames;

namespace AutoX.Gara.Shared.Packets.Vehicles;

/// <summary>
/// Packet truyền dữ liệu xe cho các thao tác tạo, cập nhật, truy vấn.
/// Sử dụng PacketBase để auto serialize/pooling.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class VehicleDataPacket : PacketBase<VehicleDataPacket>, IPacketTransformer<VehicleDataPacket>, IPacketSequenced
{
    // ─── Fixed-size fields ───────────────────────────────────────────────

    [SerializeOrder(PacketHeaderOffset.DATA_REGION)]
    public System.UInt32 SequenceId { get; set; }

    /// <summary>Id của xe. null khi tạo mới.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 1)]
    public System.Int32? VehicleId { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 2)]
    public System.Int32 CustomerId { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 3)]
    public CarType Type { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 4)]
    public CarColor Color { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 5)]
    public CarBrand Brand { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 6)]
    public System.Int32 Year { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 7)]
    public System.Double Mileage { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 8)]
    public System.DateTime RegistrationDate { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 9)]
    public System.DateTime? InsuranceExpiryDate { get; set; }

    // ─── Dynamic fields (string) ─────────────────────────────────────────
    // Theo Pattern, phải đứng sau fixed size field

    [SensitiveData(DataSensitivityLevel.Internal)]
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 10)]
    public System.String Model { get; set; }

    [SensitiveData(DataSensitivityLevel.Internal)]
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 11)]
    public System.String LicensePlate { get; set; }

    [SensitiveData(DataSensitivityLevel.Internal)]
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 12)]
    public System.String EngineNumber { get; set; }

    [SensitiveData(DataSensitivityLevel.Internal)]
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 13)]
    public System.String FrameNumber { get; set; }

    // ─── Constructor ──────────��─────────────────────────────────────────

    public VehicleDataPacket()
    {
        Model = System.String.Empty;
        FrameNumber = System.String.Empty;
        LicensePlate = System.String.Empty;
        EngineNumber = System.String.Empty;
        Year = 1900;
        Mileage = 0;
        Brand = CarBrand.None;
        Color = CarColor.None;
        Type = CarType.None;
        RegistrationDate = System.DateTime.UtcNow;
        OpCode = 0;
    }

    /// <inheritdoc/>
    public override void ResetForPool()
    {
        base.ResetForPool();

        SequenceId = 0;
        VehicleId = null;
        CustomerId = 0;
        Type = CarType.None;
        Color = CarColor.None;
        Brand = CarBrand.None;
        Year = 1900;
        Mileage = 0;
        RegistrationDate = System.DateTime.UtcNow;
        InsuranceExpiryDate = null;
        Model = System.String.Empty;
        LicensePlate = System.String.Empty;
        EngineNumber = System.String.Empty;
        FrameNumber = System.String.Empty;
        OpCode = 0;
    }

    /// <summary>Compress string fields and mark packet as compressed.</summary>
    /// <exception cref="System.ArgumentNullException">Thrown when packet is null.</exception>
    public static VehicleDataPacket Compress(VehicleDataPacket packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);

        packet.Model = packet.Model.CompressToBase64();
        packet.FrameNumber = packet.FrameNumber.CompressToBase64();
        packet.LicensePlate = packet.LicensePlate.CompressToBase64();
        packet.EngineNumber = packet.EngineNumber.CompressToBase64();

        packet.Flags.AddFlag(PacketFlags.COMPRESSED);
        return packet;
    }

    /// <summary>Decompress string fields and remove compressed flag.</summary>
    /// <exception cref="System.ArgumentNullException">Thrown when packet is null.</exception>
    public static VehicleDataPacket Decompress(VehicleDataPacket packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);

        packet.Model = packet.Model.DecompressFromBase64();
        packet.FrameNumber = packet.FrameNumber.DecompressFromBase64();
        packet.LicensePlate = packet.LicensePlate.DecompressFromBase64();
        packet.EngineNumber = packet.EngineNumber.DecompressFromBase64();

        packet.Flags.RemoveFlag(PacketFlags.COMPRESSED);
        return packet;
    }
}