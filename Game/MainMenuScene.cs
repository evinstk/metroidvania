using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez;

namespace Game
{
    class MainMenuScene : Scene
    {
        public override void Initialize()
        {
            SetDesignResolution(MainScene.ResWidth, MainScene.ResHeight, SceneResolutionPolicy.ShowAllPixelPerfect);
            ClearColor = new Color(0xff000000);

            AddRenderer(new ScreenSpaceRenderer(0));

            CreateEntity("main-menu").AddComponent<MainMenu>();
        }
    }

    class MainMenu : RenderableComponent, IUpdatable
    {
        public override float Width => MainScene.ResWidth;
        public override float Height => MainScene.ResHeight;

        static FontCharacterSource _title = new FontCharacterSource("Metroidvania");
        static FontCharacterSource[] _options = new FontCharacterSource[]
        {
            new FontCharacterSource("Training"),
            new FontCharacterSource("Exit"),
        };

        NezSpriteFont _font;
        VirtualIntegerAxis _selectionInput;
        VirtualButton _confirmInput;

        int _selection;
        int _lastInput;

        public override void OnAddedToEntity()
        {
            var font = Core.Content.Load<SpriteFont>("Fonts/DefaultFont");
            _font = new NezSpriteFont(font);

            _selectionInput = new VirtualIntegerAxis();
            _selectionInput.Nodes.Add(new VirtualAxis.GamePadLeftStickY());
            _selectionInput.Nodes.Add(new VirtualAxis.GamePadDpadUpDown());
            _selectionInput.Nodes.Add(new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.CancelOut, Keys.Up, Keys.Down));
            _selectionInput.Nodes.Add(new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.CancelOut, Keys.W, Keys.S));

            _confirmInput = new VirtualButton();
            _confirmInput.Nodes.Add(new VirtualButton.KeyboardKey(Keys.Enter));
            _confirmInput.Nodes.Add(new VirtualButton.KeyboardKey(Keys.E));
            _confirmInput.Nodes.Add(new VirtualButton.GamePadButton(0, Buttons.A));
            _confirmInput.Nodes.Add(new VirtualButton.GamePadButton(0, Buttons.Start));
        }

        public void Update()
        {
            var input = _selectionInput.Value;
            if (_lastInput == 0)
            {
                var optionsLength = _options.Length;
                _selection = (_selection + input) % optionsLength;
                _selection = _selection == -1 ? optionsLength - 1 : _selection;
            }
            _lastInput = input;

            if (_confirmInput.IsPressed)
            {
                switch (_selection)
                {
                    case 0:
                        Core.StartSceneTransition(new FadeTransition(() => new RoomScene("VFVTMVSAZGHVCLIFQHNSSYNCTKPZPVFIGXIXJV")));
                        break;
                    case 1:
                        Core.Exit();
                        break;
                }
            }
        }

        public override void Render(Batcher batcher, Camera camera)
        {
            _font.DrawInto(
                batcher,
                ref _title,
                new Vector2(20, 20),
                Color.White,
                0,
                Vector2.Zero,
                Vector2.One,
                SpriteEffects.None,
                0);

            for (var i = 0; i < _options.Length; ++i)
            {
                _font.DrawInto(
                    batcher,
                    ref _options[i],
                    new Vector2(20, 60 + i * 20),
                    i == _selection ? Color.Yellow : Color.White,
                    0,
                    Vector2.Zero,
                    Vector2.One,
                    SpriteEffects.None,
                    0);
            }
        }
    }
}
