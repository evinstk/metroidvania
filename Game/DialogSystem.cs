using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    class DialogSystem : RenderableComponent, IUpdatable
    {
        public Vector2 BoxMargin = new Vector2(10, 10);
        public int BoxHeight = 70;
        public Vector2 Margin = new Vector2(20, 20);
        public Vector2 OptionsOffset = new Vector2(270, 100);
        public float OptionsBoxWidth = 128;
        public Vector2 OptionsMargin = new Vector2(10, 10);
        public float OptionsSpacing = 20f;
        public int MsPerChar = 10;

        public override float Width => 1;
        public override float Height => 1;

        NezSpriteFont _font;
        string _line;
        int _readerIndex;
        float _charElapsed;
        StringBuilder _sb = new StringBuilder(1000);
        FontCharacterSource _text;

        List<string> _options;
        public int OptionIndex => _optionIndex;
        int _optionIndex = 0;
        int _lastInput = 0;

        VirtualIntegerAxis _inputSelect;

        public override void OnAddedToEntity()
        {
            var font = Core.Content.Load<SpriteFont>("Fonts/DefaultFont");
            _font = new NezSpriteFont(font);

            _inputSelect = new VirtualIntegerAxis();
            _inputSelect.AddGamePadDPadUpDown();
            _inputSelect.AddKeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.Up, Keys.Down);
        }

        public void FeedLine(string line)
        {
            _line = line;
            _sb.Clear();
            _text = new FontCharacterSource(_sb);
            _charElapsed = 0;
            _readerIndex = 0;

            _options = null;
        }

        public void FeedOptions(List<string> options, bool cancelLast)
        {
            Insist.IsTrue(options != null && options.Count > 0);
            _options = options;
            _optionIndex = 0;
        }

        public void Update()
        {
            if (_line == null) return;

            var lineWidth = Constants.ResWidth - Margin.X * 2;
            var charIndex = (int)(_charElapsed * 1000 / MsPerChar);
            while (_readerIndex <= charIndex && _readerIndex < _line.Length)
            {
                var nextChar = _line[_readerIndex++];
                _sb.Append(nextChar);
                // check if we need a line break
                if (nextChar == ' ')
                {
                    var tempReaderIndex = _readerIndex;
                    var tempChar = _line[tempReaderIndex++];
                    var length = 0;
                    while (tempChar != ' ' && tempReaderIndex < _line.Length)
                    {
                        _sb.Append(tempChar);
                        ++length;
                        tempChar = _line[tempReaderIndex++];
                    }
                    var size = _font.MeasureString(_sb);
                    if (size.X > lineWidth)
                    {
                        _sb.Remove(_readerIndex - 1, length + 1);
                        _sb.Append("\n");
                    }
                    else
                    {
                        _sb.Remove(_readerIndex, length);
                    }
                }
            }
            _text = new FontCharacterSource(_sb);
            _charElapsed += Time.DeltaTime;

            UpdateOptions();

            var selectInput = _inputSelect.Value;
            if (_options != null && selectInput != 0 && selectInput != _lastInput)
            {
                _optionIndex = Mathf.Clamp(_optionIndex + selectInput, 0, _options.Count - 1);
            }
            _lastInput = selectInput;
        }

        void UpdateOptions()
        {
            if (_sb.Length == _line.Length)
            {
            }
        }

        public override void Render(Batcher batcher, Camera camera)
        {
            if (_line != null)
            {
                var boxBounds = new RectangleF(
                    BoxMargin,
                    new Vector2(Constants.ResWidth - BoxMargin.X * 2, BoxHeight));
                batcher.DrawRect(boxBounds, Color.Black);
                batcher.DrawHollowRect(boxBounds, Color.White, 2);
                _font.DrawInto(
                    batcher,
                    ref _text,
                    Margin,
                    Color.White,
                    0,
                    Vector2.Zero,
                    Vector2.One,
                    SpriteEffects.None,
                    0);
            }

            if (_options != null)
            {
                var boxBounds = new RectangleF(
                    OptionsOffset, new Vector2(OptionsBoxWidth, (_options.Count * OptionsSpacing) + (OptionsMargin.Y * 2)));
                batcher.DrawRect(boxBounds, Color.Black);
                batcher.DrawHollowRect(boxBounds, Color.White, 2);

                for (var i = 0; i < _options.Count; ++i)
                {
                    var option = _options[i];
                    var optionText = new FontCharacterSource(option);
                    _font.DrawInto(
                        batcher,
                        ref optionText,
                        OptionsOffset + OptionsMargin + new Vector2(0, i * OptionsSpacing),
                        i == _optionIndex ? Color.Yellow : Color.White,
                        0,
                        Vector2.Zero,
                        Vector2.One,
                        SpriteEffects.None,
                        0);
                }
            }
        }
    }
}
