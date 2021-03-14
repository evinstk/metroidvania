﻿using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;
using Nez;
using Nez.Sprites;
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

        public Entity Create(string name, int x, int y)
        {
            // TODO: run same code/switch that room loading runs
            switch (name)
            {
                case "player":
                    return Scene.CreatePlayer(new Vector2(x, y));
                default:
                    Debug.Log($"Unknown entity type {name}");
                    return null;
            }
        }

        //public void LoadWorld(string world, string room = null, string area = null)
        //{
        //    Core.StartSceneTransition(new FadeTransition(() => new MainScene(Scene.SaveSlot, world, room, area)));
        //}

        public Entity FindEntity(string name) => Scene.FindEntity(name);

        public void LoadMusic(string bankName, string evt) => Scene.FindComponentOfType<SoundSystem>().LoadMusic(bankName, evt);
        public void PlayMusic() => Scene.FindComponentOfType<SoundSystem>().PlayMusic();
        public void StopMusic(bool fadeOut = false)
        {
            var stopMode = fadeOut ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE;
            Scene.FindComponentOfType<SoundSystem>().StopMusic(stopMode);
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

        public void ChangeAnimation(string animationName, string loopModeStr = "loop")
        {
            var loopMode = loopModeStr == "loop" ? SpriteAnimator.LoopMode.Loop
                : loopModeStr == "clamp_forever" ? SpriteAnimator.LoopMode.ClampForever
                : SpriteAnimator.LoopMode.Loop;
            var animator = Entity.GetComponent<SpriteAnimator>();
            if (animator == null)
            {
                Debug.Log($"No {typeof(SpriteAnimator).Name} on {Entity.Name}");
                return;
            }
            animator.Change(animationName, loopMode);
        }

        public bool IsAnimationRunning() => Entity.GetComponent<SpriteAnimator>()?.IsRunning ?? false;

        public void Move(Vector2 dest)
        {
            var ctrl = Entity.GetComponent<CutsceneController>();
            if (ctrl == null)
            {
                Debug.Log($"No {typeof(CutsceneController).Name} on ${Entity.Name}");
                return;
            }
            ctrl.Move(dest);
        }

        public void MoveTo(string entityName)
        {
            var area = Entity.Scene.FindEntity(entityName);
            if (area == null)
            {
                Debug.Log($"No entity {entityName} found.");
                return;
            }
            var areaPos = area.Position;
            Move(new Vector2(areaPos.X, areaPos.Y));
        }
    }

    class CameraProxy
    {
        Camera Camera;

        public CameraProxy(Camera camera)
        {
            Camera = camera;
        }

        public void FocusOn(string entityName, float panDuration = 1)
        {
            var entity = Core.Scene.FindEntity(entityName);
            if (entity == null)
            {
                Debug.Log($"No entity {entityName} found.");
                return;
            }
            Camera.GetComponent<CameraController>().SetFocus(entity.Position, panDuration);
        }

        public void ReleaseFocus()
        {
            Camera.GetComponent<CameraController>().ReleaseFocus();
        }
    }
}
