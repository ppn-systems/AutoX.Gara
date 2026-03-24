// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Extensions;
using Nalix.Common.Networking.Packets;
using Nalix.Common.Serialization;
using Nalix.Framework.Injection;
using Nalix.Shared.Frames;
using Nalix.Shared.Memory.Objects;
using System.Collections.Generic;

namespace AutoX.Gara.Shared.Protocol.Suppliers;

/// <summary>
/// Packet trả về danh sách nhà cung cấp theo trang từ server xuống client.
/// <para>
/// Cấu trúc Length tính thủ công vì chứa dynamic list <see cref="Suppliers"/>.
/// <see cref="TotalCount"/> PHẢI đứng trước <see cref="Suppliers"/> trong
/// <c>SerializeOrder</c> — fixed field sau dynamic list sẽ bị serializer bỏ qua.
/// </para>
/// </summary>
[SerializePackable(SerializeLayout.Explicit)]
public sealed class SupplierQueryResponse : PacketBase<SupplierQueryResponse>
{
    /// <summary>
    /// Tổng số byte của packet, tính thủ công để bao gồm child packets.
    /// </summary>
    /// <remarks>
    /// Layout:
    ///   - Fixed header    (PacketConstants.HeaderSize)
    ///   - SequenceId      (UInt32 = 4 bytes)
    ///   - TotalCount      (Int32  = 4 bytes)
    ///   - List item-count (Int32  = 4 bytes) ← prefix ghi bởi serializer
    ///   - Mỗi SupplierDto.Length
    /// </remarks>
    [SerializeIgnore]
    public override System.UInt16 Length
    {
        get
        {
            System.Int32 total = PacketConstants.HeaderSize
                + sizeof(System.UInt32)   // SequenceId
                + sizeof(System.Int32)    // TotalCount
                + sizeof(System.Int32);   // list item-count prefix

            for (System.Int32 i = 0; i < Suppliers.Count; i++)
            {
                total += Suppliers[i].Length;
            }

            return (System.UInt16)total;
        }
    }

    /// <summary>
    /// Tổng số nhà cung cấp khớp filter trên server (trước phân trang).
    /// Client dùng để tính TotalPages.
    /// <para>
    /// PHẢI đứng trước <see cref="Suppliers"/> — đây là fixed-size field.
    /// </para>
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 1)]
    public System.Int32 TotalCount { get; set; }

    /// <summary>
    /// Danh sách nhà cung cấp trên trang hiện tại.
    /// Dynamic field — phải đứng CUỐI CÙNG trong SerializeOrder.
    /// </summary>
    [SerializeOrder(PacketHeaderOffset.DATA_REGION + 2)]
    public List<SupplierDto> Suppliers { get; set; } = [];

    /// <summary>Khởi tạo với giá trị mặc định.</summary>
    public SupplierQueryResponse() => OpCode = OpCommand.NONE.AsUInt16();

    /// <inheritdoc/>
    public override void ResetForPool()
    {
        // Trả child packets về pool trước để tránh leak.
        if (Suppliers?.Count > 0)
        {
            var pool = InstanceManager.Instance.GetOrCreateInstance<ObjectPoolManager>();
            for (System.Int32 i = 0; i < Suppliers.Count; i++)
            {
                if (Suppliers[i] is not null)
                {
                    pool.Return(Suppliers[i]);
                }
            }
        }

        Suppliers.Clear();
        SequenceId = 0;
        TotalCount = 0;
        OpCode = OpCommand.NONE.AsUInt16();

        base.ResetForPool();
    }
}