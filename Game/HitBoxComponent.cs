using Nez;
using System.Collections.Generic;

namespace Game
{
    class HitBoxComponent : Component, IUpdatable
    {
        public BoxCollider Collider;

        HashSet<Collider> _hits = new HashSet<Collider>();
        List<ITriggerListener> _tempTriggerList = new List<ITriggerListener>();

        public HitBoxComponent(BoxCollider collider)
        {
            Collider = collider;
        }

        public override void OnEnabled()
        {
            Collider.SetEnabled(true);
            _hits.Clear();
        }

        public override void OnDisabled()
        {
            Collider.SetEnabled(false);
        }

        public void Update()
        {
            var neighbors = Physics.BoxcastBroadphase(Collider.Bounds, Collider.CollidesWithLayers);
            foreach (var neighbor in neighbors)
            {
                if (!neighbor.IsTrigger)
                    continue;

                if (Collider.Overlaps(neighbor))
                {
                    var shouldReportTriggerEvent = !_hits.Contains(neighbor);
                    if (shouldReportTriggerEvent)
                        NotifyTriggerListeners(neighbor);
                    _hits.Add(neighbor);
                }
            }
        }

        void NotifyTriggerListeners(Collider other)
        {
            other.Entity.GetComponents(_tempTriggerList);
            for (var i = 0; i < _tempTriggerList.Count; i++)
                _tempTriggerList[i].OnTriggerEnter(Collider, other);
            _tempTriggerList.Clear();

            Entity.GetComponents(_tempTriggerList);
            for (var i = 0; i < _tempTriggerList.Count; i++)
                _tempTriggerList[i].OnTriggerEnter(other, Collider);
            _tempTriggerList.Clear();
        }
    }
}
