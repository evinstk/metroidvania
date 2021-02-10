﻿using Game.Movement;
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

        public void Talk()
        {
            var movement = Entity.GetComponent<PlayerMovement>();
            movement.Talk();
        }

        public void Rest()
        {
            var movement = Entity.GetComponent<PlayerMovement>();
            movement.Rest();
        }

        public void Move(ScriptEntity dest)
        {
            var destNull = dest == null;
            Debug.LogIf(destNull, "Argument dest not defined");
            if (destNull) return;

            var controller = Entity.GetComponent<FreeController>();
            if (controller == null)
            {
                Debug.Log($"No FreeController found on \"{Entity.Name}\"");
                return;
            }
            controller.SetXAxis(Mathf.SignThreshold(dest.Entity.Position.X - Entity.Position.X, 0));
            Debug.Log($"Moving \"{Entity.Name}\" to \"{dest.Entity.Name}\"");
        }

        public void Stop()
        {
            var controller = Entity.GetComponent<FreeController>();
            if (controller == null)
            {
                Debug.Log($"No FreeController found on \"{Entity.Name}\"");
                return;
            }
            controller.SetXAxis(0);
            Debug.Log($"Stopping \"{Entity.Name}\"");
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

            // hack to force recalculation
            var boundsL = colliderL.Bounds; var boundsR = colliderR.Bounds;
            var collides = colliderL.CollidesWith(colliderR, out _);
            return collides;
        }

        public void Destroy()
        {
            Entity.Destroy();
            Debug.Log($"Destroying \"{Entity.Name}\"");
        }
    }
}
