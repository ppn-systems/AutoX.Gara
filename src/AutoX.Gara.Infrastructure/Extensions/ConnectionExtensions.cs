// Copyright (c) 2026 Ascendance Team. All rights reserved.

using Nalix.Common.Connection;

namespace AutoX.Gara.Infrastructure.Extensions;

/// <summary>
/// Manages authentication state for connections without modifying the IConnection interface.
/// Uses ConditionalWeakTable for automatic memory management.
/// </summary>
public static class ConnectionExtensions
{
    /// <summary>
    /// Weak reference table that automatically removes entries when connections are GC'd.
    /// Thread-safe for concurrent access.
    /// </summary>
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<IConnection, StateHolder> _states = [];

    /// <summary>
    /// Helper class to hold mutable state.
    /// </summary>
    private sealed class StateHolder
    {
        public AuthState State { get; set; } = AuthState.None;
    }

    /// <summary>
    /// Authentication states for connection lifecycle.
    /// </summary>
    public enum AuthState : System.Byte
    {
        /// <summary>
        /// Initial state, no handshake completed.
        /// </summary>
        None = 0,

        /// <summary>
        /// Handshake completed, awaiting login.
        /// </summary>
        HandshakeComplete = 1,

        /// <summary>
        /// Login completed, ready to disconnect.
        /// </summary>
        LoginComplete = 2
    }

    /// <summary>
    /// Gets the current authentication state of the connection.
    /// </summary>
    /// <param name="connection">The connection to check.</param>
    /// <returns>Current authentication state.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static AuthState GetAuthState(this IConnection connection) => _states.TryGetValue(connection, out StateHolder holder) ? holder.State : AuthState.None;

    /// <summary>
    /// Sets the authentication state of the connection.
    /// </summary>
    /// <param name="connection">The connection to update.</param>
    /// <param name="state">New authentication state.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static void SetAuthState(this IConnection connection, AuthState state)
    {
        StateHolder holder = _states.GetOrCreateValue(connection);
        holder.State = state;
    }

    /// <summary>
    /// Checks if connection should be kept alive after processing.
    /// </summary>
    /// <param name="connection">The connection to check.</param>
    /// <returns>True if connection should remain open, false otherwise.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static System.Boolean ShouldKeepAlive(this IConnection connection)
    {
        AuthState state = connection.GetAuthState();

        // Keep alive if handshake is done but login is not
        return state == AuthState.HandshakeComplete;
    }

    /// <summary>
    /// Resets the authentication state for a connection.
    /// </summary>
    /// <param name="connection">The connection to reset.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static void ResetAuthState(this IConnection connection)
    {
        if (_states.TryGetValue(connection, out StateHolder holder))
        {
            holder.State = AuthState.None;
        }
    }

    /// <summary>
    /// Removes the state entry for a connection (optional cleanup).
    /// Note: This is optional as ConditionalWeakTable auto-removes entries when connections are GC'd.
    /// </summary>
    /// <param name="connection">The connection to remove.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static void RemoveAuthState(this IConnection connection) => _ = _states.Remove(connection);
}