// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums.Repairs;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.DataFrames;
using System;
namespace AutoX.Gara.Shared.Protocol.Invoices;
[SerializePackable(SerializeLayout.Explicit)]
public sealed class RepairOrderQueryRequest : PacketBase<RepairOrderQueryRequest>
{
    [SerializeOrder(PacketHeaderOffset.Region + 1)]
    public int Page { get; set; } = 1;
    [SerializeOrder(PacketHeaderOffset.Region + 2)]
    public int PageSize { get; set; } = 20;
    [SerializeOrder(PacketHeaderOffset.Region + 3)]
    public RepairOrderSortField SortBy { get; set; } = RepairOrderSortField.OrderDate;
    [SerializeOrder(PacketHeaderOffset.Region + 4)]
    public bool SortDescending { get; set; } = true;
    [SerializeOrder(PacketHeaderOffset.Region + 5)]
    public int FilterCustomerId { get; set; } = 0;
    [SerializeOrder(PacketHeaderOffset.Region + 6)]
    public int FilterVehicleId { get; set; } = 0;
    [SerializeOrder(PacketHeaderOffset.Region + 7)]
    public int FilterInvoiceId { get; set; } = 0;
    [SerializeOrder(PacketHeaderOffset.Region + 8)]
    public RepairOrderStatus? FilterStatus { get; set; } = null;
    [SerializeOrder(PacketHeaderOffset.Region + 9)]
    public DateTime? FilterFromDate { get; set; } = null;
    [SerializeOrder(PacketHeaderOffset.Region + 10)]
    public DateTime? FilterToDate { get; set; } = null;
    [SerializeOrder(PacketHeaderOffset.Region + 11)]
    public string SearchTerm { get; set; } = string.Empty;
    public RepairOrderQueryRequest() => OpCode = OpCommand.NONE.AsUInt16();
    public override void ResetForPool()
    {
        base.ResetForPool();
        SequenceId = 0;
        Page = 1;
        PageSize = 20;
        SortBy = RepairOrderSortField.OrderDate;
        SortDescending = true;
        FilterCustomerId = 0;
        FilterVehicleId = 0;
        FilterInvoiceId = 0;
        FilterStatus = null;
        FilterFromDate = null;
        FilterToDate = null;
        SearchTerm = string.Empty;
        OpCode = OpCommand.NONE.AsUInt16();
    }
}
