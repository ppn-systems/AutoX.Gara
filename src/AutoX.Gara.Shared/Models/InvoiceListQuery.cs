// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Shared.Enums;

namespace AutoX.Gara.Shared.Models;

/// <summary>
/// Value object representing query parameters for retrieving a paginated list of invoices.
/// </summary>
public sealed record InvoiceListQuery(
    System.Int32 Page,
    System.Int32 PageSize,
    System.String SearchTerm,
    InvoiceSortField SortBy,
    System.Boolean SortDescending,
    System.Int32? FilterCustomerId,
    PaymentStatus? FilterPaymentStatus,
    System.DateTime? FilterFromDate,
    System.DateTime? FilterToDate)
{
    public void Validate()
    {
        if (Page < 1)
        {
            throw new System.ArgumentException("Page must be at least 1.");
        }

        if (PageSize < 1)
        {
            throw new System.ArgumentException("PageSize must be at least 1.");
        }

        if (FilterFromDate.HasValue && FilterToDate.HasValue && FilterFromDate.Value > FilterToDate.Value)
        {
            throw new System.ArgumentException("FilterFromDate must be less than or equal to FilterToDate.");
        }
    }
}

