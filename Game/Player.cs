using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Sprites;
using System;
using System.Collections.Generic;

namespace Game
{
    class Player : Component, IUpdatable
    {
        enum States
        {
            Normal,
            Attack,
            Dodge,
            Hurt,
            Dead,
            Climb,
        }

        public Collider Hitbox;

        public FMOD.Studio.EventInstance JumpSound;
        public FMOD.Studio.EventInstance HurtSound;
        public FMOD.Studio.EventInstance DeathSound;
        public FMOD.Studio.EventInstance DodgeSound;

        public float MoveSpeed = 150f;
        public float Gravity = 600f;
        public float JumpTime = .4f;
        public float JumpSpeed = 200f;
        public float WallJumpWindowDuration = .2f;
        public float WallJumpDuration = .5f;
        public float WallJumpSpeedFactor = 1f;
        public float MaxFallSpeed = 200f;
        public float CastDistance = 32;
        public float DodgeTime = .2f;
        public float DodgeSpeed = 250f;
        public float HurtTime = 0.2f;
        public float InvincibleTime = 1.5f;
        public float KnockbackSpeed = 150f;
        public float DodgeTimeout = 0.5f;

        States _state = States.Normal;
        int _facing = 1;
        bool _onGround = false;
        float _jumpTimer = 0;
        float _wallJumpWindowTimer = 0;
        float _wallJumpTimer = 0;
        float _attackTimer = 0;
        float _fireTimer = 0;
        float _dodgeTimer = 0;
        float _dodgeTimeoutTimer = 0;
        bool _canDodge = true;
        float _hurtTimer = 0;
        int _hurtDir = 0;
        float _invincibilityTimer = 0;
        AttackTypes _attackType = AttackTypes.Light;
        BoxCollider _attackCollider;
        BoxCollider _ladderCollider;

        RangedWeapon _equippedRangedWeapon;

        bool _usingGamePad = false;

        VirtualIntegerAxis _inputX;
        VirtualIntegerAxis _inputY;
        VirtualButton _inputJump;
        VirtualButton _inputAttack;
        VirtualButton _inputDodge;
        VirtualButton _inputInteract;
        VirtualButton _inputRangedModifier;
        VirtualJoystick _inputAim;

        PlatformerMover _mover;

        SpriteAnimator _upperAnimator;
        SpriteAnimator _lowerAnimator;
        SpriteAnimator _weaponAnimator;

        ScriptVars _vars;

        Inventory PlayerInventory => _vars.Get<Inventory>(Vars.PlayerInventory);

        static Dictionary<string, int> _animOverrides = new Dictionary<string, int>
        {
            { "upper_idle", 3 },
            { "lower_idle", 3 },
            { "upper_dead", 3 },
            { "lower_dead", 3 },
            { "upper_climb", 8 },
            { "lower_climb", 8 },
        };

        public override void Initialize()
        {
            _upperAnimator = Entity.AddComponent(Animator.MakeAnimator("player", Entity.Scene.Content, _animOverrides));
            _upperAnimator.Play("upper_idle");
            _upperAnimator.RenderLayer = -10;

            _lowerAnimator = Entity.AddComponent(Animator.MakeAnimator("player", Entity.Scene.Content, _animOverrides));
            _lowerAnimator.Play("lower_idle");
            _lowerAnimator.RenderLayer = -10;
        }

