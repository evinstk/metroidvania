using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;
using Nez;
using System.Collections.Generic;

namespace Game.Scripting
{
    class ChestContentsProxy
    {
        ChestContents Contents;

        [MoonSharpHidden]
        public ChestContentsProxy(ChestContents contents)
        {
            Contents = contents;
        }

        public void Add(string itemName, int quantity = 1)
        {
            var item = Item.Get(itemName);
            Contents.Items.Add(new ChestItem
            {
                Item = item,
                Quantity = quantity,
            });
        }
    }

    class SceneProxy
    {
        MainScene Scene;

        [MoonSharpHidden]
        public SceneProxy(MainScene scene)
        {
            Scene = scene;
        }

        public void SetLetterbox(float size, float duration)
        {
            var letterbox = Scene.GetPostProcessor<CinematicLetterboxPostProcessor>();
            letterbox.Tween("LetterboxSize", size, duration).SetRecycleTween(false).Start();
        }

        public void SetFade(int value, float duration)
        {
            var fadeRenderer = Scene.FadeRenderer;
            var color = new Color(value, value, value, 255);
            if (duration == 0)
                fadeRenderer.RenderTargetClearColor = color;
            else
                fadeRenderer.Tween("RenderTargetClearColor", color, duration).SetRecycleTween(false).Start();
        }

        public void ShowHud(bool show)
        {
            Scene.FindComponentOfType<Hud>().Enabled = show;
        }

        public void Create(string name, int x, int y)
        {
            // TODO: run same code/switch that room loading runs
            switch (name)
            {
                case "player":
                    Scene.CreatePlayer(new Vector2(x, y));
                    break;
                default:
                    Debug.Log($"Unknown entity type {name}");
                    break;
            }
        }

        public void LoadWorld(string world)
        {
            Core.StartSceneTransition(new FadeTransition(() => new MainScene(world)));
        }
    }

    class EntityProxy
    {
        Entity Entity;

        [MoonSharpHidden]
        public EntityProxy(Entity entity)
        {
            Entity = entity;
        }

        public Vector2 GetPosition() => Entity.Position;

        public bool InArea(string areaName)
        {
            var area = Entity.Scene.FindEntity(areaName)?.GetComponent<Collider>();
            if (area == null)
            {
                Debug.Log($"No area {areaName} found or configured.");
                return false;
            }
            return area.Bounds.Contains(Entity.Position);
        }

        List<IHealthy> _healthy = new List<IHealthy>();
        public int GetHealth()
        {
            _healthy.Clear();
            Entity.GetComponents(_healthy);
            return _healthy.Count > 0 ? _healthy[0].Health : 0;
        }

        public void Possess()
        {
            var ctrl = Entity.GetComponent<CutsceneController>();
            if (ctrl == null)
            {
                Debug.Log($"No {typeof(CutsceneController).Name} on ${Entity.Name}");
                return;
            }
            ctrl.Possess();
        }

        public void Release()
        {
            var ctrl = Entity.GetComponent<CutsceneController>();
            if (ctrl == null)
            {
                Debug.Log($"No {typeof(CutsceneController).Name} on ${Entity.Name}");
                return;
            }
            ctrl.Release();
        }
    }
}
