using MoonSharp.Interpreter;
using System;

namespace Game.Scripting
{
    class ChestContentsProxy
    {
        ChestContents Contents;

        [MoonSharpHidden]
        public ChestContentsProxy(ChestContents contents)
        {
            Contents = contents;
        }

        public void Add(string item, int quantity = 1)
        {
            var weapons = Enum.GetValues(typeof(WeaponTypes));
            foreach (WeaponTypes weapon in weapons)
            {
                var name = Enum.GetName(typeof(WeaponTypes), weapon);
                if (name == item)
                {
                    Contents.Items.Add(new WeaponChestItem
                    {
                        Weapon = weapon,
                        Quantity = quantity,
                    });
                    break;
                }
            }
        }
    }
}
