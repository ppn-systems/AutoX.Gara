// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Networking.Packets.Abstractions;
using Nalix.Common.Networking.Packets.Enums;
using Nalix.Common.Security.Attributes;
using Nalix.Common.Security.Enums;
using Nalix.Common.Serialization;
using Nalix.Common.Serialization.Attributes;
using Nalix.Framework.Injection;
using Nalix.Shared.Extensions;
using Nalix.Shared.Frames;
using Nalix.Shared.Memory.Pooling;
using System.Collections.Generic;

namespace AutoX.Gara.Shared.Protocol.Employees;

/// <summary>
/// Packet trả về danh sách nhân viên theo trang từ server xuống client.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class EmployeeQueryResponse : PacketBase<EmployeeQueryResponse>, IPacketTransformer<EmployeeQueryResponse>, IPacketSequenced
{
    [SerializeIgnore]
    public override System.UInt16 Length
    {
        get
        {
            System.Int32 total = PacketConstants.HeaderSize
                + sizeof(System.UInt32)   // SequenceId
                + sizeof(System.Int32)    // TotalCount
                + sizeof(System.Int32);   // list item-count prefix

            for (System.Int32 i = 0; i < Employees.Count; i++)
            {
                total += Employees[i].Length;
            }

            return (System.UInt16)total;
        }
    }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION)]
    public System.UInt32 SequenceId { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 1)]
    public System.Int32 TotalCount { get; set; }

    [SensitiveData(DataSensitivityLevel.Internal)]
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 2)]
    public List<EmployeeDto> Employees { get; set; } = [];

    public EmployeeQueryResponse() => OpCode = OpCommand.NONE.AsUInt16();

    public override void ResetForPool()
    {
        if (Employees?.Count > 0)
        {
            var pool = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>();
            for (System.Int32 i = 0; i < Employees.Count; i++)
            {
                if (Employees[i] is not null)
                {
                    pool.Return(Employees[i]);
                }
            }
        }

        Employees.Clear();
        SequenceId = 0;
        TotalCount = 0;
        OpCode = OpCommand.NONE.AsUInt16();

        base.ResetForPool();
    }

    public static EmployeeQueryResponse Compress(EmployeeQueryResponse packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);

        for (System.Int32 i = 0; i < packet.Employees.Count; i++)
        {
            packet.Employees[i] = EmployeeDto.Compress(packet.Employees[i]);
        }

        packet.Flags.AddFlag(PacketFlags.COMPRESSED);
        return packet;
    }

    public static EmployeeQueryResponse Decompress(EmployeeQueryResponse packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);

        for (System.Int32 i = 0; i < packet.Employees.Count; i++)
        {
            packet.Employees[i] = EmployeeDto.Decompress(packet.Employees[i]);
        }

        packet.Flags.RemoveFlag(PacketFlags.COMPRESSED);
        return packet;
    }
}