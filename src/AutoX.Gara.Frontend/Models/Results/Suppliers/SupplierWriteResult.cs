// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Protocol.Suppliers;
using Nalix.Common.Networking.Protocols;

namespace AutoX.Gara.Frontend.Results.Suppliers;

/// <summary>
/// Result of a supplier write (create/update/change status) operation.
/// </summary>
public sealed class SupplierWriteResult
{
    public System.Boolean IsSuccess { get; private init; }
    public System.String? ErrorMessage { get; private init; }
    public ProtocolAdvice Advice { get; private init; }
    public SupplierDto? UpdatedEntity { get; private init; }

    public static SupplierWriteResult Success(SupplierDto? updatedEntity = null)
        => new() { IsSuccess = true, UpdatedEntity = updatedEntity };

    public static SupplierWriteResult Failure(
        System.String message,
        ProtocolAdvice advice = ProtocolAdvice.FIX_AND_RETRY)
        => new() { IsSuccess = false, ErrorMessage = message, Advice = advice };

    public static SupplierWriteResult Timeout()
        => new()
        {
            IsSuccess = false,
            ErrorMessage = "Yêu c?u h?t th?i gian ch?. Vui lòng th? l?i.",
            Advice = ProtocolAdvice.BACKOFF_RETRY
        };
}
