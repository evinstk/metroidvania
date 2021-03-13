using Nez;
using System;

namespace Game
{
    class TriggerListener : Component, ITriggerListener
    {
        public Action<Collider, Collider> TriggerEnter;
        public Action<Collider, Collider> TriggerExit;

        public void OnTriggerEnter(Collider other, Collider local)
        {
            TriggerEnter?.Invoke(other, local);
        }

        public void OnTriggerExit(Collider other, Collider local)
        {
            TriggerExit?.Invoke(other, local);
        }
    }
}
