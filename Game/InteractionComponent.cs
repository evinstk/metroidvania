using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    class InteractionComponent : RenderableComponent, IUpdatable
    {
        public int MsPerChar = 5;

        public override float Width => 1;
        public override float Height => 1;

        public bool InDialog => _lineReader != null;

        ControllerComponent _controller;
        MobMover _mover;
        NezSpriteFont _font;

        StringBuilder _sb = new StringBuilder(1000);
        FontCharacterSource _text;

        IEnumerator<Line> _lineReader;
        Vector2 _linePosition;
        int _readerIndex;
        float _charElapsed;

        public override void OnAddedToEntity()
        {
            _controller = Entity.GetComponent<ControllerComponent>();
            Insist.IsNotNull(_controller);
            _mover = Entity.GetComponent<MobMover>();
            Insist.IsNotNull(_mover);

            var font = Core.Content.Load<SpriteFont>("Fonts/DefaultFont");
            _font = new NezSpriteFont(font);
        }

        public void Update()
        {
            var currentLine = _lineReader?.Current;
            if (currentLine != null)
            {
                var currSpeech = _lineReader?.Current.Speech;
                var charIndex = (int)(_charElapsed * 1000 / MsPerChar);
                while (currSpeech != null && _readerIndex <= charIndex && _readerIndex < currSpeech.Length)
                {
                    _sb.Append(currSpeech[_readerIndex++]);
                    _text = new FontCharacterSource(_sb);
                }
                if (_charElapsed == 0 && currentLine.Sound != null)
                {
                    var sound = Entity.Scene.Content.Load<SoundEffect>("Sounds/" + currentLine.Sound);
                    sound.Play();
                }
                _charElapsed += Time.DeltaTime;
            }

            if (_controller.InteractPressed)
            {
                if (_lineReader != null)
                {
                    AdvanceDialog();
                    return;
                }

                var hit = Physics.Linecast(
                    Entity.Position,
                    Entity.Position + new Vector2(_mover.Facing * 150, 0));
                var dialogC = hit.Collider?.GetComponent<DialogComponent>();
                if (dialogC != null)
                {
                    _lineReader = dialogC.FeedLines().GetEnumerator();
                    _linePosition = dialogC.Entity.Position + new Vector2(-50, -150);
                    AdvanceDialog();
                    return;
                }
            }
        }

        void AdvanceDialog()
        {
            Insist.IsNotNull(_lineReader);
            _sb.Clear();
            _text = new FontCharacterSource(_sb);
            _charElapsed = 0;
            _readerIndex = 0;
            if (!_lineReader.MoveNext())
            {
                _lineReader = null;
            }
        }

        public override void Render(Batcher batcher, Camera camera)
        {
            if (_text.Length > 0)
            {
                _font.DrawInto(
                    batcher,
                    ref _text,
                    _linePosition,
                    //camera.ScreenToWorldPoint(Vector2.Zero),
                    Color.White,
                    0,
                    Vector2.Zero,
                    Vector2.One,
                    SpriteEffects.None,
                    0);
            }
        }
    }
}
