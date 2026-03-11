// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Enums;

namespace AutoX.Gara.Shared.Extensions;

public static class CommandExtensions
{
    /// <summary>
    /// Chuyển một giá trị enum OpCommand sang ushort.
    /// </summary>
    /// <param name="command">Giá trị enum OpCommand.</param>
    /// <returns>Giá trị ushort tương ứng.</returns>
    public static System.UInt16 AsUInt16(this OpCommand command) => (System.UInt16)command;
}