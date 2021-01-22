using Nez;

namespace Game.Scripting
{
    class ScriptEntity
    {
        public Entity Entity;

        public ScriptEntity(Entity entity)
        {
            Entity = entity;
        }

        public bool IsInteracted()
        {
            var interactable = Entity.GetComponent<ScriptInteractable>();
            return interactable.Interacted;
        }

        public void SetEnabled(bool isEnabled)
        {
            var movement = Entity.GetComponent<ControllerComponent>();
            if (movement == null)
            {
                Debug.Log("No ControllerComponent on entity");
                return;
            }
            var interaction = Entity.GetComponent<Interaction>();
            if (interaction == null)
            {
                Debug.Log("No Interaction on entity");
                return;
            }
            movement.SetEnabled(isEnabled);
            interaction.SetEnabled(isEnabled);
        }

        public bool Collides(ScriptEntity rhs)
        {
            var rhsNull = rhs == null;
            Debug.LogIf(rhsNull, "Argument rhs not defined");
            if (rhsNull) return false;

            var colliderL = Entity.GetComponent<Collider>();
            var colliderR = rhs.Entity.GetComponent<Collider>();
            var colliderLNull = colliderL == null; var colliderRNull = colliderR == null;
            Debug.LogIf(colliderLNull, "No collider on argument \"lhs\"");
            Debug.LogIf(colliderRNull, "No collider on argument \"rhs\"");
            if (colliderLNull || colliderRNull) return false;

            return colliderL.CollidesWith(colliderR, out _);
        }

        public void Destroy()
        {
            Entity.Destroy();
            Debug.Log($"Destroying \"{Entity.Name}\"");
        }
    }
}
