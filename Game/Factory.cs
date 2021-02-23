using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using System.Collections.Generic;

namespace Game
{
    static class Factory
    {
        public static Entity CreatePlayer(this Scene scene, Vector2 position)
        {
            var entity = scene.CreateEntity("player", position);

            var anim = entity.AddComponent(Animator.MakeAnimator("player", scene.Content, new Dictionary<string, int>
            {
                { "dead", 3 },
            }));
            anim.Play("walk");
            anim.RenderLayer = -10;

            var hitbox = entity.AddComponent(new BoxCollider(16, 32));
            hitbox.PhysicsLayer = Mask.Player;
            hitbox.CollidesWithLayers = Mask.Terrain | Mask.EnemyAttack;

            var mover = entity.AddComponent<PlatformerMover>();
            mover.Collider = hitbox;

            var player = entity.AddComponent<Player>();
            player.Hitbox = hitbox;

            var followCamera = entity.AddComponent(new FollowCamera(entity, FollowCamera.CameraStyle.CameraWindow));
            followCamera.FollowLerp = 1f;

            var light = entity.AddComponent(new StencilLight(200f, Color.White, 1f));
            light.RenderLayer = RenderLayer.Light;

            return entity;
        }

        public static Entity CreateSentry(this Scene scene, Vector2 position)
        {
            var entity = scene.CreateEntity("sentry", position);

            var anim = entity.AddComponent(Animator.MakeAnimator("sentry", scene.Content));
            anim.Play("idle");
            anim.RenderLayer = -5;

            var hitbox = entity.AddComponent(new BoxCollider(16, 16));
            hitbox.PhysicsLayer = Mask.Enemy;
            hitbox.CollidesWithLayers = Mask.PlayerAttack;

            var mover = entity.AddComponent<PlatformerMover>();
            mover.Collider = hitbox;

            var hurtable = entity.AddComponent<Hurtable>();
            hurtable.Collider = hitbox;
            hurtable.OnHurt = (Hurtable self, Collider attacker) =>
            {
                self.GetComponent<Sentry>().OnHurt(self, attacker);
            };

            var sentry = entity.AddComponent<Sentry>();

            return entity;
        }

        public static Entity CreateSwitch(this Scene scene, Vector2 position, string stateVar)
        {
            var entity = scene.CreateEntity("switch", position);

            var anim = entity.AddComponent(Animator.MakeAnimator("doodads", scene.Content));
            anim.Play("wallSwitchOff");

            var collider = entity.AddComponent(new BoxCollider(16, 16));
            collider.PhysicsLayer = Mask.Interaction;

            var switchC = entity.AddComponent<Switch>();
            switchC.OffAnimation = "wallSwitchOff";
            switchC.OnAnimation = "wallSwitchOn";
            switchC.StateVar = stateVar;

            return entity;
        }

        public static Entity CreateDoor(this Scene scene, Vector2 position, string stateVar)
        {
            var entity = scene.CreateEntity("door", position);

            var anim = entity.AddComponent(Animator.MakeAnimator("doodads", scene.Content));
            anim.Play("doorReverse", SpriteAnimator.LoopMode.ClampForever);

            var collider = entity.AddComponent(new BoxCollider(6, 48));
            collider.PhysicsLayer = Mask.Terrain;

            var switchC = entity.AddComponent<Switch>();
            switchC.OffAnimation = "doorReverse";
            switchC.OnAnimation = "door";
            switchC.StateVar = stateVar;
            switchC.OnSwitch = (Switch self, bool state) =>
            {
                collider.Enabled = !state;
            };

            return entity;
        }

        public static Entity CreateChest(this Scene scene, Vector2 position, OgmoEntity ogmoEntity)
        {
            var entity = scene.CreateEntity("chest", position);

            var renderer = entity.AddComponent<SpriteRenderer>();
            renderer.Sprite = GameContent.LoadSprite("doodads", "chest_closed", scene.Content);

            var collider = entity.AddComponent<BoxCollider>();
            collider.PhysicsLayer = Mask.Interaction;

            var chest = entity.AddComponent<Chest>();
            chest.ClosedSprite = GameContent.LoadSprite("doodads", "chest_closed", scene.Content);
            chest.OpenSprite = GameContent.LoadSprite("doodads", "chest_open", scene.Content);
            //chest.ContentsVar = ogmoEntity.values["contents"];
            chest.Collider = collider;
            scene.GetScriptVars().Set(ogmoEntity.values["contents"], chest.Contents);

            return entity;
        }

        public static Entity CreateProjectile(this Scene scene, Vector2 position)
        {
            var entity = scene.CreateEntity("projectile", position);

            var anim = entity.AddComponent(Animator.MakeAnimator("doodads", scene.Content));
            anim.Play("projectile");
            anim.RenderLayer = -5;

            var collider = entity.AddComponent(new BoxCollider(8, 8));
            collider.PhysicsLayer = Mask.EnemyAttack;
            collider.CollidesWithLayers = Mask.Player | Mask.Terrain;

            var hurtable = entity.AddComponent<Hurtable>();
            hurtable.Collider = collider;
            hurtable.PauseTime = 0;
            hurtable.OnHurt = (self, attacker) =>
            {
                self.Entity.Destroy();
            };

            entity.AddComponent<PlatformerMover>();

            entity.AddComponent<Projectile>();

            return entity;
        }
    }
}
