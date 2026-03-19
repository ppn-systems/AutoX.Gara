// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Networking.Packets.Enums;
using Nalix.Common.Serialization;
using Nalix.Common.Serialization.Attributes;
using Nalix.Framework.Injection;
using Nalix.Shared.Frames;
using Nalix.Shared.Memory.Pooling;
using System.Collections.Generic;

namespace AutoX.Gara.Shared.Protocol.Employees;

/// <summary>
/// Packet trả về danh sách nhân viên theo trang từ server xuống client.
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class EmployeeQueryResponse : PacketBase<EmployeeQueryResponse>
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

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 1)]
    public System.Int32 TotalCount { get; set; }

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
}