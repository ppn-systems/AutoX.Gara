// Copyright (c) 2026 PPN Corporation. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace AutoX.Gara.Domain.Enums.Employees;

/// <summary>
/// Tr?ng th�i l�m vi?c c?a nh�n vi�n.
/// </summary>
public enum EmploymentStatus : byte
{
    [Display(Name = "Kh�ng x�c d?nh")]
    None = 0,

    /// <summary>
    /// Nh�n vi�n dang l�m vi?c.
    /// </summary>
    [Display(Name = "�ang l�m vi?c")]
    Active = 1,

    /// <summary>
    /// Nh�n vi�n d� ngh? vi?c.
    /// </summary>
    [Display(Name = "�� ngh? vi?c")]
    Inactive = 2,

    /// <summary>
    /// Nh�n vi�n dang ngh? ph�p.
    /// </summary>
    [Display(Name = "�ang ngh? ph�p")]
    OnLeave = 3,

    /// <summary>
    /// Nh�n vi�n b? ch?m d?t h?p d?ng.
    /// </summary>
    [Display(Name = "B? sa th?i")]
    Terminated = 4,

    /// <summary>
    /// Nh�n vi�n d� được tuy?n d?ng nhung chua b?t d?u l�m vi?c.
    /// </summary>
    [Display(Name = "Ch? b?t d?u")]
    Pending = 5,
}
