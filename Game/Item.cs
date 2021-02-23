using Nez;
using Nez.Sprites;
using Nez.Textures;
using System.Collections.Generic;

namespace Game
{
    abstract class Item
    {
        public virtual Sprite Icon { get; }
        public string Name;

        public static Item Get(string name)
        {
            var item = _items.Find(i => i.Name == name);
            Insist.IsNotNull(item);
            return item;
        }

        static readonly List<Item> _items = new List<Item>
        {
            // melee
            new Weapon("hud", "baton", "player", "baton")
            {
                AttackType = AttackTypes.Light,
                Name = "Guard Baton",
                Description = "Light baton commonly used by police units.",
                Damage = 5,
            },

            // ranged
            new RangedWeapon("hud", "blaster", "blaster")
            {
                Name = "Blaster",
                Description = "Common weak blaster gun.",
                Damage = 3,
                FireTimeout = 0.2f,
                ProjectileSpeed = 250f,
            },
        };
    }

    enum AttackTypes
    {
        Light,
        Heavy,
    }

    class Weapon : Item
    {
        public AttackTypes AttackType;
        public string Description;
        public int Damage;
        public override Sprite Icon => GameContent.LoadSprite(_iconPack, _iconFrameName, Core.Content);
        public SpriteAnimation Animation => GameContent.LoadAnimation(_animationPack, _animationName, Core.Scene.Content);

        string _iconPack;
        string _iconFrameName;
        string _animationPack;
        string _animationName;

        public Weapon(string iconPack, string iconFrameName, string animationPack, string animationName)
        {
            _iconPack = iconPack;
            _iconFrameName = iconFrameName;
            _animationPack = animationPack;
            _animationName = animationName;
        }
    }

    class RangedWeapon : Item
    {
        public string Description;
        public int Damage;
        public float FireTimeout;
        public float ProjectileSpeed;

        public override Sprite Icon => GameContent.LoadSprite(_iconPack, _iconFrameName, Core.Content);

        string _iconPack;
        string _iconFrameName;
        string _animationPack;

        public RangedWeapon(string iconPack, string iconFrameName, string animationPack)
        {
            _iconPack = iconPack;
            _iconFrameName = iconFrameName;
            _animationPack = animationPack;
        }

        public void AddAnimations(SpriteAnimator animator)
        {
            var animations = Animator.MakeAnimations(_animationPack, Core.Scene.Content);
            foreach ((var key, var animation) in animations)
                animator.AddAnimation(key, animation);
        }
    }
}
