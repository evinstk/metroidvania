using Nez;
using Nez.Sprites;
using Nez.Textures;
using System.Collections.Generic;

namespace Game
{
    enum WeaponTypes
    {
        Baton,
    }

    enum AttackTypes
    {
        Light,
        Heavy,
    }

    class Weapon
    {
        public WeaponTypes Type;
        public AttackTypes AttackType;
        public string Name;
        public string Description;
        public int Damage;
        public Sprite Icon => GameContent.LoadSprite(_iconPack, _iconFrameName, Core.Content);
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

        public static readonly Dictionary<WeaponTypes, Weapon> Types = new Dictionary<WeaponTypes, Weapon>
        {
            {
                WeaponTypes.Baton,
                new Weapon("hud", "baton", "player", "baton")
                {
                    Type = WeaponTypes.Baton,
                    AttackType = AttackTypes.Light,
                    Name = "Guard Baton",
                    Description = "Light baton commonly used by police units.",
                    Damage = 5,
                }
            }
        };
    }
}
