// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Packets.Customers;
using Nalix.Common.Networking.Protocols;

namespace AutoX.Gara.Frontend.ViewModels.Results;

/// <summary>
/// Represents the result of a customer list query operation.
/// </summary>
public sealed class CustomerListResult
{
    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    public System.Boolean IsSuccess { get; private init; }

    /// <summary>Gets the error message if the operation failed.</summary>
    public System.String? ErrorMessage { get; private init; }

    /// <summary>Gets the protocol advice for error handling.</summary>
    public ProtocolAdvice Advice { get; private init; }

    /// <summary>Gets the list of customer data packets returned from the server.</summary>
    public System.Collections.Generic.List<CustomerDataPacket> Customers { get; private init; } = [];

    /// <summary>Creates a successful result with the given customer list.</summary>
    public static CustomerListResult Success(System.Collections.Generic.List<CustomerDataPacket> customers)
        => new() { IsSuccess = true, Customers = customers };

    /// <summary>Creates a failure result with the given error message and advice.</summary>
    public static CustomerListResult Failure(System.String message, ProtocolAdvice advice = ProtocolAdvice.FIX_AND_RETRY)
        => new() { IsSuccess = false, ErrorMessage = message, Advice = advice };

    /// <summary>Creates a timeout failure result.</summary>
    public static CustomerListResult Timeout()
        => new() { IsSuccess = false, ErrorMessage = "Yêu cầu hết thời gian chờ. Vui lòng thử lại.", Advice = ProtocolAdvice.BACKOFF_RETRY };
}