        public override void OnAddedToEntity()
        {
            _inputX = new VirtualIntegerAxis();
            _inputX.Nodes.Add(new VirtualAxis.GamePadLeftStickX());
            _inputX.Nodes.Add(new VirtualAxis.KeyboardKeys(
                VirtualInput.OverlapBehavior.CancelOut, Keys.A, Keys.D));
            _inputX.Nodes.Add(new VirtualAxis.KeyboardKeys(
                VirtualInput.OverlapBehavior.CancelOut, Keys.Left, Keys.Right));

            _inputY = new VirtualIntegerAxis();
            _inputY.AddGamePadLeftStickY();
            _inputY.AddKeyboardKeys(VirtualInput.OverlapBehavior.CancelOut, Keys.W, Keys.S);
            _inputY.AddKeyboardKeys(VirtualInput.OverlapBehavior.CancelOut, Keys.Up, Keys.Down);

            _inputJump = new VirtualButton();
            _inputJump.Nodes.Add(new VirtualButton.GamePadButton(0, Buttons.A));
            _inputJump.Nodes.Add(new VirtualButton.KeyboardKey(Keys.Space));

            _inputAttack = new VirtualButton();
            _inputAttack.Nodes.Add(new VirtualButton.GamePadButton(0, Buttons.RightShoulder));
            _inputAttack.Nodes.Add(new VirtualButton.MouseLeftButton());

            _inputInteract = new VirtualButton();
            _inputInteract.Nodes.Add(new VirtualButton.GamePadButton(0, Buttons.Y));
            _inputInteract.Nodes.Add(new VirtualButton.KeyboardKey(Keys.E));

            _inputDodge = new VirtualButton();
            _inputDodge.AddGamePadButton(0, Buttons.B);
            _inputDodge.AddKeyboardKey(Keys.LeftShift);

            _inputRangedModifier = new VirtualButton();
            _inputRangedModifier.AddGamePadButton(0, Buttons.LeftShoulder);
            _inputRangedModifier.AddMouseRightButton();

            _inputAim = new VirtualJoystick(true);
            _inputAim.AddGamePadRightStick();

            _mover = Entity.GetComponent<PlatformerMover>();

            _weaponAnimator = Entity.AddComponent<SpriteAnimator>();
            _weaponAnimator.RenderLayer = _upperAnimator.RenderLayer - 1;
            _weaponAnimator.AddAnimation("empty", GameContent.LoadAnimation("doodads", "empty", Core.Content));
            _weaponAnimator.Play("empty");

            _vars = Entity.Scene.GetScriptVars();
        }

        public override void OnDisabled()
        {
            _weaponAnimator?.Play("empty");
            _upperAnimator.Play("upper_idle");
            _lowerAnimator.Play("lower_idle");
        }

