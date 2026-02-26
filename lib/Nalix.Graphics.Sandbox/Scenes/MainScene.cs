// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Framework.Configuration;
using Nalix.Graphics.Attributes;
using Nalix.Graphics.Engine;
using Nalix.Graphics.Sandbox.Scenes.View;
using Nalix.Graphics.Scenes;

namespace Nalix.Graphics.Sandbox.Scenes;

[DynamicLoad]
public sealed class MainScene : BaseScene
{
    public MainScene() : base(ConfigurationManager.Instance.Get<GraphicsConfig>().MainScene)
    {
    }

    protected override void LoadObjects() =>
        //ButtonView buttonView = new();
        //VersionView versionView = new();
        //ScrollingBanner scrollingBannerView = new("⚠ Playing games for more than 180 minutes a day can negatively impact your health ⚠", null, 200f);

        //base.AddObject(buttonView);
        //base.AddObject(versionView);
        //base.AddObject(scrollingBannerView);

        //base.AddObject(new DataGridTestView());


        // Add the tab/navigation test view
        base.AddObject(new TabTestView());
}