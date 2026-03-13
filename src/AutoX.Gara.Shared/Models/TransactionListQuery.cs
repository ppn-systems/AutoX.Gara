// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Domain.Enums.Transactions;
using AutoX.Gara.Shared.Enums;

namespace AutoX.Gara.Shared.Models;

public sealed record TransactionListQuery(
    System.Int32 Page,
    System.Int32 PageSize,
    System.String SearchTerm,
    TransactionSortField SortBy,
    System.Boolean SortDescending,
    System.Int32? FilterInvoiceId,
    TransactionType? FilterType,
    TransactionStatus? FilterStatus,
    PaymentMethod? FilterPaymentMethod,
    System.Decimal? FilterMinAmount,
    System.Decimal? FilterMaxAmount,
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

        if (FilterMinAmount.HasValue && FilterMaxAmount.HasValue && FilterMinAmount.Value > FilterMaxAmount.Value)
        {
            throw new System.ArgumentException("FilterMinAmount must be less than or equal to FilterMaxAmount.");
        }

        if (FilterFromDate.HasValue && FilterToDate.HasValue && FilterFromDate.Value > FilterToDate.Value)
        {
            throw new System.ArgumentException("FilterFromDate must be less than or equal to FilterToDate.");
        }
    }
}

