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
            anim.Play("idle");
            anim.RenderLayer = -10;

            var hitbox = entity.AddComponent(new BoxCollider(16, 32));
            hitbox.PhysicsLayer = Mask.Player;
            hitbox.CollidesWithLayers = Mask.EnemyAttack;

            var mover = entity.AddComponent<PlatformerMover>();
            mover.Collider = hitbox;

            var player = entity.AddComponent<Player>();
            player.Hitbox = hitbox;
            player.JumpSound = GameContent.LoadSound("Player", "jump");
            player.HurtSound = GameContent.LoadSound("Player", "hurt");
            player.DeathSound = GameContent.LoadSound("Player", "death");
            player.DodgeSound = GameContent.LoadSound("Player", "dodge");

            var cutscene = entity.AddComponent<CutsceneController>();
            cutscene.OnPossess = self =>
            {
                player.SetEnabled(false);
                hitbox.PhysicsLayer = 0;
                hitbox.CollidesWithLayers = 0;
            };
            cutscene.OnRelease = self =>
            {
                player.SetEnabled(true);
                hitbox.PhysicsLayer = Mask.Player;
                hitbox.CollidesWithLayers = Mask.EnemyAttack;
            };

            var followCamera = entity.AddComponent(new FollowCamera(entity, FollowCamera.CameraStyle.CameraWindow));
            followCamera.FollowLerp = 1f;

            var light = entity.AddComponent(new StencilLight(200f, Color.White, 0.5f));
            light.RenderLayer = RenderLayers.Light;

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
            sentry.FireSound = GameContent.LoadSound("Common", "projectile");
            sentry.DeathSound = GameContent.LoadSound("Common", "sentry_death");

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
            switchC.TurningOff = "wallSwitchOff";
            switchC.TurningOn = "wallSwitchOn";
            switchC.Off = "wallSwitchOff";
            switchC.On = "wallSwitchOn";
            switchC.StateVar = stateVar;

            var interactable = entity.AddComponent<Interactable>();
            interactable.Prompt = "Flip";
            interactable.OnInteract = (self, interactor) =>
            {
                var sw = self.GetComponent<Switch>();
                var scriptVars = self.Entity.Scene.GetScriptVars();
                var stateVal = scriptVars.Get<bool>(sw.StateVar);
                scriptVars.Set(sw.StateVar, !stateVal);
            };

            return entity;
        }

        public static Entity CreateDoor(this Scene scene, Vector2 position, string stateVar)
        {
            var entity = scene.CreateEntity("door", position);

            var anim = entity.AddComponent(Animator.MakeAnimator("doodads", scene.Content));
            anim.Play("doorReverse", SpriteAnimator.LoopMode.ClampForever);

            var collider = entity.AddComponent(new BoxCollider(6, 48));
            collider.PhysicsLayer = Mask.Terrain;
            entity.AddComponent(new AddedToEntity(en =>
            {
                collider.Enabled = !en.Scene.GetScriptVars().Get<bool>(stateVar);
            }));

            var switchC = entity.AddComponent<Switch>();
            switchC.TurningOff = "doorReverse";
            switchC.TurningOn = "door";
            switchC.Off = "door_closed";
            switchC.On = "door_open";
            switchC.StateVar = stateVar;
            switchC.OnSwitch = (Switch self, bool state) =>
            {
                collider.Enabled = !state;
            };

            return entity;
        }

        public static Entity CreateChest(this Scene scene, Vector2 position, OgmoEntity ogmoEntity)
        {
            var entity = scene.CreateEntity(ogmoEntity.values["name"], position);

            var renderer = entity.AddComponent<SpriteRenderer>();
            renderer.Sprite = GameContent.LoadSprite("doodads", "chest_closed", scene.Content);

            var collider = entity.AddComponent(new BoxCollider(2, 48));
            collider.PhysicsLayer = Mask.Interaction;

            var chest = entity.AddComponent<Chest>();
            chest.ClosedSprite = GameContent.LoadSprite("doodads", "chest_closed", scene.Content);
            chest.OpenSprite = GameContent.LoadSprite("doodads", "chest_open", scene.Content);
            chest.Collider = collider;
            chest.ContentsVar = ogmoEntity.values["contents"];

            var interactable = entity.AddComponent<Interactable>();
            interactable.Prompt = "Open";
            interactable.OnInteract = (self, interactor) =>
            {
                var vars = self.Entity.Scene.GetScriptVars();
                var contents = vars.Get<ChestContents>(self.GetComponent<Chest>().ContentsVar);
                var inventory = self.Entity.Scene.GetPlayerInventory();
                foreach (var item in contents.Items)
                {
                    for (var i = 0; i < item.Quantity; ++i)
                        inventory.Add(item.Item);
                }
                vars.Get<List<string>>(Vars.OpenChests).Add(self.Entity.Name);
            };

            return entity;
        }

        public static Entity CreateProjectile(this Scene scene, Vector2 position)
        {
            var entity = scene.CreateEntity("projectile", position);

            var anim = entity.AddComponent(Animator.MakeAnimator("doodads", scene.Content));
            anim.Play("projectile");
            anim.RenderLayer = -6;

            var collider = entity.AddComponent(new BoxCollider(8, 8));
            collider.PhysicsLayer = Mask.EnemyAttack;
            collider.CollidesWithLayers = Mask.Player | Mask.Terrain;

            entity.AddComponent<PlatformerMover>();

            var projectile = entity.AddComponent<Projectile>();

            var hurtable = entity.AddComponent<Hurtable>();
            hurtable.Collider = collider;
            hurtable.PauseTime = 0;
            hurtable.OnHurt = projectile.OnHurt;
            hurtable.HurtSound.clearHandle();

            var light = entity.AddComponent(new StencilLight(16f, Color.White, 0.2f));
            light.RenderLayer = -RenderLayers.Light;

            return entity;
        }

        public static Entity CreateLaser(this Scene scene, Vector2 position)
        {
            var entity = scene.CreateEntity("laser", position);

            var anim = entity.AddComponent(Animator.MakeAnimator("doodads", scene.Content));
            anim.Play("laser");
            anim.RenderLayer = -6;

            var collider = entity.AddComponent(new BoxCollider(8, 8));
            collider.PhysicsLayer = Mask.PlayerAttack;
            collider.CollidesWithLayers = Mask.Enemy | Mask.Terrain;

            entity.AddComponent<PlatformerMover>();

            var projectile = entity.AddComponent<Projectile>();

            var hurtable = entity.AddComponent<Hurtable>();
            hurtable.Collider = collider;
            hurtable.PauseTime = 0;
            hurtable.OnHurt = projectile.OnHurt;
            hurtable.HurtSound.clearHandle();

            entity.AddComponent<Damage>();

            var light = entity.AddComponent(new StencilLight(16f, Color.White, 0.2f));
            light.RenderLayer = -7;

            return entity;
        }

        public static Entity CreateProjectile(
            this Scene scene,
            Vector2 position,
            Vector2 speed,
            string animation,
            int physicsLayer,
            int collidesWithLayers)
        {
            scene.CreateFlash(position, Color.AliceBlue);

            var entity = scene.CreateEntity("projectile", position);

            var anim = entity.AddComponent(Animator.MakeAnimator("doodads", scene.Content));
            anim.Play(animation);
            anim.RenderLayer = -6;

            var collider = entity.AddComponent(new BoxCollider(8, 8));
            collider.PhysicsLayer = physicsLayer;
            collider.CollidesWithLayers = collidesWithLayers;

            var mover = entity.AddComponent<PlatformerMover>();
            mover.Speed = speed;

            var projectile = entity.AddComponent<Projectile>();

            var hurtable = entity.AddComponent<Hurtable>();
            hurtable.Collider = collider;
            hurtable.PauseTime = 0;
            hurtable.OnHurt = projectile.OnHurt;
            hurtable.HurtSound.clearHandle();

            entity.AddComponent<Damage>();

            var light = entity.AddComponent(new StencilLight(16f, Color.White, 0.2f));
            light.RenderLayer = -7;

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
            cypher.FireSound = GameContent.LoadSound("Common", "projectile");
            cypher.DeathSound = GameContent.LoadSound("Common", "sentry_death");

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

            var light = entity.AddComponent(new StencilLight(16f, Color.White, 0.2f));
            light.RenderLayer = -7;

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

        public static Entity CreateBoss(this Scene scene, Vector2 position)
        {
            var entity = scene.CreateEntity("boss", position);

            var anim = entity.AddComponent(Animator.MakeAnimator("boss", scene.Content));
            anim.Play("idle");
            anim.RenderLayer = -5;

            var hitbox = entity.AddComponent(new BoxCollider(48, 48));
            hitbox.PhysicsLayer = Mask.EnemyAttack;
            hitbox.CollidesWithLayers = 0;

            var hurtbox = entity.AddComponent(new BoxCollider(64, 64));
            hurtbox.PhysicsLayer = Mask.Enemy;
            hurtbox.CollidesWithLayers = Mask.PlayerAttack;

            var mover = entity.AddComponent<PlatformerMover>();
            mover.Collider = hurtbox;

            var hurtable = entity.AddComponent<Hurtable>();
            hurtable.Collider = hurtbox;

            var boss = entity.AddComponent<MechBoss>();
            boss.Hitbox = hitbox;
            boss.FireSound = GameContent.LoadSound("Common", "projectile");
            boss.DeathSound = GameContent.LoadSound("Common", "sentry_death");
            hurtable.OnHurt = boss.OnHurt;

            var cutscene = entity.AddComponent<CutsceneController>();
            cutscene.OnPossess = self =>
            {
                boss.SetEnabled(false);
                hurtbox.PhysicsLayer = 0;
                hurtbox.CollidesWithLayers = 0;
            };
            cutscene.OnRelease = self =>
            {
                boss.SetEnabled(true);
                hurtbox.PhysicsLayer = Mask.Enemy;
                hurtbox.CollidesWithLayers = Mask.PlayerAttack;
            };

            return entity;
        }

        public static Entity CreateArea(this Scene scene, Vector2 position, OgmoEntity mapEntity)
        {
            var entity = scene.CreateEntity(mapEntity.values["name"], position);

            var collider = entity.AddComponent(new BoxCollider(0, 0, mapEntity.width, mapEntity.height));
            collider.PhysicsLayer = Mask.Area;
            collider.CollidesWithLayers = 0;
            collider.IsTrigger = true;

            return entity;
        }

        public static Entity CreateStasisChamber(this Scene scene, Vector2 position)
        {
            var entity = scene.CreateEntity("stasis_chamber", position);

            var anim = entity.AddComponent(Animator.MakeAnimator("doodads", scene.Content));
            anim.Play("stasis_chamber_closed");
            anim.RenderLayer = -1;

            return entity;
        }

        public static Entity CreateDarkLord(this Scene scene, Vector2 position)
        {
            var entity = scene.CreateEntity("dark_lord", position);

            var anim = entity.AddComponent(Animator.MakeAnimator("dark_lord", scene.Content));
            anim.Play("idle");
            anim.RenderLayer = -5;

            var hitbox = entity.AddComponent(new BoxCollider(16, 32));
            hitbox.PhysicsLayer = Mask.Enemy;
            hitbox.CollidesWithLayers = Mask.PlayerAttack;

            var mover = entity.AddComponent<PlatformerMover>();
            mover.Collider = hitbox;

            var hurtable = entity.AddComponent<Hurtable>();
            hurtable.Collider = hitbox;


            var darkLord = entity.AddComponent<DarkLord>();
            hurtable.OnHurt = darkLord.OnHurt;

            var cutscene = entity.AddComponent<CutsceneController>();
            cutscene.WalkSpeed = 50f;
            cutscene.OnPossess = self =>
            {
                self.GetComponent<DarkLord>().SetEnabled(false);
            };
            cutscene.OnRelease = self =>
            {
                self.GetComponent<DarkLord>().SetEnabled(true);
            };

            return entity;
        }

        static Dictionary<string, int> _goblinOverrides = new Dictionary<string, int>
        {
            { "idle", 3 },
        };

        public static Entity CreateGoblin(this Scene scene, Vector2 position)
        {
            var entity = scene.CreateEntity("goblin", position);

            var anim = entity.AddComponent(Animator.MakeAnimator("goblin", scene.Content, _goblinOverrides));
            anim.Play("idle");
            anim.RenderLayer = -5;

            var hitbox = entity.AddComponent(new BoxCollider(24, 24));
            hitbox.PhysicsLayer = Mask.Enemy;
            hitbox.CollidesWithLayers = Mask.PlayerAttack;

            var mover = entity.AddComponent<PlatformerMover>();
            mover.Collider = hitbox;

            var hurtable = entity.AddComponent<Hurtable>();
            hurtable.Collider = hitbox;

            var goblin = entity.AddComponent<Goblin>();
            hurtable.OnHurt = goblin.OnHurt;

            var cutscene = entity.AddComponent<CutsceneController>();
            cutscene.WalkSpeed = 100f;
            cutscene.OnPossess = self =>
            {
                self.GetComponent<Goblin>().SetEnabled(false);
            };
            cutscene.OnRelease = self =>
            {
                self.GetComponent<Goblin>().SetEnabled(true);
            };

            return entity;
        }

        public static Entity CreateFlatDoor(this Scene scene, Vector2 position, OgmoEntity ogmoEntity)
        {
            var entity = scene.CreateEntity(ogmoEntity.values["name"], position);

            var stateVar = ogmoEntity.values["state_var"];

            var anim = entity.AddComponent(Animator.MakeAnimator("doodads", scene.Content));
            anim.Play("flat_door_closed");
            anim.LocalOffset = new Vector2(0, -16);

            var switchC = entity.AddComponent<Switch>();
            switchC.TurningOff = "flat_door_openingReverse"; // TODO: use snake case throughout
            switchC.TurningOn = "flat_door_opening";
            switchC.Off = "flat_door_closed";
            switchC.On = "flat_door_open";
            switchC.StateVar = stateVar;

            var interactable = entity.AddComponent<Interactable>();
            interactable.Prompt = "Enter";
            var room = ogmoEntity.values["room"];
            var area = ogmoEntity.values["area"];
            interactable.OnInteract = (self, interactor) =>
            {
                var mainScene = self.Entity.GetMainScene();
                mainScene.MoveToArea(room, area);
            };

            var collider = entity.AddComponent(new BoxCollider(2, 48));
            var stateVal = scene.GetScriptVars().Get<bool>(stateVar);
            collider.PhysicsLayer = stateVal ? Mask.Interaction : 0;

            var lastState = scene.GetScriptVars().Get<bool>(stateVar);
            var trigger = entity.AddComponent<Trigger>();
            trigger.Condition = self =>
            {
                var ret = false;
                var val = self.Entity.Scene.GetScriptVars().Get<bool>(stateVar);
                if (val != lastState)
                    ret = true;
                lastState = val;
                return ret;
            };
            trigger.Action = self =>
            {
                var val = self.Entity.Scene.GetScriptVars().Get<bool>(stateVar);
                collider.PhysicsLayer = val ? Mask.Interaction : 0;
            };

            return entity;
        }

        public static Entity CreateExit(this Scene scene, Vector2 position, OgmoEntity ogmoEntity)
        {
            var entity = scene.CreateEntity("exit", position);

            var collider = entity.AddComponent(new BoxCollider(ogmoEntity.width, ogmoEntity.height));
            collider.PhysicsLayer = Mask.Area;
            collider.CollidesWithLayers = Mask.Player;
            collider.IsTrigger = true;

            var room = ogmoEntity.values["room"];
            var area = ogmoEntity.values["area"];

            var trigger = entity.AddComponent<Trigger>();
            trigger.Condition = self => collider.CollidesWithAny(out _);
            trigger.Action = self =>
            {
                var mainScene = self.Entity.GetMainScene();
                mainScene.MoveToArea(room, area);
            };

            return entity;
        }

        public static Entity CreateTerminal(this Scene scene, Vector2 position, OgmoEntity ogmoEntity, string world, string room)
        {
            var entity = scene.CreateEntity(ogmoEntity.values["name"], position);

            var anim = entity.AddComponent(Animator.MakeAnimator("doodads", scene.Content));
            anim.Play("terminal");
            anim.RenderLayer = -5;

            var collider = entity.AddComponent<BoxCollider>();
            collider.PhysicsLayer = Mask.Interaction;

            var interactable = entity.AddComponent<Interactable>();
            interactable.Prompt = "Save";
            interactable.OnInteract = (self, interactor) =>
            {
                var mainScene = self.Entity.GetMainScene();
                Core.GetGlobalManager<SaveSystem>().Save(mainScene.SaveSlot, world, room, self.Entity.Name);
            };

            return entity;
        }
    }
}
