// Copyright (c) 2026 PPN Corporation. All rights reserved.

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

namespace AutoX.Gara.Shared.Packets.Customers;

/// <summary>
/// Represents a customer data packet used for create, update, and query operations.
/// Carries customer profile information including identity, contact details, and membership metadata.
/// Uses PacketBase for automatic serialization, pooling and metadata handling.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class CustomerDataPacket : PacketBase<CustomerDataPacket>, IPacketTransformer<CustomerDataPacket>, IPacketSequenced
{
    /// <summary>
    /// Gets or sets the sequence identifier used for packet ordering and deduplication.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION)]
    public System.UInt32 SequenceId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the customer.
    /// null when creating a new customer record.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 1)]
    public System.Int32? CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the full name of the customer.
    /// </summary>
    [SensitiveData(DataSensitivityLevel.Internal)]
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 2)]
    public System.String Name { get; set; }

    /// <summary>
    /// Gets or sets the email address of the customer.
    /// </summary>
    [SensitiveData(DataSensitivityLevel.Internal)]
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 3)]
    public System.String Email { get; set; }

    /// <summary>
    /// Gets or sets the phone number of the customer.
    /// </summary>
    [SensitiveData(DataSensitivityLevel.Internal)]
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 4)]
    public System.String PhoneNumber { get; set; }

    /// <summary>
    /// Gets or sets the physical address of the customer.
    /// </summary>
    [SensitiveData(DataSensitivityLevel.Internal)]
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 5)]
    public System.String Address { get; set; }

    /// <summary>
    /// Gets or sets the tax identification code of the customer.
    /// </summary>
    [SensitiveData(DataSensitivityLevel.Internal)]
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 6)]
    public System.String TaxCode { get; set; }

    /// <summary>
    /// Gets or sets the customer classification type (e.g., Individual, Corporate).
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 7)]
    public CustomerType? Type { get; set; }

    /// <summary>
    /// Gets or sets the membership level of the customer (e.g., Bronze, Silver, Gold).
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 8)]
    public MembershipLevel? Membership { get; set; }

    /// <summary>
    /// Gets or sets the date of birth of the customer.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 9)]
    public System.DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the customer record was created.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 10)]
    public System.DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the customer record was last updated.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 11)]
    public System.DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Initializes a new instance of <see cref="CustomerDataPacket"/> with default empty values.
    /// </summary>
    public CustomerDataPacket()
    {
        // Initialize reference/string properties to safe defaults.
        Name = System.String.Empty;
        Email = System.String.Empty;
        Address = System.String.Empty;
        TaxCode = System.String.Empty;
        PhoneNumber = System.String.Empty;
        OpCode = OpCommand.NONE.AsUInt16();

        // MagicNumber is set automatically by PacketBase based on the concrete type.
        // If you need to preserve a legacy magic number, set it explicitly here.
        // this.MagicNumber = PacketMagic.YOUR_LEGACY_VALUE.AsUInt32();
    }

    /// <inheritdoc/>
    public override void ResetForPool()
    {
        // Let base reset serializable properties and header fields according to cached metadata.
        base.ResetForPool();

        // Ensure complex/reference properties are set to safe defaults in case metadata defaults differ.
        SequenceId = 0;
        CustomerId = null;
        Name = System.String.Empty;
        Email = System.String.Empty;
        Address = System.String.Empty;
        TaxCode = System.String.Empty;
        PhoneNumber = System.String.Empty;
        Type = null;
        Membership = null;
        DateOfBirth = null;
        CreatedAt = null;
        UpdatedAt = null;

        // Re-assert OpCode to default for clarity (base.ResetForPool already sets header fields).
        OpCode = OpCommand.NONE.AsUInt16();
    }

    /// <summary>
    /// Compress string fields and mark packet as compressed.
    /// </summary>
    /// <exception cref="System.ArgumentNullException">Thrown when packet is null.</exception>
    public static CustomerDataPacket Compress(CustomerDataPacket packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);

        packet.Name = packet.Name.CompressToBase64();
        packet.Email = packet.Email.CompressToBase64();
        packet.Address = packet.Address.CompressToBase64();
        packet.TaxCode = packet.TaxCode.CompressToBase64();
        packet.PhoneNumber = packet.PhoneNumber.CompressToBase64();

        packet.Flags.AddFlag(PacketFlags.COMPRESSED);
        return packet;
    }

    /// <summary>
    /// Decompress string fields and remove compressed flag.
    /// </summary>
    /// <exception cref="System.ArgumentNullException">Thrown when packet is null.</exception>
    public static CustomerDataPacket Decompress(CustomerDataPacket packet)
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