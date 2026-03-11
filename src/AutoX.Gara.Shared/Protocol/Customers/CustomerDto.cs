// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Customers;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets.Abstractions;
using Nalix.Common.Networking.Packets.Enums;
using Nalix.Common.Security.Attributes;
using Nalix.Common.Security.Enums;
using Nalix.Common.Serialization;
using Nalix.Common.Serialization.Attributes;
using Nalix.Shared.Extensions;
using Nalix.Shared.Frames;

namespace AutoX.Gara.Shared.Protocol.Customers;

/// <summary>
/// Represents a customer data packet used for create, update, and query operations.
/// Carries customer profile information including identity, contact details, and membership metadata.
/// Uses PacketBase for automatic serialization, pooling and metadata handling.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class CustomerDto : PacketBase<CustomerDto>, IPacketTransformer<CustomerDto>, IPacketSequenced
{
    // ─── Fixed-size fields (đặt trước dynamic strings) ───────────────────────

    /// <summary>Gets or sets the sequence identifier used for packet ordering and deduplication.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION)]
    public System.UInt32 SequenceId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the customer.
    /// null when creating a new customer record.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 1)]
    public System.Int32? CustomerId { get; set; }

    /// <summary>Gets or sets the customer classification type (e.g., Individual, Corporate).</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 2)]
    public CustomerType? Type { get; set; }

    /// <summary>Gets or sets the membership level of the customer (e.g., Bronze, Silver, Gold).</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 3)]
    public MembershipLevel? Membership { get; set; }

    /// <summary>Gets or sets the gender of the customer.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 4)]
    public Gender? Gender { get; set; }

    /// <summary>Gets or sets the date of birth of the customer.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 5)]
    public System.DateTime? DateOfBirth { get; set; }

    /// <summary>Gets or sets the UTC timestamp when the customer record was created.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 6)]
    public System.DateTime? CreatedAt { get; set; }

    /// <summary>Gets or sets the UTC timestamp when the customer record was last updated.</summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 7)]
    public System.DateTime? UpdatedAt { get; set; }

    // ─── Dynamic-size fields (đặt sau tất cả fixed-size) ─────────────────────
    // Quy tắc SerializePackable: string fields phải đứng SAU enum/int/DateTime
    // để PacketBase.Length tính đúng wire-size.

    /// <summary>Gets or sets the full name of the customer.</summary>
    [SensitiveData(DataSensitivityLevel.Internal)]
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 8)]
    public System.String Name { get; set; }

    /// <summary>Gets or sets the email address of the customer.</summary>
    [SensitiveData(DataSensitivityLevel.Internal)]
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 9)]
    public System.String Email { get; set; }

    /// <summary>Gets or sets the phone number of the customer.</summary>
    [SensitiveData(DataSensitivityLevel.Internal)]
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 10)]
    public System.String PhoneNumber { get; set; }

    /// <summary>Gets or sets the physical address of the customer.</summary>
    [SensitiveData(DataSensitivityLevel.Internal)]
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 11)]
    public System.String Address { get; set; }

    /// <summary>Gets or sets the tax identification code of the customer.</summary>
    [SensitiveData(DataSensitivityLevel.Internal)]
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 12)]
    public System.String TaxCode { get; set; }

    /// <summary>
    /// Gets or sets internal staff notes about this customer.
    /// Not visible to the customer. Max 500 characters.
    /// </summary>
    [SensitiveData(DataSensitivityLevel.Internal)]
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 13)]
    public System.String Notes { get; set; }

    // ─── Constructor ─────────────────────────────────────────────────────────

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

    // ─── Pool Reset ───────────────────────────────────────────────────────────

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

    // ─── Compression ─────────────────────────────────────────────────────────

    /// <summary>Compress string fields and mark packet as compressed.</summary>
    /// <exception cref="System.ArgumentNullException">Thrown when packet is null.</exception>
    public static CustomerDto Compress(CustomerDto packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);

        packet.Name = packet.Name.CompressToBase64();
        packet.Email = packet.Email.CompressToBase64();
        packet.Address = packet.Address.CompressToBase64();
        packet.TaxCode = packet.TaxCode.CompressToBase64();
        packet.PhoneNumber = packet.PhoneNumber.CompressToBase64();
        // Notes không compress vì thường ngắn, tránh overhead

        packet.Flags.AddFlag(PacketFlags.COMPRESSED);
        return packet;
    }

    /// <summary>Decompress string fields and remove compressed flag.</summary>
    /// <exception cref="System.ArgumentNullException">Thrown when packet is null.</exception>
    public static CustomerDto Decompress(CustomerDto packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);

        packet.Name = packet.Name.DecompressFromBase64();
        packet.Email = packet.Email.DecompressFromBase64();
        packet.Address = packet.Address.DecompressFromBase64();
        packet.TaxCode = packet.TaxCode.DecompressFromBase64();
        packet.PhoneNumber = packet.PhoneNumber.DecompressFromBase64();

        packet.Flags.RemoveFlag(PacketFlags.COMPRESSED);
        return packet;
    }
}