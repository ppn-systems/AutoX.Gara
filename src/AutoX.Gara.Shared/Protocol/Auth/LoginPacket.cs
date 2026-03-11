// Copyright (c) 2026 PPN Corporation. All rights reserved.

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

namespace AutoX.Gara.Shared.Protocol.Auth;

/// <summary>
/// Represents a login packet that carries authentication credentials
/// (username, hashed password, metadata) from the client to the server.
/// Uses PacketBase for automatic serialization, pooling and metadata handling.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class LoginPacket : PacketBase<LoginPacket>, IPacketTransformer<LoginPacket>, IPacketSequenced
{
    /// <summary>
    /// Gets or sets the sequence identifier used for packet ordering and deduplication.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION)]
    public System.UInt32 SequenceId { get; set; }

    /// <summary>
    /// Gets or sets the login credentials model (username, hashed password, metadata).
    /// </summary>
    [SensitiveData(DataSensitivityLevel.Internal)]
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 1)]
    public LoginRequestModel Account { get; set; }

    /// <summary>
    /// Initializes a new instance of <see cref="LoginPacket"/> with default values.
    /// We explicitly set MagicNumber to the legacy PacketMagic.ACCOUNT value to preserve
    /// prior wire identity for this packet type.
    /// </summary>
    public LoginPacket()
    {
        Account = new LoginRequestModel();
        OpCode = OpCommand.LOGIN.AsUInt16();
    }

    /// <summary>
    /// Initializes the packet with the specified operation code and account credentials.
    /// </summary>
    /// <param name="opCode">The operation code identifying the request type.</param>
    /// <param name="account">The login credentials model. Must not be null.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="account"/> is null.</exception>
    public void Initialize(System.UInt16 opCode, LoginRequestModel account)
    {
        OpCode = opCode;
        Account = account ?? throw new System.ArgumentNullException(nameof(account));
    }

    /// <inheritdoc/>
    public override void ResetForPool()
    {
        // Let PacketBase reset header fields and serializable properties according to metadata.
        base.ResetForPool();

        // Ensure complex/reference properties are set to safe defaults.
        SequenceId = 0;
        Account = new LoginRequestModel();

        // OpCode already reset by base.ResetForPool(), but keep explicit reset for clarity.
        OpCode = OpCommand.LOGIN.AsUInt16();
    }

    /// <summary>
    /// Compress username/password fields and mark packet as compressed.
    /// </summary>
    /// <exception cref="System.ArgumentNullException">Thrown when packet or its Account is null.</exception>
    public static LoginPacket Compress(LoginPacket packet)
    {
        if (packet?.Account is null)
        {
            throw new System.ArgumentNullException(nameof(packet));
        }

        packet.Account.Username = packet.Account.Username.CompressToBase64();
        packet.Account.Password = packet.Account.Password.CompressToBase64();
        packet.Flags.AddFlag(PacketFlags.COMPRESSED);

        return packet;
    }

    /// <summary>
    /// Decompress username/password fields and remove compressed flag.
    /// </summary>
    /// <exception cref="System.ArgumentNullException">Thrown when packet or its Account is null.</exception>
    public static LoginPacket Decompress(LoginPacket packet)
    {
        if (packet?.Account is null)
        {
            throw new System.ArgumentNullException(nameof(packet));
        }

        packet.Account.Username = packet.Account.Username.DecompressFromBase64();
        packet.Account.Password = packet.Account.Password.DecompressFromBase64();
        packet.Flags.RemoveFlag(PacketFlags.COMPRESSED);

        return packet;
    }
}