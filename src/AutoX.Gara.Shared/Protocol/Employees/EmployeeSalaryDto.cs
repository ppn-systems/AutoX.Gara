// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Employees;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets.Abstractions;
using Nalix.Common.Networking.Packets.Enums;
using Nalix.Common.Serialization;
using Nalix.Common.Serialization.Attributes;
using Nalix.Shared.Extensions;
using Nalix.Shared.Frames;

namespace AutoX.Gara.Shared.Protocol.Employees;

[SerializePackable(SerializeLayout.Explicit)]
public sealed class EmployeeSalaryDto : PacketBase<EmployeeSalaryDto>, IPacketTransformer<EmployeeSalaryDto>, IPacketSequenced
{
    // Fixed-size fields

    [SerializeOrder(PacketHeaderOffset.DATA_REGION)]
    public System.UInt32 SequenceId { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 1)]
    public System.Int32? EmployeeSalaryId { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 2)]
    public System.Int32 EmployeeId { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 3)]
    public System.Decimal Salary { get; set; }

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 4)]
    public SalaryType SalaryType { get; set; } = SalaryType.Monthly;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 5)]
    public System.Decimal SalaryUnit { get; set; } = 1;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 6)]
    public System.DateTime EffectiveFrom { get; set; } = System.DateTime.UtcNow;

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 7)]
    public System.DateTime? EffectiveTo { get; set; }

    // Dynamic field(s)

    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 8)]
    public System.String Note { get; set; } = System.String.Empty;

    public EmployeeSalaryDto() => OpCode = OpCommand.NONE.AsUInt16();

    public override void ResetForPool()
    {
        base.ResetForPool();

        SequenceId = 0;
        EmployeeSalaryId = null;
        EmployeeId = 0;
        Salary = 0;
        SalaryType = SalaryType.Monthly;
        SalaryUnit = 1;
        EffectiveFrom = System.DateTime.UtcNow;
        EffectiveTo = null;
        Note = System.String.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }

    public static EmployeeSalaryDto Compress(EmployeeSalaryDto packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);
        packet.Note = packet.Note.CompressToBase64();
        packet.Flags.AddFlag(PacketFlags.COMPRESSED);
        return packet;
    }

    public static EmployeeSalaryDto Decompress(EmployeeSalaryDto packet)
    {
        System.ArgumentNullException.ThrowIfNull(packet);
        packet.Note = packet.Note.DecompressFromBase64();
        packet.Flags.RemoveFlag(PacketFlags.COMPRESSED);
        return packet;
    }
}
