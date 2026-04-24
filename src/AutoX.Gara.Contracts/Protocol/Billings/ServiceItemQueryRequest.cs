// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Contracts.Enums;
using AutoX.Gara.Contracts.Extensions;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;
namespace AutoX.Gara.Contracts.Billings;
[SerializePackable(SerializeLayout.Explicit)]
public sealed class ServiceItemQueryRequest : PacketBase<ServiceItemQueryRequest>
{
    [SerializeOrder(0)]
    public int Page { get; set; } = 1;
    [SerializeOrder(1)]
    public int PageSize { get; set; } = 20;
    [SerializeOrder(2)]
    public ServiceItemSortField SortBy { get; set; } = ServiceItemSortField.Description;
    [SerializeOrder(3)]
    public bool SortDescending { get; set; } = false;
    [SerializeOrder(4)]
    public ServiceType? FilterType { get; set; } = null;
    [SerializeOrder(5)]
    public decimal? FilterMinUnitPrice { get; set; } = null;
    [SerializeOrder(6)]
    public decimal? FilterMaxUnitPrice { get; set; } = null;
    [SerializeOrder(7)]
    public string SearchTerm { get; set; } = string.Empty;
    public ServiceItemQueryRequest() => OpCode = OpCommand.NONE.AsUInt16();
    public override void ResetForPool()
    {
        base.ResetForPool();
        SequenceId = 0;
        Page = 1;
        PageSize = 20;
        SortBy = ServiceItemSortField.Description;
        SortDescending = false;
        FilterType = null;
        FilterMinUnitPrice = null;
        FilterMaxUnitPrice = null;
        SearchTerm = string.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}



