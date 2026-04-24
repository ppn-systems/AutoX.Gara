using AutoX.Gara.Contracts.Enums;
// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Contracts.Extensions;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;
namespace AutoX.Gara.Contracts.Auth;
/// <summary>
/// Represents a login packet that carries authentication credentials
/// (username, hashed password, metadata) from the client to the server.
/// Uses PacketBase for automatic serialization, pooling and metadata handling.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class LoginPacket : PacketBase<LoginPacket>
{
    /// <summary>
    /// Gets or sets the login credentials model (username, hashed password, metadata).
    /// </summary>
    [SerializeOrder(0)]
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
}



