using AutoX.Gara.Domain.Enums.Customers;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Attributes;
using Nalix.Common.Enums;
using Nalix.Common.Infrastructure.Caching;
using Nalix.Common.Messaging.Packets;
using Nalix.Common.Messaging.Packets.Abstractions;
using Nalix.Common.Serialization;
using Nalix.Framework.Injection;
using Nalix.Shared.Extensions;
using Nalix.Shared.Memory.Pooling;
using Nalix.Shared.Messaging;
using Nalix.Shared.Serialization;

namespace AutoX.Gara.Shared.Packets;

/// <summary>
/// Packet for customer data, used in create/update/query operations.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
[MagicNumber((System.UInt32)PacketMagic.CUSTOMER)]
public class CustomerPacket : FrameBase, IPoolable, IPacketEncryptor<CustomerPacket>
{
    public override System.UInt16 Length =>
        (System.UInt16)(sizeof(System.Int32) + (System.UInt16)
        (System.Text.Encoding.UTF8.GetByteCount(Name) +
        System.Text.Encoding.UTF8.GetByteCount(PhoneNumber) +
        System.Text.Encoding.UTF8.GetByteCount(Email) +
        System.Text.Encoding.UTF8.GetByteCount(Address) +
        System.Text.Encoding.UTF8.GetByteCount(TaxCode) +
        (sizeof(System.Int64) * 2) + (sizeof(System.Byte) * 2)));

    // --- Identity ---
    /// <summary>
    /// Customer unique identifier. Null for creation.
    /// </summary>
    [SerializeOrder(0)]
    public System.Int32? CustomerId { get; set; }

    // --- Basic Info ---
    /// <summary>
    /// Customer full name.
    /// </summary>
    [SerializeOrder(1)]
    public System.String Name { get; set; } = System.String.Empty;

    /// <summary>
    /// Customer primary phone number.
    /// </summary>
    [SerializeOrder(2)]
    public System.String PhoneNumber { get; set; } = System.String.Empty;

    /// <summary>
    /// Customer email address.
    /// </summary>
    [SerializeOrder(3)]
    public System.String Email { get; set; } = System.String.Empty;

    /// <summary>
    /// Customer physical address.
    /// </summary>
    [SerializeOrder(4)]
    public System.String Address { get; set; } = System.String.Empty;

    // --- Personal Details ---
    /// <summary>
    /// Date of birth (for individual customers).
    /// </summary>
    [SerializeOrder(5)]
    public System.DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Tax registration code (for business customers).
    /// </summary>
    [SerializeOrder(6)]
    public System.String TaxCode { get; set; } = System.String.Empty;

    // --- Membership ---
    /// <summary>
    /// Customer type (Individual, Company etc.).
    /// </summary>
    [SerializeOrder(7)]
    public CustomerType? Type { get; set; }

    /// <summary>
    /// Membership tier (Standard, Gold etc.).
    /// </summary>
    [SerializeOrder(8)]
    public MembershipLevel? Membership { get; set; }

    // --- Audit ---
    /// <summary>
    /// Record creation timestamp.
    /// </summary>
    [SerializeOrder(9)]
    public System.DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Record last update timestamp.
    /// </summary>
    [SerializeOrder(10)]
    public System.DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Default constructor: initializes default values.
    /// </summary>
    public CustomerPacket()
    {
        // Nếu bạn có logic mặc định, thiết lập ở đây
        Name = System.String.Empty;
        PhoneNumber = System.String.Empty;
        Email = System.String.Empty;
        Address = System.String.Empty;
        TaxCode = System.String.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }

    /// <inheritdoc/>
    public static CustomerPacket Deserialize(System.ReadOnlySpan<System.Byte> buffer)
    {
        CustomerPacket packet = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>()
                                                           .Get<CustomerPacket>();

        _ = LiteSerializer.Deserialize(buffer, ref packet);
        return packet;
    }

    /// <inheritdoc/>
    public override System.Byte[] Serialize() => LiteSerializer.Serialize(this);

    /// <inheritdoc/>
    public override System.Int32 Serialize(System.Span<System.Byte> buffer) => LiteSerializer.Serialize(this, buffer);

    /// <inheritdoc/>
    public override void ResetForPool()
    {
        Name = System.String.Empty;
        PhoneNumber = System.String.Empty;
        Email = System.String.Empty;
        Address = System.String.Empty;
        TaxCode = System.String.Empty;
        OpCode = OpCommand.NONE.AsUInt16();

        Type = null;
        CreatedAt = null;
        UpdatedAt = null;
        Membership = null;
        DateOfBirth = null;
    }

    public static CustomerPacket Encrypt(CustomerPacket packet, System.Byte[] key, CipherSuiteType algorithm)
    {
        System.ArgumentNullException.ThrowIfNull(packet);

        packet.Email = packet.Email.EncryptToBase64(key, algorithm);
        packet.Address = packet.Address.EncryptToBase64(key, algorithm);
        packet.TaxCode = packet.TaxCode.EncryptToBase64(key, algorithm);
        packet.Name = packet.Name.EncryptToBase64(key, algorithm);
        packet.PhoneNumber = packet.PhoneNumber.EncryptToBase64(key, algorithm);

        packet.Flags.AddFlag(PacketFlags.ENCRYPTED);
        return packet;
    }
    public static CustomerPacket Decrypt(CustomerPacket packet, System.Byte[] key)
    {
        System.ArgumentNullException.ThrowIfNull(packet);

        try
        {
            packet.Email = packet.Email.DecryptFromBase64(key);
            packet.Address = packet.Address.DecryptFromBase64(key);
            packet.TaxCode = packet.TaxCode.DecryptFromBase64(key);
            packet.Name = packet.Name.DecryptFromBase64(key);
            packet.PhoneNumber = packet.PhoneNumber.DecryptFromBase64(key);

            packet.Flags.RemoveFlag(PacketFlags.ENCRYPTED);
            return packet;
        }
        catch (System.Exception ex)
        {
            throw new System.InvalidOperationException("Failed to decrypt customer data.", ex);
        }
    }
}
