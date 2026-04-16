// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Cars;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;

namespace AutoX.Gara.Shared.Protocol.Vehicles;

/// <summary>
/// Packet truy?n d? li?u xe cho các thao tác t?o, c?p nh?t, truy v?n.
/// S? d?ng PacketBase d? auto serialize/pooling.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class VehicleDto : PacketBase<VehicleDto>
{
    // --- Fixed-size fields -----------------------------------------------

    /// <summary>Id c?a xe. null khi t?o m?i.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 1)]
    public System.Int32? VehicleId { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public System.Int32 CustomerId { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 3)]
    public CarType Type { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 4)]
    public CarColor Color { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 5)]
    public CarBrand Brand { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 6)]
    public System.Int32 Year { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 7)]
    public System.Double Mileage { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 8)]
    public System.DateTime RegistrationDate { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 9)]
    public System.DateTime? InsuranceExpiryDate { get; set; }

    // --- Dynamic fields (string) -----------------------------------------
    // Theo Pattern, ph?i d?ng sau fixed size field

    [SerializeOrder(PacketHeaderOffset.Region + 10)]
    public System.String Model { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 11)]
    public System.String LicensePlate { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 12)]
    public System.String EngineNumber { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 13)]
    public System.String FrameNumber { get; set; }

    // --- Constructor ----------??-----------------------------------------

    public VehicleDto()
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
}