        public void Update()
        {
            var flip = _facing == -1;
            _upperAnimator.FlipX = flip;
            _lowerAnimator.FlipX = flip;
            _weaponAnimator.FlipX = flip;
            _onGround = _mover.OnGround();
            var onWall = _mover.OnWall(out var wallDir);

            var equippedWeapon = PlayerInventory.EquippedWeapon;
            var lastEquippedRangedWeapon = _equippedRangedWeapon;
            _equippedRangedWeapon = PlayerInventory.EquippedRangedWeapon;

            var inputX = _inputX.Value;
            var inputY = _inputY.Value;

            if (_inputX.Nodes[0].Value != 0) _usingGamePad = true;
            if (_inputX.Nodes[1].Value != 0) _usingGamePad = false;

            if (lastEquippedRangedWeapon != _equippedRangedWeapon && _equippedRangedWeapon != null)
            {
                _equippedRangedWeapon.AddAnimations(_weaponAnimator);
            }

            if (Time.TimeScale == 0) return;

            var weaponAnimation = string.Empty;

            // NORMAL STATE
            if (_state == States.Normal)
            {
                // animation
                var rangedWeaponActive = _inputRangedModifier.IsDown && _equippedRangedWeapon != null;
                _weaponAnimator.Change(rangedWeaponActive ? "idle" : "empty");
                if (rangedWeaponActive)
                {
                    var aim = _inputAim.Value.Y;
                    if (aim <= -.98f)
                        weaponAnimation = "b";
                    else if (aim <= -.83f)
                        weaponAnimation = "bbm";
                    else if (aim <= -.56f)
                        weaponAnimation = "bm";
                    else if (aim <= -.2f)
                        weaponAnimation = "bmm";
                    else if (aim <= .2f)
                        weaponAnimation = "m";
                    else if (aim <= .56f)
                        weaponAnimation = "tmm";
                    else if (aim <= .83f)
                        weaponAnimation = "tm";
                    else if (aim <= .98f)
                        weaponAnimation = "ttm";
                    else if (aim <= 1f)
                        weaponAnimation = "t";
                    _upperAnimator.Change($"upper_{weaponAnimation}");
                    _lowerAnimator.Change($"lower_aiming");
                    _weaponAnimator.Change(weaponAnimation);
                }
                else
                {
                    _weaponAnimator.Change("empty");
                }
                if (_onGround)
                {
                    if (inputX == 0)
                    {
                        _lowerAnimator.Change("lower_idle");
                        if (!rangedWeaponActive)
                            _upperAnimator.Change("upper_idle");
                    }
                    else
                    {
                        _lowerAnimator.Change("lower_walk");
                        if (!rangedWeaponActive)
                            _upperAnimator.Change("upper_walk");
                    }
                }
                else
                {
                    _lowerAnimator.Change("lower_jump");
                    if (!rangedWeaponActive)
                        _upperAnimator.Change("upper_jump");
                }

                // horizontal movement
                {
                    if (_wallJumpTimer <= 0)
                    {
                        _mover.Speed.X = inputX * MoveSpeed;

                        if (inputX != 0)
                            _facing = inputX;
                    }
                }

                // invoke jumping
                {
                    if (_inputJump.IsPressed && _onGround)
                    {
                        _inputJump.ConsumeBuffer();
                        _jumpTimer = JumpTime;
                        JumpSound.start();
                    }
                }

                // invoke wall jumping
                {
                    if (onWall && !_onGround && _wallJumpWindowTimer > 0)
                    {
                        _wallJumpWindowTimer -= Time.DeltaTime;
                        if (_inputJump.IsPressed)
                        {
                            _inputJump.ConsumeBuffer();
                            _jumpTimer = JumpTime;
                            _wallJumpTimer = WallJumpDuration;
                            _mover.Speed.X = -wallDir * MoveSpeed * WallJumpSpeedFactor;
                            _facing = -wallDir;
                            JumpSound.start();
                        }
                    }
                }

                // invoke attacking
                {
                    if (_inputAttack.IsPressed && equippedWeapon != null && !_inputRangedModifier.IsDown)
                    {
                        _inputAttack.ConsumeBuffer();
                        _state = States.Attack;
                        _attackTimer = 0;
                        _attackType = equippedWeapon.AttackType;
                        _weaponAnimator.AddAnimation("attack", equippedWeapon.Animation);
                        equippedWeapon.Sound?.Play();

                        if (_attackCollider == null)
                        {
                            _attackCollider = Entity.AddComponent(new BoxCollider(0, 0));
                            var damage = Entity.GetOrCreateComponent<Damage>();
                            damage.Amount = equippedWeapon.Damage;
                        }
                        _attackCollider.PhysicsLayer = Mask.PlayerAttack;

                        if (_onGround)
                            _mover.Speed.X = 0;
                    }
                }

                // invoke firing
                {
                    var mana = _vars.Get<int>(Vars.PlayerMana);
                    if (_inputAttack.IsPressed && _equippedRangedWeapon?.ManaCost <= mana && _inputRangedModifier.IsDown && _fireTimer <= 0)
                    {
                        _vars[Vars.PlayerMana] = mana - _equippedRangedWeapon.ManaCost;

                        var aimVector = Vector2.Zero;
                        var projectilePos = Vector2.Zero;
                        switch (weaponAnimation)
                        {
                            case "b":
                                aimVector = new Vector2(0, 1);
                                projectilePos = new Vector2(-4, 18);
                                break;
                            case "bbm":
                                aimVector = new Vector2(.38f, .92f);
                                projectilePos = new Vector2(4, 15);
                                break;
                            case "bm":
                                aimVector = new Vector2(.71f, .71f);
                                projectilePos = new Vector2(11, 9);
                                break;
                            case "bmm":
                                aimVector = new Vector2(.92f, .38f);
                                projectilePos = new Vector2(12, 4);
                                break;
                            case "m":
                                aimVector = new Vector2(1, 0);
                                projectilePos = new Vector2(12, 0);
                                break;
                            case "tmm":
                                aimVector = new Vector2(.92f, -.38f);
                                projectilePos = new Vector2(12, -4);
                                break;
                            case "tm":
                                aimVector = new Vector2(.71f, -.71f);
                                projectilePos = new Vector2(11, -9);
                                break;
                            case "ttm":
                                aimVector = new Vector2(.38f, -.92f);
                                projectilePos = new Vector2(8, -16);
                                break;
                            case "t":
                                aimVector = new Vector2(0, -1);
                                projectilePos = new Vector2(-6, -20);
                                break;
                        }
                        aimVector.X = Math.Abs(aimVector.X) * _facing;
                        projectilePos.X = projectilePos.X * _facing;
                        projectilePos += Entity.Position;

                        Entity.Scene.CreateFlash(projectilePos, Color.AliceBlue);
                        var projectileEntity = Entity.Scene.CreateLaser(projectilePos);
                        var mover = projectileEntity.GetComponent<PlatformerMover>();
                        mover.Speed = _equippedRangedWeapon.ProjectileSpeed * aimVector;

                        var damage = projectileEntity.GetComponent<Damage>();
                        damage.Amount = _equippedRangedWeapon.Damage;
                        _equippedRangedWeapon.Sound?.Play();
                        _fireTimer = _equippedRangedWeapon.FireTimeout;
                    }
                }

                // invoke dodging
                {
                    if (_inputDodge.IsPressed && _canDodge && _dodgeTimeoutTimer <= 0)
                    {
                        _canDodge = false;
                        if (inputX != 0)
                            _facing = inputX;
                        _inputDodge.ConsumeBuffer();
                        _state = States.Dodge;
                        DodgeSound.start();
                        _dodgeTimer = 0;
                        Hitbox.CollidesWithLayers &= ~Mask.EnemyAttack;
                        Hitbox.PhysicsLayer &= ~Mask.Player;
                    }
                }

                // invoke climbing
                {
                    var ladder = Physics.OverlapRectangle(Hitbox.Bounds, Mask.Ladder);
                    if (inputY != 0 && inputX == 0 && ladder?.Bounds.Contains(Entity.Position) == true)
                    {
                        _state = States.Climb;
                        _ladderCollider = (BoxCollider)ladder;
                    }
                }
            }
            // ATTACK STATE
            else if (_state == States.Attack)
            {
                if (_attackType == AttackTypes.Light)
                {
                    _upperAnimator.Change("upper_attack", SpriteAnimator.LoopMode.ClampForever);
                    _lowerAnimator.Change("lower_attack", SpriteAnimator.LoopMode.ClampForever);
                    _weaponAnimator.Change("attack", SpriteAnimator.LoopMode.ClampForever);
                    _attackTimer += Time.DeltaTime;

                    if (_attackTimer > .1f && _attackTimer < .2f)
                    {
                        _attackCollider.SetLocalOffset(new Vector2(16, 0));
                        _attackCollider.SetSize(32, 32);
                    }
                    else if (_attackTimer > .1f && _attackCollider != null)
                    {
                        Entity.RemoveComponent(_attackCollider);
                        _attackCollider = null;
                    }

                    if (_onGround)
                        _mover.Speed.X = 0;

                    if (_facing < 0 && _attackCollider != null)
                    {
                        var offset = _attackCollider.LocalOffset;
                        _attackCollider.LocalOffset = new Vector2(-offset.X, offset.Y);
                    }

                    if (!_upperAnimator.IsRunning)
                    {
                        _upperAnimator.Change("upper_idle");
                        _lowerAnimator.Change("lower_idle");
                        _weaponAnimator.Play("empty");
                        _state = States.Normal;
                    }
                }
                // handle other weapon attack types
                else
                {
                    throw new NotImplementedException($"Attack type {Enum.GetName(typeof(AttackTypes), _attackType)} not implemented.");
                }
            }
            // DODGE STATE
            else if (_state == States.Dodge)
            {
                _upperAnimator.Change("upper_dodge", SpriteAnimator.LoopMode.ClampForever);
                _lowerAnimator.Change("lower_dodge", SpriteAnimator.LoopMode.ClampForever);
                _weaponAnimator.Change("empty");
                _dodgeTimer += Time.DeltaTime;
                _mover.Speed.X = _facing * DodgeSpeed;
                _mover.Speed.Y = 0;

                if (_dodgeTimer >= DodgeTime)
                {
                    _upperAnimator.Change("upper_idle");
                    _lowerAnimator.Change("lower_idle");
                    _weaponAnimator.Change("empty");
                    _state = States.Normal;
                    _dodgeTimeoutTimer = DodgeTimeout;
                    Hitbox.CollidesWithLayers |= Mask.EnemyAttack;
                    Hitbox.PhysicsLayer |= Mask.Player;
                }
            }
            // CLIMB STATE
            else if (_state == States.Climb)
            {
                _upperAnimator.Change("upper_climb");
                _lowerAnimator.Change("lower_climb");
                _mover.Speed.X = 0;
                _mover.Speed.Y = MoveSpeed * inputY;
                Entity.Position = new Vector2(_ladderCollider.AbsolutePosition.X, Entity.Position.Y);

                if (inputY == 0)
                {
                    _upperAnimator.Pause();
                    _lowerAnimator.Pause();
                }
                else
                {
                    _upperAnimator.UnPause();
                    _lowerAnimator.UnPause();
                }
                if (_inputJump.IsPressed || !Hitbox.Overlaps(_ladderCollider))
                {
                    _state = States.Normal;
                    _mover.Speed.Y = 0;
                }
            }
            // HURT STATE
            else if (_state == States.Hurt)
            {
                _upperAnimator.Change("upper_hurt");
                _lowerAnimator.Change("lower_hurt");
                _weaponAnimator.Change("empty");
                _hurtTimer -= Time.DeltaTime;

                _mover.Speed.X = _hurtDir * KnockbackSpeed;
                _mover.Speed.Y = Mathf.Sin(2f * MathHelper.Pi * _hurtTimer / HurtTime) * KnockbackSpeed;

                if (_hurtTimer <= 0)
                    _state = States.Normal;
            }
            // DEAD STATE
            else if (_state == States.Dead)
            {
                _mover.Speed.X = 0;
                _upperAnimator.Change("upper_dead", SpriteAnimator.LoopMode.ClampForever);
                _lowerAnimator.Change("lower_dead", SpriteAnimator.LoopMode.ClampForever);
                _weaponAnimator.Change("empty");
                Hitbox.PhysicsLayer &= ~Mask.Player;
            }

            // gravity
            if (!_onGround && _state != States.Dodge && _state != States.Hurt && _state != States.Climb)
            {
                _mover.Speed.Y += Gravity * Time.DeltaTime;
            }

            // reset dodge
            if (_onGround)
            {
                _canDodge = true;
            }
            if (_dodgeTimeoutTimer > 0)
            {
                _dodgeTimeoutTimer -= Time.DeltaTime;
            }

            // jumping
            if (_jumpTimer > 0)
            {
                _mover.Speed.Y = -JumpSpeed;
                _jumpTimer -= Time.DeltaTime;
                if (!_inputJump.IsDown || _mover.OnGround(-1) || _jumpTimer <= 0 || _state == States.Dodge)
                {
                    _jumpTimer = 0;
                    _mover.Speed.Y = 0;
                }
            }

            // wall jump window timer
            if (!onWall || _onGround)
            {
                _wallJumpWindowTimer = WallJumpWindowDuration;
            }

            // wall jump timer
            if (_wallJumpTimer > 0)
            {
                _wallJumpTimer -= Time.DeltaTime;
            }

            // fire timer
            if (_fireTimer > 0)
            {
                _fireTimer -= Time.DeltaTime;
            }

            // invincible timer
            if (_state != States.Hurt && _invincibilityTimer > 0)
            {
                if (Timer.OnInterval(0.05f))
                {
                    _upperAnimator.Color = _upperAnimator.Color == Color.Transparent ? Color.White : Color.Transparent;
                    _lowerAnimator.Color = _lowerAnimator.Color == Color.Transparent ? Color.White : Color.Transparent;
                    _weaponAnimator.Color = _weaponAnimator.Color == Color.Transparent ? Color.White : Color.Transparent;
                }
                _invincibilityTimer -= Time.DeltaTime;
                if (_invincibilityTimer <= 0)
                {
                    _upperAnimator.Color = Color.White;
                    _lowerAnimator.Color = Color.White;
                    _weaponAnimator.Color = Color.White;
                }
            }

            // hurt check
            if (_invincibilityTimer <= 0 && Hitbox.CollidesWithAny(out var hurtHit) && _state != States.Dead)
            {
                Timer.PauseFor(0.1f);
                _upperAnimator.Change("upper_hurt", SpriteAnimator.LoopMode.ClampForever);
                _lowerAnimator.Change("lower_hurt", SpriteAnimator.LoopMode.ClampForever);
                _weaponAnimator.Change("empty");
                var health = _vars.Get<int>(Vars.PlayerHealth);
                var damage = hurtHit.Collider.GetComponent<Damage>();
                health -= damage?.Amount ?? 1;
                _vars.Set(Vars.PlayerHealth, health);

                if (_attackCollider != null)
                    Entity.RemoveComponent(_attackCollider);

                if (health <= 0)
                {
                    _state = States.Dead;
                    DeathSound.start();
                }
                else
                {
                    _state = States.Hurt;
                    HurtSound.start();
                    _hurtTimer = HurtTime;
                    _invincibilityTimer = InvincibleTime;
                    _hurtDir = Math.Sign(Hitbox.AbsolutePosition.X - hurtHit.Collider.AbsolutePosition.X);
                }
            }

            // interaction
            var hit = InteractionCast();
            var interactable = hit.Collider?.GetComponent<Interactable>();
            if ((hit.Collider?.PhysicsLayer & Mask.Interaction) > 0 && interactable?.Enabled == true)
            {
                _vars[Vars.HudPrompt] = $"[{(_usingGamePad ? "Y" : "E")}] {interactable.Prompt}";
                if (_inputInteract.IsPressed)
                {
                    _inputInteract.ConsumeBuffer();
                    if (interactable != null)
                        interactable.Interact(Entity);
                }
            }
            else
            {
                _vars[Vars.HudPrompt] = string.Empty;
            }

            _mover.Speed.Y = Mathf.Clamp(_mover.Speed.Y, -JumpSpeed, MaxFallSpeed);
        }

        public RaycastHit InteractionCast(int mask = Mask.Interaction | Mask.Terrain) => Physics.Linecast(
            Entity.Position,
            Entity.Position + new Vector2(CastDistance, 0) * _facing,
            mask);
    }
}
