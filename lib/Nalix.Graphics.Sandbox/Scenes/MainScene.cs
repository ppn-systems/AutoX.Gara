// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Framework.Configuration;
using Nalix.Graphics.Attributes;
using Nalix.Graphics.Engine;
using Nalix.Graphics.Scenes;
using Nalix.Graphics.UI.Controls;
using SFML.System;

namespace Nalix.Graphics.Sandbox.Scenes;

[DynamicLoad]
public sealed class MainScene : BaseScene
{
    public MainScene() : base(ConfigurationManager.Instance.Get<GraphicsConfig>().MainScene)
    {
    }

    protected override void LoadObjects()
    {
        ComboBox<System.String> a = new(["Option 1", "Option 2", "Option 3", "Option 2", "Option 3", "Option 2", "Option 3", "Option 2", "Option 3", "Option 2", "Option 3", "Option 2", "Option 3", "Option 2", "Option 3"])
        {
            Position = new Vector2f(150f, 100f)
        };
        base.AddObject(a);
    }
    //ButtonView buttonView = new();
    //VersionView versionView = new();
    //ScrollingBanner scrollingBannerView = new("⚠ Playing games for more than 180 minutes a day can negatively impact your health ⚠", null, 200f);

    //base.AddObject(buttonView);
    //base.AddObject(versionView);
    //base.AddObject(scrollingBannerView);

    //base.AddObject(new DataGridTestView());



    //// Add the tab/navigation test view
    //base.AddObject(new TabTestView());
}