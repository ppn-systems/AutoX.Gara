using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace AutoX.Gara.Domain.Enums;

/// <summary>
/// �?i di?n cho gi?i t�nh c?a nh�n vi�n.
/// </summary>
public enum Gender : byte
{
    /// <summary>
    /// Gi?i t�nh kh�ng x�c d?nh ho?c kh�ng cung c?p.
    /// </summary>
    [Display(Name = "Kh�ng x�c d?nh")]
    None = 0,

    /// <summary>
    /// Gi?i t�nh nam.
    /// </summary>
    [Display(Name = "Nam")]
    Male = 1,

    /// <summary>
    /// Gi?i t�nh n?.
    /// </summary>
    [Display(Name = "N?")]
    Female = 2
}