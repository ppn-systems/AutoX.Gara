// Copyright (c) 2026 PPN Corporation. All rights reserved.
using System;
namespace AutoX.Gara.Domain.Abstractions;
/// <summary>
/// Root interface for all domain entities.
/// </summary>
public interface IEntity<out TId>
{
    TId Id { get; }
}
/// <summary>
/// Interface for entities that need audit tracking.
/// </summary>
public interface IAuditEntity
{
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
}
/// <summary>
/// Interface for entities that support soft deletion.
/// </summary>
public interface ISoftDelete
{
    DateTime? DeletedAt { get; set; }
    bool IsDeleted => DeletedAt.HasValue;
}
/// <summary>
/// Base class for all domain entities.
/// </summary>
public abstract class Entity<TId> : IEntity<TId>
{
    public virtual TId Id { get; set; }
}
/// <summary>
/// Base class for entities with audit and soft delete support.
/// </summary>
public abstract class AuditEntity<TId> : Entity<TId>, IAuditEntity, ISoftDelete
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
}
