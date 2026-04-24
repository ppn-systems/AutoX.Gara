using AutoX.Gara.Domain.Abstractions;
using AutoX.Gara.Domain.Entities.Invoices;
using AutoX.Gara.Domain.Enums.Cars;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoX.Gara.Domain.Entities.Customers;

/// <summary>
/// Lớp đại diện cho xe.
/// </summary>
[Table(nameof(Vehicle))]
public class Vehicle : AuditEntity<int>
{
    #region Fields

    private string _carLicensePlate = string.Empty;
    private string _engineNumber = string.Empty;
    private string _frameNumber = string.Empty;
    private string _carModel = string.Empty;

    #endregion

    #region Identification Properties

    /// <summary>
    /// Id chủ xe.
    /// </summary>
    [Required]
    public int CustomerId { get; set; }

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
    public int Year { get; set; } = 1900;

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
    public string Model
    {
        get => _carModel;
        set => _carModel = value?.Trim() ?? string.Empty;
    }

    #endregion

    #region Registration Properties

    /// <summary>
    /// Biển số xe khách hàng.
    /// </summary>
    [Required(ErrorMessage = "Vehicle license plate is required.")]
    [MaxLength(9)]
    [RegularExpression(@"^[0-9]{2}[A-Z]{1,2}-[0-9]{3,5}$", ErrorMessage = "Invalid license plate format.")]
    public string LicensePlate
    {
        get => _carLicensePlate;
        set => _carLicensePlate = value?.Trim().ToUpper() ?? string.Empty;
    }

    /// <summary>
    /// Số khung.
    /// </summary>
    [MaxLength(17, ErrorMessage = "Frame number must not exceed 17 characters.")]
    public string FrameNumber
    {
        get => _frameNumber;
        set => _frameNumber = value?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Số máy.
    /// </summary>
    [MaxLength(17, ErrorMessage = "Engine number must not exceed 17 characters.")]
    public string EngineNumber
    {
        get => _engineNumber;
        set => _engineNumber = value?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Ngày đăng ký xe.
    /// </summary>
    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

    #endregion

    #region Usage and Maintenance Properties

    /// <summary>
    /// Quá trình lái xe (Km đã đi).
    /// </summary>
    [Range(0, 1000000, ErrorMessage = "Mileage must be between 0 and 1,000,000 km.")]
    public double Mileage { get; set; } = 0;

    /// <summary>
    /// Ngày hết hạn bảo hiểm.
    /// </summary>
    public DateTime? InsuranceExpiryDate { get; set; }

    /// <summary>
    /// Lịch sử sửa chữa của xe.
    /// </summary>
    public virtual ICollection<RepairOrder> RepairOrders { get; set; } = [];

    #endregion
}
