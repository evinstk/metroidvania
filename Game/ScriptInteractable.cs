using Game.Editor.Prefab;
using Nez;

namespace Game
{
    class ScriptInteractableData : DataComponent
    {
        public override void AddToEntity(Entity entity)
        {
            entity.AddComponent<ScriptInteractable>();
        }
    }

    class ScriptInteractable : Component, IInteractable, IUpdatable
    {
        public bool Interacted = false;
        bool _pendingState = false;

        public void Interact()
        {
            _pendingState = true;
        }

        public void Update()
        {
            Interacted = _pendingState;
            _pendingState = false;
        }
    }
}
