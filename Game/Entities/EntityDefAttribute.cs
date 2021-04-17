using System;

namespace Game.Entities
{
    [AttributeUsage(AttributeTargets.Class)]
    class EntityDefAttribute : Attribute
    {
        public readonly string Name;
        public EntityDefAttribute(string name)
        {
            Name = name;
        }
    }
}
