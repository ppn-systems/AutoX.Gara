using AutoX.Gara.Shared.Enums;
using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Domain.Enums.Transactions;
using Nalix.Common.Networking.Protocols;

namespace AutoX.Gara.Shared.Models;

public sealed record TransactionListQuery(
    int Page,
    int PageSize,
    string SearchTerm,
    TransactionSortField SortBy,
    bool SortDescending,
    int? FilterInvoiceId,
    TransactionType? FilterType,
    TransactionStatus? FilterStatus,
    PaymentMethod? FilterPaymentMethod,
    decimal? FilterMinAmount,
    decimal? FilterMaxAmount,
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

        if (FilterMinAmount.HasValue && FilterMaxAmount.HasValue && FilterMinAmount.Value > FilterMaxAmount.Value)
        {
            throw new ArgumentException("FilterMinAmount must be less than or equal to FilterMaxAmount.");
        }

        if (FilterFromDate.HasValue && FilterToDate.HasValue && FilterFromDate.Value > FilterToDate.Value)
        {
            throw new ArgumentException("FilterFromDate must be less than or equal to FilterToDate.");
        }
    }
}
