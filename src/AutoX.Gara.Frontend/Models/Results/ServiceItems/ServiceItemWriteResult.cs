// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Shared.Protocol.Billings;
using Nalix.Common.Networking.Protocols;
namespace AutoX.Gara.Frontend.Models.Results.ServiceItems;
/// <summary>
/// Result of a service item write (create/update/delete) operation.
/// </summary>
public sealed class ServiceItemWriteResult
{
    public bool IsSuccess { get; private init; }
    public string? ErrorMessage { get; private init; }
    public ProtocolAdvice Advice { get; private init; }
    public ServiceItemDto? UpdatedEntity { get; private init; }
    public static ServiceItemWriteResult Success(ServiceItemDto? updatedEntity = null)
        => new() { IsSuccess = true, UpdatedEntity = updatedEntity };
    public static ServiceItemWriteResult Failure(
        string message,
        ProtocolAdvice advice = ProtocolAdvice.FIX_AND_RETRY)
        => new() { IsSuccess = false, ErrorMessage = message, Advice = advice };
    public static ServiceItemWriteResult Timeout()
        => new()
        {
            IsSuccess = false,
            ErrorMessage = "Y�u c?u h?t th?i gian ch?. Vui l�ng th? l?i.",
            Advice = ProtocolAdvice.BACKOFF_RETRY
        };
}
