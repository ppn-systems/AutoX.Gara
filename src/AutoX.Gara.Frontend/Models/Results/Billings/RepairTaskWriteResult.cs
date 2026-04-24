// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Contracts.Protocol.Repairs;
using Nalix.Common.Networking.Protocols;
namespace AutoX.Gara.Frontend.Models.Results.Billings;
public sealed class RepairTaskWriteResult
{
    public bool IsSuccess { get; private init; }
    public string? ErrorMessage { get; private init; }
    public ProtocolAdvice Advice { get; private init; }
    public RepairTaskDto? UpdatedEntity { get; private init; }
    public static RepairTaskWriteResult Success(RepairTaskDto? updatedEntity = null)
        => new() { IsSuccess = true, UpdatedEntity = updatedEntity };
    public static RepairTaskWriteResult Failure(string message, ProtocolAdvice advice = ProtocolAdvice.FIX_AND_RETRY)
        => new() { IsSuccess = false, ErrorMessage = message, Advice = advice };
    public static RepairTaskWriteResult Timeout()
        => new()
        {
            IsSuccess = false,
            ErrorMessage = "Y�u c?u h?t th?i gian ch?. Vui l�ng th? l?i.",
            Advice = ProtocolAdvice.BACKOFF_RETRY
        };
}

