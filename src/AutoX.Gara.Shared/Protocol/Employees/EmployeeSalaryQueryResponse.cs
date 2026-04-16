// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.Injection;
using Nalix.Framework.DataFrames;
using Nalix.Framework.Memory.Objects;
using System.Collections.Generic;

namespace AutoX.Gara.Shared.Protocol.Employees;

[SerializePackable(SerializeLayout.Explicit)]
public sealed class EmployeeSalaryQueryResponse : PacketBase<EmployeeSalaryQueryResponse>
{
    [SerializeIgnore]
    public override System.Int32 Length
    {
        get
        {
            System.Int32 total = PacketConstants.HeaderSize
                + sizeof(System.UInt32) // SequenceId
                + sizeof(System.Int32)  // TotalCount
                + sizeof(System.Int32); // list item-count prefix

            for (System.Int32 i = 0; i < Salaries.Count; i++)
            {
                total += Salaries[i].Length;
            }

            return total;
        }
    }

    [SerializeOrder(PacketHeaderOffset.Region + 1)]
    public System.Int32 TotalCount { get; set; }

    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public List<EmployeeSalaryDto> Salaries { get; set; } = [];

    public EmployeeSalaryQueryResponse() => OpCode = OpCommand.NONE.AsUInt16();

    public override void ResetForPool()
    {
        if (Salaries?.Count > 0)
        {
            var pool = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>();
            for (System.Int32 i = 0; i < Salaries.Count; i++)
            {
                if (Salaries[i] is not null)
                {
                    pool.Return(Salaries[i]);
                }
            }
        }

        Salaries.Clear();
        SequenceId = 0;
        TotalCount = 0;
        OpCode = OpCommand.NONE.AsUInt16();

        base.ResetForPool();
    }
}
