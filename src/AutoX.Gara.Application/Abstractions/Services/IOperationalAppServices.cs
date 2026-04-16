using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Application.Abstractions.Persistence;
using AutoX.Gara.Domain.Entities.Billings;
using AutoX.Gara.Domain.Entities.Inventory;
using AutoX.Gara.Domain.Entities.Invoices;
using AutoX.Gara.Domain.Entities.Repairs;
using AutoX.Gara.Domain.Entities.Suppliers;
using AutoX.Gara.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoX.Gara.Application.Abstractions.Services;

public interface ISupplierAppService
{
    Task<ServiceResult<(List<Supplier> items, int totalCount)>> GetPageAsync(SupplierListQuery query);
    Task<ServiceResult<Supplier>> CreateAsync(Supplier supplier);
    Task<ServiceResult<Supplier>> UpdateAsync(Supplier supplier);
    Task<ServiceResult<bool>> DeleteAsync(int supplierId);
}

public interface IInvoiceAppService
{
    Task<ServiceResult<(List<Invoice> items, int totalCount)>> GetPageAsync(InvoiceListQuery query);
    Task<ServiceResult<Invoice>> CreateAsync(Invoice invoice, int? repairOrderId = null);
    Task<ServiceResult<Invoice>> UpdateAsync(Invoice invoice, int? repairOrderId = null);
    Task<ServiceResult<bool>> DeleteAsync(int invoiceId);
}

public interface IRepairOrderAppService
{
    Task<ServiceResult<(List<RepairOrder> items, int totalCount)>> GetPageAsync(RepairOrderListQuery query);
    Task<ServiceResult<RepairOrder>> CreateAsync(RepairOrder order);
    Task<ServiceResult<RepairOrder>> UpdateAsync(RepairOrder order);
    Task<ServiceResult<bool>> DeleteAsync(int orderId);
}

public interface ITransactionAppService
{
    Task<ServiceResult<(List<Transaction> items, int totalCount)>> GetPageAsync(TransactionListQuery query);
    Task<ServiceResult<Transaction>> CreateAsync(Transaction transaction);
    Task<ServiceResult<bool>> DeleteAsync(int transactionId);
}