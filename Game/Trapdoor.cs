using Nez;
using Nez.Sprites;
using Nez.Tiled;
using System.Linq;

namespace Game
{
    partial class MainScene : Scene
    {
        public Entity MakeTrapdoor(TmxObject tmxObject, int layerIndex)
        {
            var trapdoor = CreateEntity("trapdoor");
            trapdoor.SetPosition(tmxObject.X, tmxObject.Y);

            var trapdoorMap = Json.ReadJson<Data.TextureMap.TextureMap>("Content/Textures/trapdoor.json");

            var trapdoorRenderer = trapdoor.AddComponent<SpriteRenderer>();
            trapdoorRenderer.SetRenderLayer(layerIndex);

            var trapdoorAnimator = trapdoor.AddAnimator(trapdoorMap);
            trapdoorAnimator.Play("closed", Animator<Frame>.LoopMode.ClampForever);

            var collider = trapdoor.AddComponent<BoxCollider>();
            Flags.SetFlagExclusive(ref collider.PhysicsLayer, Layer.Doodad);

            var closed = trapdoorMap.GetGroups()["closed"].First();
            var trigger = trapdoor.AddComponent(new BoxCollider(closed.Bounds.W * .5f, 2));
            Flags.SetFlagExclusive(ref trigger.PhysicsLayer, Layer.Doodad);
            trigger.IsTrigger = true;

            var listener = trapdoor.AddComponent<TriggerListener>();
            listener.TriggerEnter = () =>
            {
                if (trapdoorAnimator.IsAnimationActive("closed"))
                {
                    trapdoorAnimator.Play("trapdoor", Animator<Frame>.LoopMode.ClampForever);
                    collider.SetEnabled(false);
                    trigger.SetEnabled(false);
                }
            };

            return trapdoor;
        }
    }
}
