// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Launcher.Scenes.CommonView;
using AutoX.Gara.Launcher.Scenes.RegisterView;
using Nalix.Framework.Configuration;
using Nalix.Graphics.Attributes;
using Nalix.Graphics.Engine;
using Nalix.Graphics.Scenes;

namespace AutoX.Gara.Launcher.Scenes;

[DynamicLoad]
public sealed class RegisterScene : BaseScene
{
    public RegisterScene() : base(SceneNameConstants.RegisterScene)
    {
    }

    protected override void LoadObjects()
    {
        BackButtonView backButton = new();

        backButton.BackRequested += () => SceneManager.Instance.ScheduleSceneChange(ConfigurationManager.Instance.Get<GraphicsConfig>().MainScene);

        AddObject(backButton);
        AddObject(new ButtonView());
    }
}
