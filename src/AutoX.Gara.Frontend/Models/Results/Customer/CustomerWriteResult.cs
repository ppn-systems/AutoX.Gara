// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Shared.Protocol.Customers;
using Nalix.Common.Networking.Protocols;
namespace AutoX.Gara.Frontend.Models.Results.Customer;
/// <summary>
/// Represents the result of a single customer write operation (create / update / delete).
/// </summary>
public sealed class CustomerWriteResult
{
    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    public bool IsSuccess { get; private init; }
    /// <summary>Gets the error message if the operation failed.</summary>
    public string? ErrorMessage { get; private init; }
    /// <summary>Gets the protocol advice for error handling on failure.</summary>
    public ProtocolAdvice Advice { get; private init; }
    /// <summary>
    /// The server-confirmed entity returned after a successful create or update.
    /// Enables optimistic UI updates without a full list reload.
    /// <c>null</c> on delete or when the server does not echo back the entity.
    /// </summary>
    public CustomerDto? UpdatedEntity { get; private init; }
    // --- Factory Methods -----------------------------------------------------
    /// <summary>Creates a successful write result, optionally carrying the server-confirmed entity.</summary>
    public static CustomerWriteResult Success(CustomerDto? updatedEntity = null)
        => new() { IsSuccess = true, UpdatedEntity = updatedEntity };
    /// <summary>Creates a failure write result with the given error message and advice.</summary>
    public static CustomerWriteResult Failure(
        string message,
        ProtocolAdvice advice = ProtocolAdvice.FIX_AND_RETRY)
        => new() { IsSuccess = false, ErrorMessage = message, Advice = advice };
    /// <summary>Creates a timeout failure write result.</summary>
    public static CustomerWriteResult Timeout()
        => new()
        {
            IsSuccess = false,
            ErrorMessage = "Y�u c?u h?t th?i gian ch?. Vui l�ng th? l?i.",
            Advice = ProtocolAdvice.BACKOFF_RETRY
        };
}
