using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez;
using System.IO;

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
        enum States
        {
            Top,
            Save,
            Delete,
        }

        public override float Width => MainScene.ResWidth;
        public override float Height => MainScene.ResHeight;

        static FontCharacterSource _title = new FontCharacterSource("Metroidvania");
        static string[] _options = new string[]
        {
            "Start",
            "Exit",
        };
        static string[] _noYes = new string[]
        {
            "No",
            "Yes",
        };
        const int _numSaveSlots = 4;
        bool[] _saveExists = new bool[4];

        NezSpriteFont _font;
        VirtualIntegerAxis _selectionInput;
        VirtualButton _confirmInput;
        VirtualButton _deleteInput;

        States _state = States.Top;
        int _selection;
        int _saveSelection; // for delete
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

            _deleteInput = new VirtualButton();
            _deleteInput.Nodes.Add(new VirtualButton.KeyboardKey(Keys.Delete));
            _deleteInput.Nodes.Add(new VirtualButton.KeyboardKey(Keys.X));
            _deleteInput.Nodes.Add(new VirtualButton.GamePadButton(0, Buttons.Back));

            SyncSaveExists();
        }

        void SyncSaveExists()
        {
            for (var i = 0; i < _numSaveSlots; ++i)
            {
                var path = GameFile.GetSavePath(i);
                _saveExists[i] = File.Exists(path);
            }
        }

        public void Update()
        {
            if (_state == States.Top) UpdateTop();
            else if (_state == States.Save) UpdateSave();
            else if (_state == States.Delete) UpdateDelete();
        }

        void UpdateTop()
        {
            HandleSelect(_options.Length);

            if (_confirmInput.IsPressed)
            {
                var selection = _options[_selection];
                if (selection == "Start")
                {
                    _selection = 0;
                    _state = States.Save;
                }
                else if (selection == "Exit")
                {
                    Core.Exit();
                }
            }
        }

        void UpdateSave()
        {
            HandleSelect(_numSaveSlots + 1);

            if (_confirmInput.IsPressed)
            {
                if (_selection < _numSaveSlots)
                {
                    var saveExists = _saveExists[_selection];
                    if (saveExists)
                    {
                        Core.StartSceneTransition(new FadeTransition(() => new RoomScene(_selection)));
                    }
                    else
                    {
                        SaveSystem2.Save(_selection);
                        Core.StartSceneTransition(new FadeTransition(() => new RoomScene(_selection)));
                    }
                }
                else if (_selection == _numSaveSlots) // back
                {
                    _selection = 0;
                    _state = States.Top;
                }
            }
            if (_deleteInput.IsPressed && _selection < _numSaveSlots && _saveExists[_selection])
            {
                _saveSelection = _selection;
                _selection = 0;
                _state = States.Delete;
            }
        }

        void UpdateDelete()
        {
            HandleSelect(_noYes.Length);
            if (_confirmInput.IsPressed)
            {
                var selection = _noYes[_selection];
                if (selection == "No")
                {
                    _selection = _saveSelection;
                    _state = States.Save;
                }
                else if (selection == "Yes")
                {
                    SaveSystem2.Delete(_saveSelection);
                    SyncSaveExists();
                    _selection = _saveSelection;
                    _state = States.Save;
                }
            }
        }

        void HandleSelect(int numSelections)
        {
            var input = _selectionInput.Value;
            if (_lastInput == 0)
            {
                _selection = (_selection + input) % numSelections;
                _selection = _selection == -1 ? numSelections - 1 : _selection;
            }
            _lastInput = input;
        }

        public override void Render(Batcher batcher, Camera camera)
        {
            if (_state == States.Top) RenderTop(batcher);
            else if (_state == States.Save) RenderSave(batcher);
            else if (_state == States.Delete) RenderDelete(batcher);
        }

        void RenderTop(Batcher batcher)
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
                RenderOption(batcher, _options[i], new Vector2(20, 60 + i * 20), i == _selection);
            }
        }

        void RenderSave(Batcher batcher)
        {
            for (var i = 0; i < _numSaveSlots; ++i)
            {
                var text = _saveExists[i] ? "Continue" : "New Game";
                RenderOption(batcher, $"{i + 1}. {text}", new Vector2(20, 60 + i * 20), i == _selection);
            }

            RenderOption(batcher, "Back", new Vector2(20, MainScene.ResHeight - 50), _selection == _numSaveSlots);
        }

        void RenderDelete(Batcher batcher)
        {
            var prompt = new FontCharacterSource($"Really delete save #{_saveSelection + 1}?");
            _font.DrawInto(
                batcher,
                ref prompt,
                new Vector2(20, 20),
                Color.White,
                0,
                Vector2.Zero,
                Vector2.One,
                SpriteEffects.None,
                0);

            for (var i = 0; i < _noYes.Length; ++i)
            {
                RenderOption(batcher, _noYes[i], new Vector2(20, 60 + i * 20), i == _selection);
            }
        }

        void RenderOption(Batcher batcher, string option, Vector2 position, bool selected)
        {
            var source = new FontCharacterSource(option);
            _font.DrawInto(
                batcher,
                ref source,
                position,
                selected ? Color.Yellow : Color.White,
                0,
                Vector2.Zero,
                Vector2.One,
                SpriteEffects.None,
                0);
        }
    }
}
