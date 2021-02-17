using Nez;
using Nez.Textures;
using System.Collections.Generic;

namespace Game
{
    enum WeaponTypes
    {
        Baton,
    }

    class Weapon
    {
        public WeaponTypes Type;
        public string Name;
        public string Description;
        public int Damage;
        public Sprite Icon => GameContent.LoadSprite(_pack, _frameName, Core.Content);

        string _pack;
        string _frameName;

        public Weapon(string pack, string frameName)
        {
            _pack = pack;
            _frameName = frameName;
        }

        public static readonly Dictionary<WeaponTypes, Weapon> Types = new Dictionary<WeaponTypes, Weapon>
        {
            {
                WeaponTypes.Baton,
                new Weapon("hud", "baton")
                {
                    Name = "Guard Baton",
                    Description = "Light baton commonly used by police units.",
                    Damage = 5,
                }
            }
        };
    }
}
