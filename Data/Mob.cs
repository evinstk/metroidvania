using Microsoft.Xna.Framework.Content;

namespace Data
{
    public class Mob
    {
        public string Type;
        public string Animator;
        public int MoveSpeed;
        public Point ColliderSize;
        public int Health;
        [ContentSerializer(Optional = true)]
        public string AiType;
    }
}
