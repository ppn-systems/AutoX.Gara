// Copyright (c) 2026 PPN Corporation. All rights reserved.

namespace Nalix.Graphics.Attributes;

/// <summary>
/// Indicates that a class should be automatically discovered and loaded by the engine.
/// </summary>
/// <remarks>
/// This attribute is used as an opt-in marker.
/// Only classes decorated with <see cref="DynamicLoadAttribute"/> will be loaded.
/// </remarks>
/// <param name="reason">
/// Optional description explaining why the class is auto-loaded
/// (for debugging, logging, or documentation purposes).
/// </param>
[System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class DynamicLoadAttribute(System.String reason) : System.Attribute
{
    /// <summary>
    /// Gets the reason or description for auto-loading this class.
    /// </summary>
    public System.String Reason { get; } = reason;

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicLoadAttribute"/> class without a reason.
    /// </summary>
    public DynamicLoadAttribute() : this(System.String.Empty) { }
}
