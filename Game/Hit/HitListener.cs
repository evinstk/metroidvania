using Game.Editor.Prefab;
using Game.Editor.Scriptable;
using Nez;

namespace Game.Hit
{
    class HitListenerData : DataComponent
    {
        public IntReference CurrHealth = new IntReference();

        public override void AddToEntity(Entity entity)
        {
            var listener = entity.AddComponent<HitListener>();
            listener.CurrHealth = CurrHealth.Dereference();
        }
    }

    class HitListener : Component, ITriggerListener
    {
        public IntValue CurrHealth;

        public void OnTriggerEnter(Collider other, Collider local)
        {
            if (--CurrHealth.RuntimeValue <= 0)
            {
                Entity.Parent.Entity.Destroy();
                var scene = Entity.Scene as RoomScene;
                Core.Schedule(2f, (ITimer timer) =>
                {
                    Core.StartSceneTransition(new FadeTransition(() =>
                        scene.SaveSlotIndex != -1
                        ? new RoomScene(scene.SaveSlotIndex)
                        : new RoomScene(scene.WorldRoomId)));
                });
            }
        }

        public void OnTriggerExit(Collider other, Collider local)
        {
        }
    }
}
