using Nez;
using System;

namespace Game
{
    class Interactable : Component
    {
        public Action<Interactable, Entity> OnInteract;
        public string Prompt = "Interact";

        public void Interact(Entity interactor) => OnInteract?.Invoke(this, interactor);
    }
}
