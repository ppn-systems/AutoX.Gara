// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Application.Abstractions.Persistence;
using AutoX.Gara.Domain.Entities.Billings;
using AutoX.Gara.Domain.Entities.Customers;
using AutoX.Gara.Domain.Entities.Invoices;
using AutoX.Gara.Domain.Enums.Transactions;
using AutoX.Gara.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nalix.Common.Networking.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoX.Gara.Application.Billings;

public sealed class InvoiceAppService(IDataSessionFactory dataSessionFactory, ILogger<InvoiceAppService> logger)
{
    private readonly IDataSessionFactory _dataSessionFactory = dataSessionFactory ?? throw new ArgumentNullException(nameof(dataSessionFactory));
    private readonly ILogger<InvoiceAppService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<ServiceResult<(List<Invoice> items, int totalCount)>> GetPageAsync(InvoiceListQuery query)
    {
        try
        {
            await using var session = _dataSessionFactory.Create();
            var result = await session.Invoices.GetPageAsync(query).ConfigureAwait(false);
            return ServiceResult<(List<Invoice> items, int totalCount)>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting invoice page.");
            return ServiceResult<(List<Invoice> items, int totalCount)>.Failure("Lỗi khi lấy danh sách hóa đơn.");
        }
    }

    public async Task<ServiceResult<Invoice>> CreateAsync(Invoice invoice, int? repairOrderId = null)
    {
        var validation = ValidateInvoicePayload(invoice);
        if (!validation.IsSuccess)
        {
            return ServiceResult<Invoice>.Failure(validation.ErrorMessage!, validation.Reason);
        }

        try
        {
            await using var session = _dataSessionFactory.Create();

            bool customerExists = await session.Context.Set<Customer>()
                .AsNoTracking()
                .AnyAsync(c => c.Id == invoice.CustomerId)
                .ConfigureAwait(false);
            if (!customerExists)
            {
                return ServiceResult<Invoice>.Failure("Không tìm thấy khách hàng của hóa đơn.", ProtocolReason.NOT_FOUND);
            }


            if (await session.Invoices.ExistsByInvoiceNumberAsync(invoice.InvoiceNumber).ConfigureAwait(false))
            {
                return ServiceResult<Invoice>.Failure("Số hóa đơn đã tồn tại.", ProtocolReason.ALREADY_EXISTS);
            }

            await using var tx = await session.BeginTransactionAsync().ConfigureAwait(false);
            try

            {
                await session.Invoices.AddAsync(invoice).ConfigureAwait(false);
                await session.SaveChangesAsync().ConfigureAwait(false);

                if (repairOrderId.HasValue && repairOrderId.Value > 0)
                {
                    var linkResult = await LinkRepairOrderInternal(session, invoice.Id, repairOrderId.Value, invoice.CustomerId).ConfigureAwait(false);
                    if (!linkResult.IsSuccess)
                    {
                        await tx.RollbackAsync().ConfigureAwait(false);
                        return ServiceResult<Invoice>.Failure(linkResult.ErrorMessage, linkResult.Reason);
                    }
                }

                await session.SaveChangesAsync().ConfigureAwait(false);



                // Recalculate contextually

                var withDetails = await session.Invoices.GetInvoiceWithFullGraphTrackedAsync(invoice.Id).ConfigureAwait(false);
                if (withDetails != null)
                {
                    withDetails.Recalculate();
                    await session.SaveChangesAsync().ConfigureAwait(false);
                }

                await tx.CommitAsync().ConfigureAwait(false);
                return ServiceResult<Invoice>.Success(withDetails ?? invoice);
            }
            catch (Exception)
            {
                await tx.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating invoice.");
            return ServiceResult<Invoice>.Failure("Lỗi khi tạo hóa đơn mới.");
        }
    }

    public async Task<ServiceResult<Invoice>> UpdateAsync(Invoice invoice, int? repairOrderId = null)
    {
        var validation = ValidateInvoicePayload(invoice);
        if (!validation.IsSuccess)
        {
            return ServiceResult<Invoice>.Failure(validation.ErrorMessage!, validation.Reason);
        }

        try
        {
            await using var session = _dataSessionFactory.Create();
            var repo = session.Invoices;

            var existing = await repo.GetInvoiceWithFullGraphTrackedAsync(invoice.Id).ConfigureAwait(false);
            if (existing is null)
            {
                return ServiceResult<Invoice>.Failure("Không tìm thấy hóa đơn.", ProtocolReason.NOT_FOUND);
            }

            bool customerExists = await session.Context.Set<Customer>()
                .AsNoTracking()
                .AnyAsync(c => c.Id == invoice.CustomerId)
                .ConfigureAwait(false);
            if (!customerExists)
            {
                return ServiceResult<Invoice>.Failure("Không tìm thấy khách hàng của hóa đơn.", ProtocolReason.NOT_FOUND);
            }

            if (!string.Equals(existing.InvoiceNumber, invoice.InvoiceNumber, StringComparison.Ordinal))
            {
                if (await repo.ExistsByInvoiceNumberAsync(invoice.InvoiceNumber, existing.Id).ConfigureAwait(false))
                {
                    return ServiceResult<Invoice>.Failure("Số hóa đơn mới đã tồn tại.", ProtocolReason.ALREADY_EXISTS);
                }
            }

            existing.CustomerId = invoice.CustomerId;
            existing.InvoiceNumber = invoice.InvoiceNumber;
            existing.InvoiceDate = invoice.InvoiceDate;
            existing.PaymentStatus = invoice.PaymentStatus;
            existing.TaxRate = invoice.TaxRate;
            existing.DiscountType = invoice.DiscountType;
            existing.Discount = invoice.Discount;
            existing.Notes = invoice.Notes;

            if (repairOrderId.HasValue && repairOrderId.Value > 0)
            {
                var linkResult = await LinkRepairOrderInternal(session, existing.Id, repairOrderId.Value, existing.CustomerId).ConfigureAwait(false);
                if (!linkResult.IsSuccess)
                {
                    return ServiceResult<Invoice>.Failure(linkResult.ErrorMessage, linkResult.Reason);
                }
            }

            existing.Recalculate();
            repo.Update(existing);
            await repo.SaveChangesAsync().ConfigureAwait(false);

            return ServiceResult<Invoice>.Success(existing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating invoice {Id}.", invoice.Id);
            return ServiceResult<Invoice>.Failure("Lỗi khi cập nhật hóa đơn.");
        }
    }

    private async Task<ServiceResult<bool>> LinkRepairOrderInternal(IDataSession session, int invoiceId, int repairOrderId, int customerId)
    {
        var ro = await session.Context.Set<RepairOrder>()
            .FirstOrDefaultAsync(r => r.Id == repairOrderId).ConfigureAwait(false);

        if (ro == null)
        {
            return ServiceResult<bool>.Failure("Không tìm thấy lệnh sửa chữa.", ProtocolReason.NOT_FOUND);
        }

        if (ro.CustomerId != customerId)
        {
            return ServiceResult<bool>.Failure("Lệnh sửa chữa không thuộc về khách hàng này.", ProtocolReason.VALIDATION_FAILED);
        }

        if (ro.InvoiceId.HasValue && ro.InvoiceId.Value != invoiceId)
        {
            return ServiceResult<bool>.Failure("Lệnh sửa chữa đã được gắn vào hóa đơn khác.", ProtocolReason.ALREADY_EXISTS);
        }

        ro.InvoiceId = invoiceId;
        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int invoiceId)
    {
        try
        {
            await using var session = _dataSessionFactory.Create();
            var existing = await session.Invoices.GetInvoiceWithFullGraphTrackedAsync(invoiceId).ConfigureAwait(false);
            if (existing is null)
            {
                return ServiceResult<bool>.Failure("Không tìm thấy hóa đơn.", ProtocolReason.NOT_FOUND);
            }

            bool hasPaidTransaction = existing.Transactions != null
                && existing.Transactions.Any(t => t.Status == TransactionStatus.Completed && !t.IsReversed);
            if (hasPaidTransaction)
            {
                return ServiceResult<bool>.Failure("Không thể xóa hóa đơn đã có giao dịch thanh toán.", ProtocolReason.VALIDATION_FAILED);
            }

            session.Invoices.Delete(existing);
            await session.Invoices.SaveChangesAsync().ConfigureAwait(false);

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting invoice {Id}.", invoiceId);
            return ServiceResult<bool>.Failure("Lỗi khi xóa hóa đơn.");
        }
    }

    private static ServiceResult<bool> ValidateInvoicePayload(Invoice invoice)
    {
        if (invoice is null || invoice.CustomerId <= 0 || string.IsNullOrWhiteSpace(invoice.InvoiceNumber))
        {
            return ServiceResult<bool>.Failure("Dữ liệu hóa đơn không hợp lệ.", ProtocolReason.MALFORMED_PACKET);
        }

        if (invoice.DiscountType == AutoX.Gara.Domain.Enums.DiscountType.Percentage
            && (invoice.Discount < 0 || invoice.Discount > 100))
        {
            return ServiceResult<bool>.Failure("Giảm giá phần trăm phải trong khoảng 0-100.", ProtocolReason.VALIDATION_FAILED);
        }

        if (invoice.DiscountType != AutoX.Gara.Domain.Enums.DiscountType.Percentage && invoice.Discount < 0)
        {
            return ServiceResult<bool>.Failure("Giảm giá không được âm.", ProtocolReason.VALIDATION_FAILED);
        }

        return ServiceResult<bool>.Success(true);
    }
}


