// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Repairs;
using AutoX.Gara.Domain.Enums.Cars;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoX.Gara.Domain.Entities.Customers;

/// <summary>
/// Lớp đại diện cho xe.
/// </summary>
[Table(nameof(Vehicle))]
public class Vehicle
{
    #region Fields

    private System.String _carLicensePlate = System.String.Empty;
    private System.String _engineNumber = System.String.Empty;
    private System.String _frameNumber = System.String.Empty;
    private System.String _carModel = System.String.Empty;

    #endregion

    #region Identification Properties

    /// <summary>
    /// Mã xe.
    /// </summary>
    [Key]
    public System.Int32 Id { get; set; }

    /// <summary>
    /// Id chủ xe.
    /// </summary>
    [Required]
    public System.Int32 CustomerId { get; set; }

    /// <summary>
    /// Thông tin chủ xe (Navigation Property).
    /// </summary>
    [ForeignKey(nameof(CustomerId))]
    public virtual Customer Customer { get; set; }

    #endregion

    #region Basic Information Properties

    /// <summary>
    /// Năm sản xuất.
    /// </summary>
    [Range(1900, 2100)]
    public System.Int32 Year { get; set; } = 1900;

    /// <summary>
    /// Loại xe (Sedan, SUV, Hatchback, ...).
    /// </summary>
    public CarType Type { get; set; } = CarType.None;

    /// <summary>
    /// Màu sắc.
    /// </summary>
    public CarColor Color { get; set; } = CarColor.None;

    /// <summary>
    /// Hãng xe.
    /// </summary>
    public CarBrand Brand { get; set; } = CarBrand.None;

    /// <summary>
    /// Model xe.
    /// </summary>
    [MaxLength(50, ErrorMessage = "Vehicle model must not exceed 50 characters.")]
    public System.String Model
    {
        get => _carModel;
        set => _carModel = value.Trim();
    }

    #endregion

    #region Registration Properties

    /// <summary>
    /// Biển số xe khách hàng.
    /// </summary>
    [Required(ErrorMessage = "Vehicle license plate is required.")]
    [MaxLength(9)]
    [RegularExpression(@"^[0-9]{2}[A-Z]{1,2}-[0-9]{3,5}$", ErrorMessage = "Invalid license plate format.")]
    public System.String LicensePlate
    {
        get => _carLicensePlate;
        set => _carLicensePlate = value?.Trim().ToUpper() ?? System.String.Empty;
    }

    /// <summary>
    /// Số khung.
    /// </summary>
    [MaxLength(17, ErrorMessage = "Frame number must not exceed 17 characters.")]
    public System.String FrameNumber
    {
        get => _frameNumber;
        set => _frameNumber = value.Trim();
    }

    /// <summary>
    /// Số máy.
    /// </summary>
    [MaxLength(17, ErrorMessage = "Engine number must not exceed 17 characters.")]
    public System.String EngineNumber
    {
        get => _engineNumber;
        set => _engineNumber = value.Trim();
    }

    /// <summary>
    /// Ngày đăng ký xe.
    /// </summary>
    public System.DateTime RegistrationDate { get; set; } = System.DateTime.UtcNow;

    #endregion

    #region Usage and Maintenance Properties

    /// <summary>
    /// Quá trình lái xe (Km đã đi).
    /// </summary>
    [Range(0, 1000000, ErrorMessage = "Mileage must be between 0 and 1,000,000 km.")]
    public System.Double Mileage { get; set; } = 0;

    /// <summary>
    /// Ngày hết hạn bảo hiểm.
    /// </summary>
    public System.DateTime? InsuranceExpiryDate { get; set; }

    /// <summary>
    /// Lịch sử sửa chữa của xe.
    /// </summary>
    public virtual ICollection<RepairOrder> RepairOrder { get; set; } = [];

    #endregion
}