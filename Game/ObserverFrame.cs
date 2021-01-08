using Microsoft.Xna.Framework;
using Nez;
using Nez.Textures;

namespace Game
{
    class ObserverFrame : IFrame
    {
        public Sprite Sprite;
        public bool Flip;
        public Rectangle[] HitBoxes;

        public void OnEnter()
        {
        }
    }
}
