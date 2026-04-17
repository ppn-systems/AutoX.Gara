using AutoX.Gara.Shared.Enums;
using System;
using System.Collections.Generic;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Payments;
using Nalix.Common.Networking.Protocols;

namespace AutoX.Gara.Shared.Models;

/// <summary>
/// Value object representing query parameters for retrieving a paginated list of invoices.
/// </summary>
public sealed record InvoiceListQuery(
    int Page,
    int PageSize,
    string SearchTerm,
    InvoiceSortField SortBy,
    bool SortDescending,
    int? FilterCustomerId,
    PaymentStatus? FilterPaymentStatus,
    DateTime? FilterFromDate,
    DateTime? FilterToDate)
{
    public void Validate()
    {
        if (Page < 1)
        {
            throw new ArgumentException("Page must be at least 1.");
        }

        if (PageSize < 1)
        {
            throw new ArgumentException("PageSize must be at least 1.");
        }

        if (FilterFromDate.HasValue && FilterToDate.HasValue && FilterFromDate.Value > FilterToDate.Value)
        {
            throw new ArgumentException("FilterFromDate must be less than or equal to FilterToDate.");
        }
    }
}
