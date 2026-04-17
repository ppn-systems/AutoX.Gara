using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AutoX.Gara.Frontend.Messages;

/// <summary>

/// Broadcast when a Transaction write changes Invoice totals (BalanceDue/AmountPaid).

/// Payload: InvoiceId.

/// </summary>

public sealed class InvoiceTotalsChangedMessage(int invoiceId) : ValueChangedMessage<int>(invoiceId);
