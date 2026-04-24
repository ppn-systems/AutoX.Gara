#nullable enable

// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Common.Networking.Protocols;

namespace AutoX.Gara.Shared.Models;

/// <summary>
/// Kết quả chung cho các tác vụ nghiệp vụ tại Application Layer.
/// </summary>
public record ServiceResult<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public string? ErrorMessage { get; init; }
    public ProtocolReason Reason { get; init; } = ProtocolReason.NONE;

    public static ServiceResult<T> Success(T data) => new() { IsSuccess = true, Data = data };



    public static ServiceResult<T> Failure(string message, ProtocolReason reason = ProtocolReason.INTERNAL_ERROR)

        => new() { IsSuccess = false, ErrorMessage = message, Reason = reason };
}

public record AuthData(string Username, string Role);
