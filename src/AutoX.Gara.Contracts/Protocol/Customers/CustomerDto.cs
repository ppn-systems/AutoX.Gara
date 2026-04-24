// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Customers;
using AutoX.Gara.Contracts.Enums;
using AutoX.Gara.Contracts.Extensions;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;
using System;
namespace AutoX.Gara.Contracts.Customers;
/// <summary>
/// Represents a customer data packet used for create, update, and query operations.
/// Carries customer profile information including identity, contact details, and membership metadata.
/// Uses PacketBase for automatic serialization, pooling and metadata handling.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class CustomerDto : PacketBase<CustomerDto>
{
    // --- Fixed-size fields (d?t tru?c dynamic strings) -----------------------
    /// <summary>
    /// Gets or sets the unique identifier of the customer.
    /// null when creating a new customer record.
    /// </summary>
    [SerializeOrder(0)]
    public int? CustomerId { get; set; }
    /// <summary>Gets or sets the customer classification type (e.g., Individual, Corporate).</summary>
    [SerializeOrder(1)]
    public CustomerType? Type { get; set; }
    /// <summary>Gets or sets the membership level of the customer (e.g., Bronze, Silver, Gold).</summary>
    [SerializeOrder(2)]
    public MembershipLevel? Membership { get; set; }
    /// <summary>Gets or sets the gender of the customer.</summary>
    [SerializeOrder(3)]
    public Gender? Gender { get; set; }
    /// <summary>Gets or sets the date of birth of the customer.</summary>
    [SerializeOrder(4)]
    public DateTime? DateOfBirth { get; set; }
    /// <summary>Gets or sets the UTC timestamp when the customer record was created.</summary>
    [SerializeOrder(5)]
    public DateTime? CreatedAt { get; set; }
    /// <summary>Gets or sets the UTC timestamp when the customer record was last updated.</summary>
    [SerializeOrder(6)]
    public DateTime? UpdatedAt { get; set; }
    // --- Dynamic-size fields (d?t sau t?t c? fixed-size) ---------------------
    // Quy t?c SerializePackable: string fields ph?i d?ng SAU enum/int/DateTime
    // d? PacketBase.Length t�nh d�ng wire-size.
    /// <summary>Gets or sets the full name of the customer.</summary>
    [SerializeOrder(7)]
    public string Name { get; set; }
    /// <summary>Gets or sets the email address of the customer.</summary>
    [SerializeOrder(8)]
    public string Email { get; set; }
    /// <summary>Gets or sets the phone number of the customer.</summary>
    [SerializeOrder(9)]
    public string PhoneNumber { get; set; }
    /// <summary>Gets or sets the physical address of the customer.</summary>
    [SerializeOrder(10)]
    public string Address { get; set; }
    /// <summary>Gets or sets the tax identification code of the customer.</summary>
    [SerializeOrder(11)]
    public string TaxCode { get; set; }
    /// <summary>
    /// Gets or sets internal staff notes about this customer.
    /// Not visible to the customer. Max 500 characters.
    /// </summary>
    [SerializeOrder(12)]
    public string Notes { get; set; }
    // --- Constructor ---------------------------------------------------------
    /// <summary>Initializes a new instance of <see cref="CustomerDto"/> with default empty values.</summary>
    public CustomerDto()
    {
        Name = string.Empty;
        Email = string.Empty;
        Address = string.Empty;
        TaxCode = string.Empty;
        PhoneNumber = string.Empty;
        Notes = string.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
    // --- Pool Reset -----------------------------------------------------------
    /// <inheritdoc/>
    public override void ResetForPool()
    {
        base.ResetForPool();
        SequenceId = 0;
        CustomerId = null;
        Type = null;
        Membership = null;
        Gender = null;
        DateOfBirth = null;
        CreatedAt = null;
        UpdatedAt = null;
        Name = string.Empty;
        Email = string.Empty;
        Address = string.Empty;
        TaxCode = string.Empty;
        PhoneNumber = string.Empty;
        Notes = string.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}



