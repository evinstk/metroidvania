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
        public Sprite Icon;

        public static readonly Dictionary<WeaponTypes, Weapon> Types = new Dictionary<WeaponTypes, Weapon>
        {
            {
                WeaponTypes.Baton,
                new Weapon
                {
                    Name = "Guard Baton",
                    Description = "Light baton commonly used by police units.",
                    Damage = 5,
                }
            }
        };
    }
}
