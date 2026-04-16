using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Common.Networking.Protocols;
using AutoX.Gara.Shared.Enums;

namespace AutoX.Gara.Shared.Extensions;

public static class CommandExtensions
{
    /// <summary>
    /// Chuy?n m?t gi� tr? enum OpCommand sang ushort.
    /// </summary>
    /// <param name="command">Gi� tr? enum OpCommand.</param>
    /// <returns>Gi� tr? ushort tuong ?ng.</returns>
    public static System.UInt16 AsUInt16(this OpCommand command) => (System.UInt16)command;
}