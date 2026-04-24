// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Contracts.Enums;
using AutoX.Gara.Contracts.Extensions;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;
namespace AutoX.Gara.Contracts.Billings;
[SerializePackable(SerializeLayout.Explicit)]
public sealed class ServiceItemDto : PacketBase<ServiceItemDto>
{
    [SerializeOrder(0)]
    public int? ServiceItemId { get; set; }
    [SerializeOrder(1)]
    public ServiceType Type { get; set; }
    [SerializeOrder(2)]
    public decimal UnitPrice { get; set; }
    [SerializeOrder(3)]
    public string Description { get; set; }
    public ServiceItemDto()
    {
        Description = string.Empty;
        Type = ServiceType.None;
        UnitPrice = 0;
        OpCode = OpCommand.NONE.AsUInt16();
    }
    public override void ResetForPool()
    {
        base.ResetForPool();
        SequenceId = 0;
        ServiceItemId = null;
        Type = ServiceType.None;
        UnitPrice = 0;
        Description = string.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}



