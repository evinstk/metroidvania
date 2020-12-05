using Nez;
using System;

namespace Game
{
    class TriggerListener : Component, ITriggerListener
    {
        public Action TriggerEnter { get;  set; }
        public Action TriggerExit { get; set; }

        public void OnTriggerEnter(Collider other, Collider local)
        {
            TriggerEnter?.Invoke();
        }

        public void OnTriggerExit(Collider other, Collider local)
        {
            TriggerExit?.Invoke();
        }
    }
}
