// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums.Cars;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;
using System;
namespace AutoX.Gara.Contracts.Protocol.Vehicles;
/// <summary>
/// Packet truy?n dữ liệu xe cho c�c thao t�c t?o, c?p nh?t, truy v?n.
/// S? d?ng PacketBase d? auto serialize/pooling.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class VehicleDto : PacketBase<VehicleDto>
{
    // --- Fixed-size fields -----------------------------------------------
    /// <summary>Id c?a xe. null khi t?o m?i.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 1)]
    public int? VehicleId { get; set; }
    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public int CustomerId { get; set; }
    [SerializeOrder(PacketHeaderOffset.Region + 3)]
    public CarType Type { get; set; }
    [SerializeOrder(PacketHeaderOffset.Region + 4)]
    public CarColor Color { get; set; }
    [SerializeOrder(PacketHeaderOffset.Region + 5)]
    public CarBrand Brand { get; set; }
    [SerializeOrder(PacketHeaderOffset.Region + 6)]
    public int Year { get; set; }
    [SerializeOrder(PacketHeaderOffset.Region + 7)]
    public double Mileage { get; set; }
    [SerializeOrder(PacketHeaderOffset.Region + 8)]
    public DateTime RegistrationDate { get; set; }
    [SerializeOrder(PacketHeaderOffset.Region + 9)]
    public DateTime? InsuranceExpiryDate { get; set; }
    // --- Dynamic fields (string) -----------------------------------------
    // Theo Pattern, ph?i d?ng sau fixed size field
    [SerializeOrder(PacketHeaderOffset.Region + 10)]
    public string Model { get; set; }
    [SerializeOrder(PacketHeaderOffset.Region + 11)]
    public string LicensePlate { get; set; }
    [SerializeOrder(PacketHeaderOffset.Region + 12)]
    public string EngineNumber { get; set; }
    [SerializeOrder(PacketHeaderOffset.Region + 13)]
    public string FrameNumber { get; set; }
    // --- Constructor ----------??-----------------------------------------
    public VehicleDto()
    {
        Model = string.Empty;
        FrameNumber = string.Empty;
        LicensePlate = string.Empty;
        EngineNumber = string.Empty;
        Year = 1900;
        Mileage = 0;
        Brand = CarBrand.None;
        Color = CarColor.None;
        Type = CarType.None;
        RegistrationDate = DateTime.UtcNow;
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
        RegistrationDate = DateTime.UtcNow;
        InsuranceExpiryDate = null;
        Model = string.Empty;
        LicensePlate = string.Empty;
        EngineNumber = string.Empty;
        FrameNumber = string.Empty;
        OpCode = 0;
    }
}

