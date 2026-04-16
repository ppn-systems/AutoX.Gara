// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Customers;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;

namespace AutoX.Gara.Shared.Protocol.Customers;

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
    [SerializeOrder(PacketHeaderOffset.Region + 1)]
    public System.Int32? CustomerId { get; set; }

    /// <summary>Gets or sets the customer classification type (e.g., Individual, Corporate).</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public CustomerType? Type { get; set; }

    /// <summary>Gets or sets the membership level of the customer (e.g., Bronze, Silver, Gold).</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 3)]
    public MembershipLevel? Membership { get; set; }

    /// <summary>Gets or sets the gender of the customer.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 4)]
    public Gender? Gender { get; set; }

    /// <summary>Gets or sets the date of birth of the customer.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 5)]
    public System.DateTime? DateOfBirth { get; set; }

    /// <summary>Gets or sets the UTC timestamp when the customer record was created.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 6)]
    public System.DateTime? CreatedAt { get; set; }

    /// <summary>Gets or sets the UTC timestamp when the customer record was last updated.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 7)]
    public System.DateTime? UpdatedAt { get; set; }

    // --- Dynamic-size fields (d?t sau t?t c? fixed-size) ---------------------
    // Quy t?c SerializePackable: string fields ph?i d?ng SAU enum/int/DateTime
    // d? PacketBase.Length tính dúng wire-size.

    /// <summary>Gets or sets the full name of the customer.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 8)]
    public System.String Name { get; set; }

    /// <summary>Gets or sets the email address of the customer.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 9)]
    public System.String Email { get; set; }

    /// <summary>Gets or sets the phone number of the customer.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 10)]
    public System.String PhoneNumber { get; set; }

    /// <summary>Gets or sets the physical address of the customer.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 11)]
    public System.String Address { get; set; }

    /// <summary>Gets or sets the tax identification code of the customer.</summary>
    [SerializeOrder(PacketHeaderOffset.Region + 12)]
    public System.String TaxCode { get; set; }

    /// <summary>
    /// Gets or sets internal staff notes about this customer.
    /// Not visible to the customer. Max 500 characters.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.Region + 13)]
    public System.String Notes { get; set; }

    // --- Constructor ---------------------------------------------------------

    /// <summary>Initializes a new instance of <see cref="CustomerDto"/> with default empty values.</summary>
    public CustomerDto()
    {
        Name = System.String.Empty;
        Email = System.String.Empty;
        Address = System.String.Empty;
        TaxCode = System.String.Empty;
        PhoneNumber = System.String.Empty;
        Notes = System.String.Empty;
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
        Name = System.String.Empty;
        Email = System.String.Empty;
        Address = System.String.Empty;
        TaxCode = System.String.Empty;
        PhoneNumber = System.String.Empty;
        Notes = System.String.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}