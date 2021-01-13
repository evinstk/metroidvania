using Game.Editor.Scriptable;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;

namespace Game.Editor.Prefab
{
    class SwitchData : DataComponent
    {
        public SpriteRendererData OnSprite = new SpriteRendererData();
        public SpriteRendererData OffSprite = new SpriteRendererData();

        public override void AddToEntity(Entity entity)
        {
            entity.AddComponent<Switch>();
        }
    }

    class SwitchState : DataComponent
    {
        public BooleanReference State = new BooleanReference();

        public override void AddToEntity(Entity entity)
        {
            var switchC = entity.GetComponent<Switch>();
            if (switchC != null)
            {
                var boolVal = Core.GetGlobalManager<ScriptableObjectManager<BooleanValue>>().GetResource(State.Id);
                switchC.State = boolVal;
            }
            else
            {
                Debug.Log("No Switch component on entity.");
            }
        }
    }

    interface IInteractable
    {
        void Interact();
    }

    class Switch : Component, IUpdatable, IInteractable
    {
        public BooleanValue State = new BooleanValue();

        SpriteRenderer _renderer;

        public override void OnAddedToEntity()
        {
            _renderer = Entity.GetComponent<SpriteRenderer>();
        }

        public void Update()
        {
            var color = State.Value ? Color.Green : Color.DarkRed;
            _renderer.Color = color;
        }

        public void Interact()
        {
            State.Value = !State.Value;
        }
    }
}
