// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Abstractions;
using AutoX.Gara.Frontend.ViewModels.Results;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Packets.Customers;
using Nalix.Common.Diagnostics.Abstractions;
using Nalix.Common.Networking.Protocols;
using Nalix.Common.Security.Enums;
using Nalix.Framework.Injection;
using Nalix.Framework.Random;
using Nalix.SDK.Transport;
using Nalix.SDK.Transport.Extensions;
using Nalix.Shared.Frames.Controls;

namespace AutoX.Gara.Frontend.Services;

/// <summary>
/// Real implementation of <see cref="ICustomerService"/>.
/// All network I/O is encapsulated here; the ViewModel has no knowledge of ReliableClient.
/// </summary>
public sealed class CustomerService : ICustomerService
{
    private const System.Int32 RequestTimeoutMs = 8_000;

    // ─── GetListAsync ─────────────────���───────────────────────────────────────

    /// <inheritdoc/>
    public async System.Threading.Tasks.Task<CustomerListResult> GetListAsync(
        System.Int32 page,
        System.Int32 pageSize,
        System.Threading.CancellationToken ct = default)
    {
        try
        {
            System.UInt32 sq = Csprng.NextUInt32();
            ReliableClient client = InstanceManager.Instance.GetOrCreateInstance<ReliableClient>();

            CustomersQueryPacket packet = new()
            {
                Page = page,
                SequenceId = sq,
                PageSize = pageSize,
                OpCode = (System.UInt16)OpCommand.CUSTOMER_LIST
            };

            System.Threading.Tasks.TaskCompletionSource<CustomerListResult> tcs = new(System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);

            System.IDisposable? sub = null;
            System.IDisposable? errSub = null;

            sub = client.OnOnce<CustomersPacket>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    System.Diagnostics.Debug.WriteLine("[CLIENT DEBUG] Nhận CustomersPacket từ server với seqid=" + sq);
                    System.Diagnostics.Debug.WriteLine("[CLIENT DEBUG] Số lượng customer: " + resp.Customers.Count);
                    sub?.Dispose();
                    errSub?.Dispose();
                    tcs.TrySetResult(CustomerListResult.Success(resp.Customers));
                });

