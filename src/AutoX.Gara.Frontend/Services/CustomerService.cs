// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Customers;
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

    // ─── GetListAsync ─────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async System.Threading.Tasks.Task<CustomerListResult> GetListAsync(
        System.Int32 page,
        System.Int32 pageSize,
        System.String? searchTerm = null,
        CustomerSortField sortBy = CustomerSortField.CreatedAt,
        System.Boolean sortDescending = true,
        CustomerType filterType = CustomerType.None,
        MembershipLevel filterMembership = MembershipLevel.None,
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
                SearchTerm = searchTerm ?? System.String.Empty,
                SortBy = sortBy,
                SortDescending = sortDescending,
                FilterType = filterType,
                FilterMembership = filterMembership,
                OpCode = (System.UInt16)OpCommand.CUSTOMER_LIST
            };

            System.Threading.Tasks.TaskCompletionSource<CustomerListResult> tcs =
                new(System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);

            System.IDisposable? sub = null;
            System.IDisposable? errSub = null;

            sub = client.OnOnce<CustomersPacket>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    sub?.Dispose();
                    errSub?.Dispose();

                    System.Boolean hasMore = resp.Customers.Count == pageSize;
                    tcs.TrySetResult(CustomerListResult.Success(
                        resp.Customers,
                        totalCount: resp.TotalCount,
                        hasMore: hasMore));
                });

            errSub = client.OnOnce<Directive>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    sub?.Dispose();
                    errSub?.Dispose();
                    tcs.TrySetResult(CustomerListResult.Failure(MapErrorReason(resp.Reason), resp.Action));
                });

            await client.SendAsync(packet, ct).ConfigureAwait(false);

            using System.Threading.CancellationTokenSource cts =
                System.Threading.CancellationTokenSource.CreateLinkedTokenSource(ct);

            System.Threading.Tasks.Task timeoutTask =
                System.Threading.Tasks.Task.Delay(RequestTimeoutMs, cts.Token);

            System.Threading.Tasks.Task winner =
                await System.Threading.Tasks.Task.WhenAny(tcs.Task, timeoutTask).ConfigureAwait(false);

            cts.Cancel(); // Hủy timeout task để tránh fire sau khi done

            if (!ReferenceEquals(winner, tcs.Task))
            {
                sub?.Dispose();
                errSub?.Dispose();
                return CustomerListResult.Timeout();
            }

            return await tcs.Task.ConfigureAwait(false);
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
        => SendWritePacketAsync((System.UInt16)OpCommand.CUSTOMER_CREATE, data, expectEcho: true, ct);

    // ─── UpdateAsync ──────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public System.Threading.Tasks.Task<CustomerWriteResult> UpdateAsync(
        CustomerDataPacket data,
        System.Threading.CancellationToken ct = default)
        => SendWritePacketAsync((System.UInt16)OpCommand.CUSTOMER_UPDATE, data, expectEcho: true, ct);

    // ─── DeleteAsync ──────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public System.Threading.Tasks.Task<CustomerWriteResult> DeleteAsync(
        CustomerDataPacket data,
        System.Threading.CancellationToken ct = default)
        => SendWritePacketAsync((System.UInt16)OpCommand.CUSTOMER_DELETE, data, expectEcho: false, ct);

    // ─── Private Helpers ─────────────────────────────────────────────────────

    /// <summary>
    /// Generic send-and-await cho create/update/delete.
    /// <para>
    /// Khi <paramref name="expectEcho"/> = <c>true</c> (create/update), server sẽ
    /// echo lại <see cref="CustomerDataPacket"/> với Id và timestamps đã được DB xác nhận.
    /// ViewModel dùng dữ liệu này để update optimistic UI mà không cần reload toàn bộ list.
    /// </para>
    /// <para>
    /// Khi <paramref name="expectEcho"/> = <c>false</c> (delete), server chỉ trả về
    /// <see cref="Directive"/> NONE để xác nhận thành công.
    /// </para>
    /// </summary>
    private static async System.Threading.Tasks.Task<CustomerWriteResult> SendWritePacketAsync(
        System.UInt16 opcode,
        CustomerDataPacket data,
        System.Boolean expectEcho,
        System.Threading.CancellationToken ct)
    {
        try
        {
            System.UInt32 sq = Csprng.NextUInt32();
            ReliableClient client = InstanceManager.Instance.GetOrCreateInstance<ReliableClient>();

            data.OpCode = opcode;
            data.SequenceId = sq;

            ILogger logger = InstanceManager.Instance.GetOrCreateInstance<ILogger>();
            logger.Info($"Sending packet SeqId={sq} OpCode={opcode} expectEcho={expectEcho}");

            CustomerDataPacket.Encrypt(data, client.Options.EncryptionKey, CipherSuiteType.SALSA20);

            System.Threading.Tasks.TaskCompletionSource<CustomerWriteResult> tcs =
                new(System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);

            System.IDisposable? echoSub = null;
            System.IDisposable? errSub = null;

            if (expectEcho)
            {
                // Server echo lại CustomerDataPacket sau khi lưu thành công
                echoSub = client.OnOnce<CustomerDataPacket>(
                    predicate: p => p.SequenceId == sq,
                    handler: confirmed =>
                    {
                        echoSub?.Dispose();
                        errSub?.Dispose();
                        tcs.TrySetResult(CustomerWriteResult.Success(confirmed));
                    });
            }

            // Luôn lắng nghe Directive để bắt lỗi (và success khi delete)
            errSub = client.OnOnce<Directive>(
                predicate: p => p.SequenceId == sq,
                handler: resp =>
                {
                    echoSub?.Dispose();
                    errSub?.Dispose();

                    CustomerWriteResult result = resp.Type == ControlType.NONE
                        ? CustomerWriteResult.Success()          // Delete thành công
                        : CustomerWriteResult.Failure(MapErrorReason(resp.Reason), resp.Action);

                    tcs.TrySetResult(result);
                });

            await client.SendAsync(data, ct).ConfigureAwait(false);

            using System.Threading.CancellationTokenSource cts =
                System.Threading.CancellationTokenSource.CreateLinkedTokenSource(ct);

            System.Threading.Tasks.Task timeoutTask =
                System.Threading.Tasks.Task.Delay(RequestTimeoutMs, cts.Token);

            System.Threading.Tasks.Task winner =
                await System.Threading.Tasks.Task.WhenAny(tcs.Task, timeoutTask).ConfigureAwait(false);

            cts.Cancel(); // Hủy timeout task để tránh fire sau khi done

            if (!ReferenceEquals(winner, tcs.Task))
            {
                echoSub?.Dispose();
                errSub?.Dispose();
                return CustomerWriteResult.Timeout();
            }

            return await tcs.Task.ConfigureAwait(false);
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