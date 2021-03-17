using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Sprites;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    class DialogSystem : RenderableComponent, IUpdatable
    {
        public Vector2 PortraitSize = new Vector2(64);

        public Vector2 InnerMargin = new Vector2(10, 10);
        public Vector2 BoxMarginDefault = new Vector2(10, 10);

        public Vector2 OptionsOffset = new Vector2(322, 100);
        public Vector2 OptionsMargin = new Vector2(10, 10);
        public float OptionsSpacing = 8f;
        public float OptionsWidth = 128f;

        public int MsPerChar = 10;

        public override float Width => 1;
        public override float Height => 1;

        NezSpriteFont _font;
        string _line;
        string _speaker;
        int _readerIndex;
        float _charElapsed;
        StringBuilder _sb = new StringBuilder(1000);
        FontCharacterSource _text;
        bool _showBorder = true;
        Vector2 _boxMargin = new Vector2(10, 10);
        bool _playSound = true;

        List<string> _options = new List<string>();
        public int OptionIndex => _optionIndex;
        int _optionIndex = 0;
        int _lastInput = 0;

        VirtualIntegerAxis _inputSelect;
        SpriteAnimator _portraitAnimator;

        FMOD.Studio.EventInstance _speechSound;

        public override void OnAddedToEntity()
        {
            var font = Core.Content.Load<SpriteFont>("Fonts/DefaultFont");
            _font = new NezSpriteFont(font);

            _inputSelect = new VirtualIntegerAxis();
            _inputSelect.AddGamePadDPadUpDown();
            _inputSelect.AddKeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.Up, Keys.Down);

            // using animator for the sprite, rendering here
            _portraitAnimator = Entity.AddComponent(Animator.MakeAnimator("portraits", Entity.Scene.Content));
            _portraitAnimator.RenderLayer = RenderLayers.Null;

            _speechSound = GameContent.LoadSound("Common", "speech");
        }

        public override void OnRemovedFromEntity()
        {
            _speechSound.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            _speechSound.release();
        }

        public void FeedLine(
            string line,
            string portrait = null,
            string speaker = null,
            List<string> options = null,
            Vector2? boxMargin = null,
            bool showBorder = true,
            float pitch = 0,
            bool playSound = true)
        {
            _line = line;
            _speaker = speaker;
            _sb.Clear();
            _text = new FontCharacterSource(_sb);
            _charElapsed = 0;
            _readerIndex = 0;
            _showBorder = showBorder;
            _boxMargin = boxMargin ?? BoxMarginDefault;
            _speechSound.setParameterByName("pitch", pitch);
            _playSound = playSound;
            if (_line == null)
                _speechSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

            if (options != null && options.Count > 0)
            {
                foreach (var option in options)
                    _options.Add(BuildOption(option));
                _optionIndex = 0;
            }
            else
            {
                _options.Clear();
            }

            _portraitAnimator.Play(portrait ?? "empty", SpriteAnimator.LoopMode.ClampForever);
        }

        public string CurrentLine => _line;

        StringBuilder _optionBuilder = new StringBuilder(100);
        string BuildOption(string option)
        {
            _optionBuilder.Clear();
            var ci = 0;
            while (ci < option.Length)
            {
                var nextChar = option[ci++];
                _optionBuilder.Append(nextChar);
                // check if we need a line break
                if (nextChar == ' ')
                {
                    var ti = ci;
                    var tempChar = option[ti++];
                    var length = 0;
                    while (tempChar != ' ' && ti < option.Length)
                    {
                        _optionBuilder.Append(tempChar);
                        ++length;
                        tempChar = option[ti++];
                    }
                    var size = _font.MeasureString(_optionBuilder);
                    if (size.X > OptionsWidth)
                    {
                        _optionBuilder.Remove(ci - 1, length + 1);
                        _optionBuilder.Append("\n");
                    }
                    else
                    {
                        _optionBuilder.Remove(ci, length);
                    }
                }
            }
            return _optionBuilder.ToString();
        }

        bool HasPortrait => !_portraitAnimator.IsAnimationActive("empty");

        public void Update()
        {
            if (_line == null) return;

            var lineWidth = Constants.ResWidth - (_boxMargin.X + InnerMargin.X) * 2 - (HasPortrait ? (PortraitSize.X + InnerMargin.X * 2) : 0);
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

            if (_readerIndex >= _line.Length)
                _speechSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            else if (_playSound && _speechSound.getPlaybackState(out var playbackState) == FMOD.RESULT.OK && playbackState != FMOD.Studio.PLAYBACK_STATE.PLAYING)
                _speechSound.start();

            _charElapsed += Time.DeltaTime;

            var selectInput = _inputSelect.Value;
            if (_options.Count > 0 && selectInput != 0 && selectInput != _lastInput)
            {
                _optionIndex = Mathf.Clamp(_optionIndex + selectInput, 0, _options.Count - 1);
            }
            _lastInput = selectInput;

            _portraitAnimator.LocalOffset = _boxMargin + InnerMargin + PortraitSize / 2;
        }

        public override void Render(Batcher batcher, Camera camera)
        {
            if (_line != null)
            {
                var boxBounds = new RectangleF(
                    _boxMargin,
                    new Vector2(
                        Constants.ResWidth - _boxMargin.X * 2,
                        PortraitSize.Y + 2 * InnerMargin.Y));
                batcher.DrawRect(boxBounds, Color.Black);
                if (_showBorder)
                    batcher.DrawHollowRect(boxBounds, Color.White, 2);
                _font.DrawInto(
                    batcher,
                    ref _text,
                    _boxMargin + InnerMargin
                    + (HasPortrait ? new Vector2(PortraitSize.X + InnerMargin.X * 2, 0) : Vector2.Zero),
                    Color.White,
                    0,
                    Vector2.Zero,
                    Vector2.One,
                    SpriteEffects.None,
                    _layerDepth);

                if (!_portraitAnimator.IsAnimationActive("empty"))
                {
                    var portraitBounds = new RectangleF(_boxMargin + InnerMargin, PortraitSize + Vector2.One);
                    batcher.DrawRect(portraitBounds, new Color(0xff202020));
                    batcher.Draw(_portraitAnimator.Sprite, _boxMargin + InnerMargin,
                        Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
                    batcher.DrawHollowRect(portraitBounds, Color.White);
                }

                if (_speaker != null)
                {
                    var speakerSize = _font.MeasureString(_speaker);
                    //batcher.DrawRect(
                    //    new RectangleF(_boxMargin + InnerMargin, speakerSize),
                    //    Color.Black);
                    var speaker = new FontCharacterSource(_speaker);
                    _font.DrawInto(
                        batcher,
                        ref speaker,
                        _boxMargin + InnerMargin + new Vector2(0, PortraitSize.Y - speakerSize.Y),
                        Color.White,
                        0,
                        Vector2.Zero,
                        Vector2.One,
                        SpriteEffects.None,
                        0);
                }
            }

            if (_options.Count > 0)
            {
                var optionsHeight = 0f;
                foreach (var option in _options)
                    optionsHeight += _font.MeasureString(option).Y;

                var boxBounds = new RectangleF(
                    OptionsOffset,
                    new Vector2(
                        OptionsWidth + (OptionsMargin.X * 2),
                        optionsHeight + (OptionsSpacing * (_options.Count - 1)) + (OptionsMargin.Y * 2)));
                batcher.DrawRect(boxBounds, Color.Black);
                batcher.DrawHollowRect(boxBounds, Color.White, 2);

                var offsetY = 0f;
                for (var i = 0; i < _options.Count; ++i)
                {
                    var option = _options[i];
                    var optionText = new FontCharacterSource(option);
                    _font.DrawInto(
                        batcher,
                        ref optionText,
                        OptionsOffset + OptionsMargin + new Vector2(0, offsetY),
                        i == _optionIndex ? Color.Yellow : Color.White,
                        0,
                        Vector2.Zero,
                        Vector2.One,
                        SpriteEffects.None,
                        _layerDepth);
                    offsetY += _font.MeasureString(option).Y + OptionsSpacing;
                }
            }
        }
    }
}
