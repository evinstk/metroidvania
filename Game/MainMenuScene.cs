using Microsoft.Xna.Framework;
using Nez;
using Nez.UI;

namespace Game
{
    class MainMenuScene : Scene
    {
        public override void Initialize()
        {
            SetDesignResolution(Constants.ResWidth, Constants.ResHeight, SceneResolutionPolicy.ShowAllPixelPerfect);
            ClearColor = Constants.ClearColor;

            AddRenderer(new ScreenSpaceRenderer(0, RenderLayers.PauseMenu));
        }

        public override void OnStart()
        {
            var bg = CreateEntity("background");
            bg.Parent = Camera.Transform;
            var bgRenderer = bg.AddComponent(new TiledSpriteRenderer(Content.LoadTexture($"{ContentPath.Backgrounds}/stars.png")));
            bgRenderer.RenderLayer = RenderLayers.PauseMenu;

            CreateEntity("main_menu").AddComponent<MainMenu>();
        }
    }

    class MainMenu : Component
    {
        FMOD.Studio.EventInstance _music;
        FMOD.Studio.EventInstance _select;
        FMOD.Studio.EventInstance _confirm;

        public override void OnAddedToEntity()
        {
            _music = GameContent.LoadSound("Music", "main_menu");
            _music.start();
            _select = GameContent.LoadSound("Common", "select");
            _confirm = GameContent.LoadSound("Common", "confirm");

            var canvas = Entity.AddComponent<UICanvas>();
            canvas.RenderLayer = RenderLayers.PauseMenu;

            var table = canvas.Stage.AddElement(new Table());
            table.Top().Left();
            table.FillParent = true;

            var logoSprite = GameContent.LoadSprite("logo", "logo", Core.Scene.Content);
            table.Add(new Image(new SpriteDrawable(logoSprite))).SetExpandX().Top().SetPadTop(32);

            table.Row();

            var options = new Table();
            options.Defaults().Left().Height(16);
            table.Add(options).Left().SetPadLeft(80).SetPadTop(32);

            var textButtonStyle = new TextButtonStyle();
            textButtonStyle.OverFontColor = Color.Yellow;
            var startButton = new ConfigurableTextButton("Start", textButtonStyle);
            startButton.OnClicked += LaunchGame;
            options.Add(startButton);
            options.Row();
            var exitButton = new ConfigurableTextButton("Exit", textButtonStyle);
            exitButton.OnClicked += button => Core.Exit();
            options.Add(exitButton);

            canvas.Stage.SetGamepadFocusElement(startButton);

            // assign after setting focus element
            startButton.OnFocusedEvent += PlaySelect;
            exitButton.OnFocusedEvent += PlaySelect;
        }

        public override void OnRemovedFromEntity()
        {
            _music.release();
            _select.release();
        }

        void LaunchGame(Button button)
        {
            _confirm.start();
            var saveSlot = 0;
            var save = Core.GetGlobalManager<SaveSystem>().Load(saveSlot);
            var transition = Core.StartSceneTransition(new FadeTransition(() => new MainScene(saveSlot, save)));
            transition.FadeOutDuration = 2;
            _music.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

        void PlaySelect(ConfigurableTextButton btn) => _select.start();
    }
}
