// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Graphics.Entities;
using Nalix.Graphics.UI.DataGrid;
using SFML.Graphics;
using SFML.System;
using System.Collections.Generic;

namespace Nalix.Graphics.Sandbox.Scenes.View;

/// <summary>
/// Small test view that hosts a DataGrid&lt;Person&gt; for manual testing.
/// Attach this view to your MainScene to see virtualization, selection and scrolling.
/// </summary>
public sealed class DataGridTestView : RenderObject
{
    private readonly DataGrid<Person> _grid;
    private readonly List<Person> _items;

    public DataGridTestView()
    {
        // Create grid with embedded font (falls back to EmbeddedAssets in DataGrid constructor)
        _grid = new DataGrid<Person>
        {
            // Position and size for testing; adjust as needed
            Position = new Vector2f(20f, 40f),
            Size = new Vector2f(420, 520f),
            RowHeight = 28f
        };

        // Create sample columns
        _grid.AddColumn(new DataGridColumn<Person>("ID", 60f, p => p.Id.ToString()));
        _grid.AddColumn(new DataGridColumn<Person>("Name", 220f, p => p.Name));
        _grid.AddColumn(new DataGridColumn<Person>("Age", 80f, p => p.Age.ToString()));
        _grid.AddColumn(new DataGridColumn<Person>("Email", 360f, p => p.Email));

        // Prepare sample data (enough rows to test virtualization and scrolling)
        _items = [];
        for (System.Int32 i = 1; i <= 250; i++)
        {
            _items.Add(new Person(i, $"Person {i}", 18 + (i % 50), $"person{i}@example.com"));
        }

        _grid.SetItems(_items);

        // Optional: handle selection changed to log selected items
        _grid.SelectionChanged += OnSelectionChanged;

        // Optional: initial sort by Name ascending
        _grid.SortBy(1, true);
    }

    private void OnSelectionChanged(Person[] selected)
    {
        // For testing: write a simple log to console
        System.Console.WriteLine($"Selected {selected.Length} item(s).");
        if (selected.Length > 0)
        {
            System.Console.WriteLine($"First selected: {selected[0].Name} (Id={selected[0].Id})");
        }
    }

    public override void Update(System.Single dt) =>
        // Delegate interaction/selection updates to the inner grid
        _grid.Update(dt);

    public override void Draw(IRenderTarget target) =>
        // Draw the grid
        _grid.Draw(target);

    [return: System.Diagnostics.CodeAnalysis.NotNull]
    protected override IDrawable GetDrawable() =>
        // Not used; DataGrid draws directly to IRenderTarget
        throw new System.NotSupportedException("Use Draw(IRenderTarget) directly for DataGridTestView.");
}

/// <summary>
/// Simple data model used for DataGrid testing.
/// </summary>
public sealed class Person(System.Int32 id, System.String name, System.Int32 age, System.String email)
{
    public System.Int32 Id { get; set; } = id;

    public System.String Name { get; set; } = name;

    public System.Int32 Age { get; set; } = age;

    public System.String Email { get; set; } = email;
}