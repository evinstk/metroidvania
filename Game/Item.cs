using Microsoft.Xna.Framework.Audio;
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
            new Weapon("hud", "baton", "player", "baton", "slash.wav")
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
                SoundFile = "blaster.wav",
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
        public SoundEffect Sound => _sound != null ? Core.Scene.Content.LoadSoundEffect($"{ContentPath.Sounds}{_sound}") : null;

        string _iconPack;
        string _iconFrameName;
        string _animationPack;
        string _animationName;
        string _sound;

        public Weapon(string iconPack, string iconFrameName, string animationPack, string animationName, string sound)
        {
            _iconPack = iconPack;
            _iconFrameName = iconFrameName;
            _animationPack = animationPack;
            _animationName = animationName;
            _sound = sound;
        }
    }

    class RangedWeapon : Item
    {
        public string Description;
        public int Damage;
        public float FireTimeout;
        public float ProjectileSpeed;
        public string SoundFile;

        public override Sprite Icon => GameContent.LoadSprite(_iconPack, _iconFrameName, Core.Content);
        public SoundEffect Sound => SoundFile != null ? Core.Scene.Content.LoadSoundEffect($"{ContentPath.Sounds}{SoundFile}") : null;

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
