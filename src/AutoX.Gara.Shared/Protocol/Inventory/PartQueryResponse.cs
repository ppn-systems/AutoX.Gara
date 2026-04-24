using AutoX.Gara.Shared.Enums;
// Copyright (c) 2026 PPN Corporation. All rights reserved.



using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;
using Nalix.Framework.Extensions;
using Nalix.Framework.Injection;
using Nalix.Framework.Memory.Objects;
using System.Collections.Generic;



namespace AutoX.Gara.Shared.Protocol.Inventory;



// -----------------------------------------------------------------------------

// PART QUERY RESPONSE

// -----------------------------------------------------------------------------



/// <summary>

/// Packet tr? v? danh s�ch ph? t�ng (<c>Part</c>) theo trang.

/// </summary>

[SerializePackable(SerializeLayout.Explicit)]

public sealed class PartQueryResponse : PacketBase<PartQueryResponse>

{

    /// <summary>

    /// T?ng byte length c?a packet, t�nh b?ng tay v� c� dynamic list.

    /// </summary>

    [SerializeIgnore]

    public override int Length

    {

        get

        {

            int total = PacketConstants.HeaderSize

                + sizeof(System.UInt32)   // SequenceId

                + sizeof(int)    // TotalCount

                + sizeof(int);   // list item-count prefix



            for (int i = 0; i < Parts.Count; i++)

            {

                total += Parts[i].Length;

            }



            return total;

        }

    }



    /// <summary>

    /// T?ng s? ph? t�ng kh?p filter (tru?c khi ph�n trang).

    /// <para>Ph?i d?ng tru?c <see cref="Parts"/> ? fixed field ph?i tru?c dynamic field.</para>

    /// </summary>

    [SerializeOrder(PacketHeaderOffset.Region + 1)]

    public int TotalCount { get; set; }



    /// <summary>Danh s�ch ph? t�ng c?a trang hi?n t?i. Dynamic ? d?t cu?i c�ng.</summary>

    [SerializeOrder(PacketHeaderOffset.Region + 2)]

    public List<PartDto> Parts { get; set; } = [];



    /// <summary>Kh?i t?o v?i gi� tr? m?c d?nh.</summary>

    public PartQueryResponse() => OpCode = OpCommand.NONE.AsUInt16();



    /// <inheritdoc/>

    public override void ResetForPool()

    {

        if (Parts?.Count > 0)

        {

            var pool = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>();

            for (int i = 0; i < Parts.Count; i++)

            {

                if (Parts[i] is not null)

                {

                    pool.Return(Parts[i]);

                }

            }

        }



        Parts.Clear();

        SequenceId = 0;

        TotalCount = 0;

        OpCode = OpCommand.NONE.AsUInt16();

        base.ResetForPool();

    }



    /// <inheritdoc/>

    public static PartQueryResponse Compress(PartQueryResponse packet)

    {

        System.ArgumentNullException.ThrowIfNull(packet);

        packet.Flags.AddFlag(PacketFlags.COMPRESSED);

        return packet;

    }



    /// <inheritdoc/>

    public static PartQueryResponse Decompress(PartQueryResponse packet)

    {

        System.ArgumentNullException.ThrowIfNull(packet);

        packet.Flags.RemoveFlag(PacketFlags.COMPRESSED);

        return packet;

    }

}
