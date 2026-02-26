// Copyright (c) 2026 PPN Corporation. All rights reserved.

namespace Nalix.Graphics.Abstractions;

public interface IUpdatable
{
    /// <summary>
    /// Updates the object with the given delta time.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since the last update in seconds.</param>
    void Update(System.Single deltaTime);
}
