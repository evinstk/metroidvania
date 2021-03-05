using Nez;
using Nez.Sprites;
using System.Collections.Generic;

namespace Game
{
    class Portrait
    {
        static readonly Dictionary<string, SpriteAnimation> _portraits;

        static Portrait()
        {
            var portraits = Animator.MakeAnimations("portraits", Core.Content);
            _portraits = new Dictionary<string, SpriteAnimation>();
            foreach ((var key, var portrait) in portraits)
                _portraits.Add(key, portrait);
        }

        public static SpriteAnimation Get(string name) => _portraits[name];
    }
}