            // Also handle error directive
            errSub = client.OnOnce<Directive>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    System.Diagnostics.Debug.WriteLine($"[CLIENT DEBUG] Nhận Directive ERROR từ server: {resp.Reason} ({resp.Action}), seqid={sq}");
                    sub?.Dispose();
                    errSub?.Dispose();
                    tcs.TrySetResult(CustomerListResult.Failure(MapErrorReason(resp.Reason), resp.Action));
                });

            await client.SendAsync(packet, ct);

            using System.Threading.CancellationTokenSource cts = System.Threading.CancellationTokenSource.CreateLinkedTokenSource(ct);

            System.Threading.Tasks.Task timeoutTask = System.Threading.Tasks.Task.Delay(RequestTimeoutMs, cts.Token);

            System.Threading.Tasks.Task winner = await System.Threading.Tasks.Task.WhenAny(tcs.Task, timeoutTask);

            if (winner != tcs.Task)
            {
                sub?.Dispose();
                errSub?.Dispose();
                return CustomerListResult.Timeout();
            }

            return await tcs.Task;
        }
        catch (System.OperationCanceledException)
        {
            return CustomerListResult.Failure("Yêu cầu bị hủy.", ProtocolAdvice.NONE);
        }
        catch (System.Exception ex)
        {
            LogException(ex);
            return CustomerListResult.Failure($"Lỗi không xác định: {ex.Message}", ProtocolAdvice.DO_NOT_RETRY);
        }
    }

    // ─── CreateAsync ──────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public System.Threading.Tasks.Task<CustomerWriteResult> CreateAsync(
        CustomerDataPacket data,
        System.Threading.CancellationToken ct = default)
        => SendWritePacketAsync((System.UInt16)OpCommand.CUSTOMER_CREATE, data, ct);

    // ─── UpdateAsync ──────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public System.Threading.Tasks.Task<CustomerWriteResult> UpdateAsync(
        CustomerDataPacket data,
        System.Threading.CancellationToken ct = default)
        => SendWritePacketAsync((System.UInt16)OpCommand.CUSTOMER_UPDATE, data, ct);

    // ─── DeleteAsync ──────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public System.Threading.Tasks.Task<CustomerWriteResult> DeleteAsync(
        CustomerDataPacket data,
        System.Threading.CancellationToken ct = default)
        => SendWritePacketAsync((System.UInt16)OpCommand.CUSTOMER_DELETE, data, ct);

    // ─── Private Helpers ─────────────────────────────────────────────────────

    /// <summary>
    /// Generic send-and-await for create/update/delete operations that return a <see cref="Directive"/>.
    /// </summary>
    private static async System.Threading.Tasks.Task<CustomerWriteResult> SendWritePacketAsync(
        System.UInt16 opcode,
        CustomerDataPacket data,
        System.Threading.CancellationToken ct)
    {
        try
        {
            System.UInt32 sq = Csprng.NextUInt32();
            ReliableClient client = InstanceManager.Instance.GetOrCreateInstance<ReliableClient>();

            data.OpCode = opcode;
            data.SequenceId = sq;

            InstanceManager.Instance.GetOrCreateInstance<ILogger>().Info($"Sending packet with SeqId={sq} OpCode={opcode}");

            CustomerDataPacket.Encrypt(data, client.Options.EncryptionKey, CipherSuiteType.SALSA20);

            System.Threading.Tasks.TaskCompletionSource<CustomerWriteResult> tcs = new(
                System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);

            System.IDisposable? sub = null;
            sub = client.OnOnce<Directive>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    sub?.Dispose();

                    CustomerWriteResult result = resp.Type == ControlType.NONE
                        ? CustomerWriteResult.Success()
                        : CustomerWriteResult.Failure(MapErrorReason(resp.Reason), resp.Action);

                    tcs.TrySetResult(result);
                });

            await client.SendAsync(data, ct);

            using System.Threading.CancellationTokenSource cts =
                System.Threading.CancellationTokenSource.CreateLinkedTokenSource(ct);

            System.Threading.Tasks.Task timeoutTask =
                System.Threading.Tasks.Task.Delay(RequestTimeoutMs, cts.Token);

            System.Threading.Tasks.Task winner =
                await System.Threading.Tasks.Task.WhenAny(tcs.Task, timeoutTask);

            if (winner != tcs.Task)
            {
                sub?.Dispose();
                return CustomerWriteResult.Timeout();
            }

            return await tcs.Task;
        }
        catch (System.OperationCanceledException)
        {
            return CustomerWriteResult.Failure("Yêu cầu bị hủy.", ProtocolAdvice.NONE);
        }
        catch (System.Exception ex)
        {
            LogException(ex);
            return CustomerWriteResult.Failure($"Lỗi không xác định: {ex.Message}", ProtocolAdvice.DO_NOT_RETRY);
        }
    }

    private static System.String MapErrorReason(ProtocolReason reason)
        => reason switch
        {
            ProtocolReason.NOT_FOUND => "Không tìm thấy khách hàng.",
            ProtocolReason.ALREADY_EXISTS => "Email hoặc số điện thoại đã tồn tại.",
            ProtocolReason.MALFORMED_PACKET => "Dữ liệu không hợp lệ.",
            ProtocolReason.INTERNAL_ERROR => "Lỗi hệ thống. Vui lòng thử lại sau.",
            ProtocolReason.FORBIDDEN => "Bạn không có quyền thực hiện thao tác này.",
            _ => "Thao tác thất bại. Vui lòng thử lại."
        };

    private static void LogException(System.Exception ex)
    {
        ILogger logger = InstanceManager.Instance.GetOrCreateInstance<ILogger>();
        logger.Error(ex.ToString());
        if (ex.InnerException is not null)
        {
            logger.Error("Inner: " + ex.InnerException);
        }
    }
}