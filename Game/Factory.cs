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
            hitbox.CollidesWithLayers = Mask.EnemyAttack;

            var mover = entity.AddComponent<PlatformerMover>();
            mover.Collider = hitbox;

            var player = entity.AddComponent<Player>();
            player.Hitbox = hitbox;
            player.JumpSound = scene.Content.LoadSoundEffect($"{ContentPath.Sounds}jump.wav");
            player.HurtSound = scene.Content.LoadSoundEffect($"{ContentPath.Sounds}hurt.wav");
            player.DeathSound = scene.Content.LoadSoundEffect($"{ContentPath.Sounds}death.wav");
            player.DodgeSound = scene.Content.LoadSoundEffect($"{ContentPath.Sounds}whoosh.wav");

            var followCamera = entity.AddComponent(new FollowCamera(entity, FollowCamera.CameraStyle.CameraWindow));
            followCamera.FollowLerp = 1f;

            var light = entity.AddComponent(new StencilLight(200f, Color.White, 0.5f));
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
            hitbox.PhysicsLayer = Mask.Enemy | Mask.EnemyAttack;
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
            sentry.Hitbox = hitbox;
            sentry.FireSound = scene.Content.LoadSoundEffect($"{ContentPath.Sounds}projectile.wav");
            sentry.DeathSound = scene.Content.LoadSoundEffect($"{ContentPath.Sounds}sentry_death.wav");

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

            var collider = entity.AddComponent(new BoxCollider(2, 48));
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

            entity.AddComponent<PlatformerMover>();

            var projectile = entity.AddComponent<Projectile>();

            var hurtable = entity.AddComponent<Hurtable>();
            hurtable.Collider = collider;
            hurtable.PauseTime = 0;
            hurtable.OnHurt = projectile.OnHurt;
            hurtable.HurtSound = null;

            var light = entity.AddComponent(new StencilLight(16f, Color.White, 0.2f));
            light.RenderLayer = -RenderLayer.Light;

            return entity;
        }

        public static Entity CreateLaser(this Scene scene, Vector2 position)
        {
            var entity = scene.CreateEntity("laser", position);

            var anim = entity.AddComponent(Animator.MakeAnimator("doodads", scene.Content));
            anim.Play("laser");
            anim.RenderLayer = -5;

            var collider = entity.AddComponent(new BoxCollider(8, 8));
            collider.PhysicsLayer = Mask.PlayerAttack;
            collider.CollidesWithLayers = Mask.Enemy | Mask.Terrain;

            entity.AddComponent<PlatformerMover>();

            var projectile = entity.AddComponent<Projectile>();

            var hurtable = entity.AddComponent<Hurtable>();
            hurtable.Collider = collider;
            hurtable.PauseTime = 0;
            hurtable.OnHurt = projectile.OnHurt;
            hurtable.HurtSound = null;

            entity.AddComponent<Damage>();

            var light = entity.AddComponent(new StencilLight(16f, Color.White, 0.2f));
            light.RenderLayer = -RenderLayer.Light;

            return entity;
        }

        static Dictionary<string, int> _cypherOverrides = new Dictionary<string, int>
        {
            { "idle", 6 },
        };

        public static Entity CreateCypher(this Scene scene, Vector2 position)
        {
            var entity = scene.CreateEntity("cypher", position);

            var anim = entity.AddComponent(Animator.MakeAnimator("cypher", scene.Content, _cypherOverrides));
            anim.Play("idle");
            anim.RenderLayer = -5;

            var hitbox = entity.AddComponent(new BoxCollider(32, 32));
            hitbox.PhysicsLayer = Mask.Enemy | Mask.EnemyAttack;
            hitbox.CollidesWithLayers = Mask.PlayerAttack;

            var mover = entity.AddComponent<PlatformerMover>();
            mover.Collider = hitbox;

            var cypher = entity.AddComponent<Cypher>();
            cypher.Hitbox = hitbox;
            cypher.DeathSound = scene.Content.LoadSoundEffect($"{ContentPath.Sounds}sentry_death.wav");

            var hurtable = entity.AddComponent<Hurtable>();
            hurtable.Collider = hitbox;
            hurtable.OnHurt = cypher.OnHurt;

            return entity;
        }

        static Dictionary<string, int> _doodadsOverrides = new Dictionary<string, int>
        {
            { "small_explosion", 24 }
        };

        public static Entity CreateBoom(this Scene scene, Vector2 position)
        {
            var entity = scene.CreateEntity("boom", position);

            var anim = entity.AddComponent(Animator.MakeAnimator("doodads", scene.Content, _doodadsOverrides));
            anim.Play("small_explosion", SpriteAnimator.LoopMode.ClampForever);
            anim.RenderLayer = -6;

            var light = entity.AddComponent(new StencilLight(8f, new Color(0x2671df), 1f));
            light.RenderLayer = RenderLayer.Light;

            Core.Schedule(anim.CurrentAnimation.GetDuration(), onTime =>
            {
                if (!entity.IsDestroyed)
                    entity.Destroy();
            });

            return entity;
        }

        public static Entity CreateFlash(this Scene scene, Vector2 position, Color color, float duration = 0.02f)
        {
            var entity = scene.CreateEntity("flash", position);

            var light = entity.AddComponent(new StencilLight(8f, color, 1f));
            light.RenderLayer = -7;

            Core.Schedule(duration, onTime =>
            {
                if (!entity.IsDestroyed)
                    entity.Destroy();
            });

            return entity;
        }
    }
}
