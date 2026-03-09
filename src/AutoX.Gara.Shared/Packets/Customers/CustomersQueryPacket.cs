// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Caching;
using Nalix.Common.Networking.Packets.Abstractions;
using Nalix.Common.Networking.Packets.Enums;
using Nalix.Common.Serialization;
using Nalix.Common.Serialization.Attributes;
using Nalix.Shared.Frames;

namespace AutoX.Gara.Shared.Packets.Customers;

/// <summary>
/// Represents a paging query request packet sent by the client
/// to retrieve a paginated list of customers from the server.
/// Uses PacketBase for automatic serialization, pooling and metadata handling.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class CustomersQueryPacket : PacketBase<CustomersQueryPacket>, IPoolable, IPacketSequenced
{
    /// <summary>
    /// Gets the sequence identifier used for packet ordering and deduplication.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION)]
    public System.UInt32 SequenceId { get; set; }

    /// <summary>
    /// Gets or sets the one-based page number to retrieve.
    /// Defaults to 1.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 1)]
    public System.Int32 Page { get; set; } = 1;

    /// <summary>
    /// Gets or sets the maximum number of customer records per page.
    /// Defaults to 20.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 2)]
    public System.Int32 PageSize { get; set; } = 20;

    /// <summary>
    /// Initializes a new instance of <see cref="CustomersQueryPacket"/> with default values.
    /// </summary>
    public CustomersQueryPacket() => OpCode = OpCommand.NONE.AsUInt16();// If you must preserve a legacy magic number, set it explicitly here:// this.MagicNumber = PacketMagic.YOUR_LEGACY_VALUE.AsUInt32();

    /// <inheritdoc/>
    public override void ResetForPool()
    {
        // Let the base reset serializable properties and header fields according to metadata.
        base.ResetForPool();

        // Re-apply domain-specific defaults that differ from metadata defaults.
        SequenceId = 0;
        Page = 1;
        PageSize = 20;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}