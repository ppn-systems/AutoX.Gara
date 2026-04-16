using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Protocol.Repairs;

using Nalix.Common.Networking.Protocols;

namespace AutoX.Gara.Frontend.Models.Results.Billings;

public sealed class RepairOrderItemWriteResult

{
    public bool IsSuccess { get; private init; }

    public string? ErrorMessage { get; private init; }

    public ProtocolAdvice Advice { get; private init; }

    public RepairOrderItemDto? UpdatedEntity { get; private init; }

    public static RepairOrderItemWriteResult Success(RepairOrderItemDto? updatedEntity = null)

        => new() { IsSuccess = true, UpdatedEntity = updatedEntity };

    public static RepairOrderItemWriteResult Failure(string message, ProtocolAdvice advice = ProtocolAdvice.FIX_AND_RETRY)

        => new() { IsSuccess = false, ErrorMessage = message, Advice = advice };

    public static RepairOrderItemWriteResult Timeout()

        => new()

        {
            IsSuccess = false,

            ErrorMessage = "Y�u c?u h?t th?i gian ch?. Vui l�ng th? l?i.",

            Advice = ProtocolAdvice.BACKOFF_RETRY

        };
}