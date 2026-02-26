using AutoX.Gara.Launcher.Scenes.MainMenuView;
using Nalix.Framework.Configuration;
using Nalix.Graphics.Attributes;
using Nalix.Graphics.Engine;
using Nalix.Graphics.Scenes;

namespace AutoX.Gara.Launcher.Scenes;

[DynamicLoad]
public sealed class MainMenuScene : BaseScene
{
    public MainMenuScene() : base(ConfigurationManager.Instance.Get<GraphicsConfig>().MainScene)
    {
    }

    protected override void LoadObjects()
    {
        ButtonView buttonView = new();
        VersionView versionView = new();

        buttonView.ExitRequested += () => System.Environment.Exit(0);
        buttonView.LoginRequested += () => SceneManager.Instance.ScheduleSceneChange(SceneNameConstants.LoginScene);
        buttonView.RegisterRequested += () => SceneManager.Instance.ScheduleSceneChange(SceneNameConstants.RegisterScene);

        base.AddObject(buttonView);
        base.AddObject(versionView);
    }
}
