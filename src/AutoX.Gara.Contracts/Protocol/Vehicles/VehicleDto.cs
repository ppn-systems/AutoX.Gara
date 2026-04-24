// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums.Cars;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;
using System;
namespace AutoX.Gara.Contracts.Vehicles;
/// <summary>
/// Packet truy?n dữ liệu xe cho c�c thao t�c t?o, c?p nh?t, truy v?n.
/// S? d?ng PacketBase d? auto serialize/pooling.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class VehicleDto : PacketBase<VehicleDto>
{
    // --- Fixed-size fields -----------------------------------------------
    /// <summary>Id c?a xe. null khi t?o m?i.</summary>
    [SerializeOrder(0)]
    public int? VehicleId { get; set; }
    [SerializeOrder(1)]
    public int CustomerId { get; set; }
    [SerializeOrder(2)]
    public CarType Type { get; set; }
    [SerializeOrder(3)]
    public CarColor Color { get; set; }
    [SerializeOrder(4)]
    public CarBrand Brand { get; set; }
    [SerializeOrder(5)]
    public int Year { get; set; }
    [SerializeOrder(6)]
    public double Mileage { get; set; }
    [SerializeOrder(7)]
    public DateTime RegistrationDate { get; set; }
    [SerializeOrder(8)]
    public DateTime? InsuranceExpiryDate { get; set; }
    // --- Dynamic fields (string) -----------------------------------------
    // Theo Pattern, ph?i d?ng sau fixed size field
    [SerializeOrder(9)]
    public string Model { get; set; }
    [SerializeOrder(10)]
    public string LicensePlate { get; set; }
    [SerializeOrder(11)]
    public string EngineNumber { get; set; }
    [SerializeOrder(12)]
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



