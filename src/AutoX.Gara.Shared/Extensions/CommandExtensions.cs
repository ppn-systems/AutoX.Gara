using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Common.Networking.Protocols;
using AutoX.Gara.Shared.Enums;

namespace AutoX.Gara.Shared.Extensions;

public static class CommandExtensions
{
    /// <summary>
    /// Chuy?n m?t giï¿½ tr? enum OpCommand sang ushort.
    /// </summary>
    /// <param name="command">Giï¿½ tr? enum OpCommand.</param>
    /// <returns>Giï¿½ tr? ushort tuong ?ng.</returns>
    public static System.UInt16 AsUInt16(this OpCommand command) => (System.UInt16)command;
}
