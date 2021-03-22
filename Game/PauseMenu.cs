using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez;

namespace Game
{
    class PauseMenu : RenderableComponent, IUpdatable
    {
        public override float Width => Constants.ResWidth;
        public override float Height => Constants.ResHeight;

        static Color _clearColor = new Color(0x88000000);
        static FontCharacterSource[] _options = new FontCharacterSource[]
        {
            new FontCharacterSource("Resume"),
            new FontCharacterSource("Restart at Checkpoint"),
            new FontCharacterSource("Quit to Main Menu"),
            new FontCharacterSource("Quit to Desktop"),
        };

        bool _showing;
        int _selection;
        int _lastInput;

        NezSpriteFont _font;
        VirtualButton _pauseInput;
        VirtualIntegerAxis _selectionInput;
        VirtualButton _confirmInput;

        public override void OnAddedToEntity()
        {
            var font = Core.Content.Load<SpriteFont>("Fonts/DefaultFont");
            _font = new NezSpriteFont(font);

            _pauseInput = new VirtualButton();
            _pauseInput.Nodes.Add(new VirtualButton.KeyboardKey(Keys.Escape));
            _pauseInput.Nodes.Add(new VirtualButton.GamePadButton(0, Buttons.Back));

            _selectionInput = new VirtualIntegerAxis();
            _selectionInput.Nodes.Add(new VirtualAxis.GamePadLeftStickY());
            _selectionInput.Nodes.Add(new VirtualAxis.GamePadDpadUpDown());
            _selectionInput.Nodes.Add(new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.CancelOut, Keys.Up, Keys.Down));
            _selectionInput.Nodes.Add(new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.CancelOut, Keys.W, Keys.S));

            _confirmInput = new VirtualButton();
            _confirmInput.Nodes.Add(new VirtualButton.KeyboardKey(Keys.Enter));
            _confirmInput.Nodes.Add(new VirtualButton.KeyboardKey(Keys.E));
            _confirmInput.Nodes.Add(new VirtualButton.GamePadButton(0, Buttons.A));
        }

        public void Update()
        {
            if (_showing)
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
                            _showing = false;
                            Time.TimeScale = 1;
                            break;
                        case 1:
                            Time.TimeScale = 1;
                            var saveSlot = Entity.GetMainScene().SaveSlot;
                            var save = Core.GetGlobalManager<SaveSystem>().Load(saveSlot);
                            Core.StartSceneTransition(new FadeTransition(() => new MainScene(saveSlot, save)));
                            break;
                        case 2:
                            Time.TimeScale = 1;
                            var transition = Core.StartSceneTransition(new FadeTransition(() => new MainMenuScene()));
                            transition.FadeOutDuration = 3f;
                            break;
                        case 3:
                            Core.Exit();
                            break;
                    }
                }
            }

            // TODO: if pause_timer is in effect, still need to process this somehow
            if (_pauseInput.IsPressed)
            {
                _showing = !_showing;
                if (_showing)
                {
                    Time.TimeScale = 0;
                    _selection = 0;
                }
                else
                {
                    Time.TimeScale = 1;
                }
            }
        }

        public override void Render(Batcher batcher, Camera camera)
        {
            if (!_showing) return;

            batcher.DrawRect(
                new Rectangle(0, 0, Constants.ResWidth, Constants.ResHeight),
                _clearColor);

            for (var i = 0; i < _options.Length; ++i)
            {
                _font.DrawInto(
                    batcher,
                    ref _options[i],
                    new Vector2(100, 60 + i * 20),
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
