// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Graphics.Assets;
using Nalix.Graphics.Entities;
using Nalix.Graphics.Extensions;
using Nalix.Graphics.UI.DataGrid;
using Nalix.Graphics.UI.Tab;
using SFML.Graphics;
using SFML.System;
using System.Collections.Generic;

namespace Nalix.Graphics.Sandbox.Scenes.View;

/// <summary>
/// Test view that composes NavigationPane (left), TabControl (top) and content area.
/// Selecting "Customers" shows a DataGrid of Person instances.
/// </summary>
public sealed class TabTestView : RenderObject
{
    private readonly NavigationPane _nav;
    private readonly TabControl _tabs;

    // content instances
    private readonly DataGrid<Person> _customersGrid;
    private readonly SimplePlaceholder _jobsPlaceholder;
    private readonly SimplePlaceholder _inventoryPlaceholder;
    private readonly SimplePlaceholder _reportsPlaceholder;
    private readonly SimplePlaceholder _settingsPlaceholder;

    // currently selected nav index (0..4)
    private System.Int32 _currentNavIndex = 0;

    // layout constants
    private const System.Single LeftPaneWidth = 220f;
    private readonly Vector2f _size;

    /// <summary>
    /// Initialize test UI composition.
    /// </summary>
    public TabTestView()
    {
        // overall size (adjust for your window)
        _size = new Vector2f(1024f, 720f);

        // create navigation pane (left)
        _nav = new NavigationPane
        {
            Position = new Vector2f(0f, 0f),
            Size = new Vector2f(LeftPaneWidth, _size.Y)
        };
        _nav.SelectionChanged += OnNavSelectionChanged;

        // Add navigation items (Jobs, Customers, Inventory, Reports, Settings)
        _nav.AddItem("Jobs", null, "jobs");
        _nav.AddItem("Customers", null, "customers");
        _nav.AddItem("Inventory", null, "inventory");
        _nav.AddItem("Reports", null, "reports");
        _nav.AddItem("Settings", null, "settings");

        // create top tab control (for demo, tabs are static)
        _tabs = new TabControl
        {
            Position = new Vector2f(LeftPaneWidth + 8f, 8f)
        };
        _tabs.AddTab(new UI.Models.TabItem("Overview"));
        _tabs.AddTab(new UI.Models.TabItem("Details"));
        _tabs.AddTab(new UI.Models.TabItem("Activity"));

        // Prepare DataGrid for Customers
        var font = EmbeddedAssets.JetBrainsMono.ToFont();
        _customersGrid = new DataGrid<Person>(font)
        {
            Position = new Vector2f(LeftPaneWidth + 20f, 64f),
            Size = new Vector2f(_size.X - LeftPaneWidth - 40f, _size.Y - 80f),
            RowHeight = 28f
        };
        _customersGrid.AddColumn(new DataGridColumn<Person>("ID", 60f, p => p.Id.ToString()));
        _customersGrid.AddColumn(new DataGridColumn<Person>("Name", 200f, p => p.Name));
        _customersGrid.AddColumn(new DataGridColumn<Person>("Age", 80f, p => p.Age.ToString()));
        _customersGrid.AddColumn(new DataGridColumn<Person>("Email", 400f, p => p.Email));

        // populate sample data
        var customers = new List<Person>();
        for (System.Int32 i = 1; i <= 100; i++)
        {
            customers.Add(new Person(i, $"Customer {i}", 18 + (i % 50), $"customer{i}@example.com"));
        }
        _customersGrid.SetItems(customers);

        // placeholders for other sections
        _jobsPlaceholder = new SimplePlaceholder("Jobs", new Vector2f(LeftPaneWidth + 20f, 64f), new Vector2f(_size.X - LeftPaneWidth - 40f, _size.Y - 80f), font);
        _inventoryPlaceholder = new SimplePlaceholder("Inventory", new Vector2f(LeftPaneWidth + 20f, 64f), new Vector2f(_size.X - LeftPaneWidth - 40f, _size.Y - 80f), font);
        _reportsPlaceholder = new SimplePlaceholder("Reports", new Vector2f(LeftPaneWidth + 20f, 64f), new Vector2f(_size.X - LeftPaneWidth - 40f, _size.Y - 80f), font);
        _settingsPlaceholder = new SimplePlaceholder("Settings", new Vector2f(LeftPaneWidth + 20f, 64f), new Vector2f(_size.X - LeftPaneWidth - 40f, _size.Y - 80f), font);

        // initial selection already set by NavigationPane.AddItem selecting the first item.
        // ensure we know current selected index by listening to SelectionChanged above.
    }

    private void OnNavSelectionChanged(System.Int32 idx, System.String title, System.Object tag) => _currentNavIndex = idx;

    /// <summary>
    /// Update forwards input updates to interactive widgets.
    /// </summary>
    public override void Update(System.Single dt)
    {
        // Update navigation and tab controls so they respond to input.
        _nav.Update(dt);
        _tabs.Update(dt);

        // Forward update to active content only (DataGrid has input handling for row selection)
        switch (_currentNavIndex)
        {
            case 0:
                _jobsPlaceholder.Update(dt);
                break;
            case 1:
                _customersGrid.Update(dt);
                break;
            case 2:
                _inventoryPlaceholder.Update(dt);
                break;
            case 3:
                _reportsPlaceholder.Update(dt);
                break;
            case 4:
                _settingsPlaceholder.Update(dt);
                break;
        }
    }

    /// <summary>
    /// Draw the composed UI: left nav, top tabs and active content.
    /// </summary>
    public override void Draw(IRenderTarget target)
    {
        // draw navigation pane
        _nav.Draw(target);

        // draw tab control
        _tabs.Draw(target);

        // draw active content area
        switch (_currentNavIndex)
        {
            case 0:
                _jobsPlaceholder.Draw(target);
                break;
            case 1:
                _customersGrid.Draw(target);
                break;
            case 2:
                _inventoryPlaceholder.Draw(target);
                break;
            case 3:
                _reportsPlaceholder.Draw(target);
                break;
            case 4:
                _settingsPlaceholder.Draw(target);
                break;
        }
    }

    [return: System.Diagnostics.CodeAnalysis.NotNull]
    protected override IDrawable GetDrawable() => throw new System.NotSupportedException("TabTestView uses Draw(IRenderTarget) directly.");

    /// <summary>
    /// Very small placeholder view for non-Customers sections in this test.
    /// </summary>
    private sealed class SimplePlaceholder
    {
        private readonly System.String _label;
        private readonly Vector2f _position;
        private readonly Vector2f _size;
        private readonly Text _text;
        private readonly RectangleShape _bg;

        public SimplePlaceholder(System.String label, Vector2f position, Vector2f size, Font font)
        {
            _label = label;
            _position = position;
            _size = size;
            _bg = new RectangleShape(size) { Position = position, FillColor = new Color(24, 26, 28) };
            _text = new Text(font, label, 28u) { FillColor = new Color(200, 200, 200) };

            var lb = _text.GetLocalBounds();
            _text.Origin = new Vector2f(lb.Left + (lb.Width / 2f), lb.Top + (lb.Height / 2f));
            _text.Position = new Vector2f(position.X + (size.X / 2f), position.Y + (size.Y / 2f));
        }

        public void Update(System.Single dt) { /* no-op */ }

        public void Draw(IRenderTarget target)
        {
            target.Draw(_bg);
            target.Draw(_text);
        }
    }
}