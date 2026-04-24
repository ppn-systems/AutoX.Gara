// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Protocol.Invoices;

using Nalix.Common.Networking.Protocols;

namespace AutoX.Gara.Frontend.Models.Results.Billings;

public sealed class RepairOrderWriteResult

{
    public bool IsSuccess { get; private init; }

    public string? ErrorMessage { get; private init; }

    public ProtocolAdvice Advice { get; private init; }

    public RepairOrderDto? UpdatedEntity { get; private init; }

    public static RepairOrderWriteResult Success(RepairOrderDto? updatedEntity = null)

        => new() { IsSuccess = true, UpdatedEntity = updatedEntity };

    public static RepairOrderWriteResult Failure(

        string message,

        ProtocolAdvice advice = ProtocolAdvice.FIX_AND_RETRY)

        => new() { IsSuccess = false, ErrorMessage = message, Advice = advice };

    public static RepairOrderWriteResult Timeout()

        => new()

        {
            IsSuccess = false,

            ErrorMessage = "Y�u c?u h?t th?i gian ch?. Vui l�ng th? l?i.",

            Advice = ProtocolAdvice.BACKOFF_RETRY

        };
}
