﻿using Microsoft.Xna.Framework;
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
        }

        public Collider Hitbox;

        public float MoveSpeed = 150f;
        public float Gravity = 600f;
        public float JumpTime = .4f;
        public float JumpSpeed = 200f;
        public float MaxFallSpeed = 200f;
        public float CastDistance = 32;
        public float DodgeTime = .2f;
        public float DodgeSpeed = 250f;
        public float HurtTime = 0.2f;
        public float InvincibleTime = 1.5f;
        public float KnockbackSpeed = 150f;

        States _state = States.Normal;
        int _facing = 1;
        bool _onGround = false;
        float _jumpTimer = 0;
        float _attackTimer = 0;
        float _dodgeTimer = 0;
        float _hurtTimer = 0;
        int _hurtDir = 0;
        float _invincibilityTimer = 0;
        AttackTypes _attackType = AttackTypes.Light;
        BoxCollider _attackCollider;

        List<IInteractable> _tempInteractableList = new List<IInteractable>();
        bool _usingGamePad = false;

        VirtualIntegerAxis _inputX;
        VirtualButton _inputJump;
        VirtualButton _inputAttack;
        VirtualButton _inputDodge;
        VirtualButton _inputInteract;

        PlatformerMover _mover;
        SpriteAnimator _animator;
        SpriteAnimator _weaponAnimator;
        ScriptVars _vars;

        Inventory PlayerInventory => _vars.Get<Inventory>(Vars.PlayerInventory);

        public override void OnAddedToEntity()
        {
            _inputX = new VirtualIntegerAxis();
            _inputX.Nodes.Add(new VirtualAxis.GamePadLeftStickX());
            _inputX.Nodes.Add(new VirtualAxis.KeyboardKeys(
                VirtualInput.OverlapBehavior.CancelOut, Keys.A, Keys.D));
            _inputX.Nodes.Add(new VirtualAxis.KeyboardKeys(
                VirtualInput.OverlapBehavior.CancelOut, Keys.Left, Keys.Right));

            _inputJump = new VirtualButton();
            _inputJump.Nodes.Add(new VirtualButton.GamePadButton(0, Buttons.A));
            _inputJump.Nodes.Add(new VirtualButton.KeyboardKey(Keys.Space));

            _inputAttack = new VirtualButton();
            _inputAttack.Nodes.Add(new VirtualButton.GamePadButton(0, Buttons.X));
            _inputAttack.Nodes.Add(new VirtualButton.MouseLeftButton());

            _inputInteract = new VirtualButton();
            _inputInteract.Nodes.Add(new VirtualButton.GamePadButton(0, Buttons.Y));
            _inputInteract.Nodes.Add(new VirtualButton.KeyboardKey(Keys.E));

            _inputDodge = new VirtualButton();
            _inputDodge.AddGamePadButton(0, Buttons.B);
            _inputDodge.AddKeyboardKey(Keys.LeftShift);

            _mover = Entity.GetComponent<PlatformerMover>();
            _animator = Entity.GetComponent<SpriteAnimator>();

            _weaponAnimator = Entity.AddComponent<SpriteAnimator>();
            _weaponAnimator.RenderLayer = _animator.RenderLayer - 1;
            _weaponAnimator.AddAnimation("empty", GameContent.LoadAnimation("doodads", "empty", Core.Content));
            _weaponAnimator.Play("empty");

            _vars = Entity.Scene.GetScriptVars();
        }

        public void Update()
        {
            var flip = _facing == -1;
            _animator.FlipX = flip;
            _weaponAnimator.FlipX = flip;
            _onGround = _mover.OnGround();
            var inputX = _inputX.Value;

            if (_inputX.Nodes[0].Value != 0) _usingGamePad = true;
            if (_inputX.Nodes[1].Value != 0) _usingGamePad = false;

            if (Time.TimeScale == 0) return;

            // NORMAL STATE
            if (_state == States.Normal)
            {
                // animation
                if (_onGround)
                {
                    if (inputX == 0)
                        _animator.Change("idle");
                    else
                        _animator.Change("walk");
                }
                else
                {
                    _animator.Change("jump");
                }

                // horizontal movement
                {
                    _mover.Speed.X = inputX * MoveSpeed;

                    if (inputX != 0 && _onGround)
                        _facing = inputX;
                }

                // invoke jumping
                {
                    if (_inputJump.IsPressed && _onGround)
                    {
                        _inputJump.ConsumeBuffer();
                        _jumpTimer = JumpTime;
                    }
                }

                // invoke attacking
                {
                    var equippedWeapon = PlayerInventory.EquippedWeapon;
                    if (_inputAttack.IsPressed && equippedWeapon != null)
                    {
                        _inputAttack.ConsumeBuffer();
                        _state = States.Attack;
                        _attackTimer = 0;
                        _attackType = equippedWeapon.AttackType;
                        _weaponAnimator.AddAnimation("attack", equippedWeapon.Animation);

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

                // invoke dodging
                {
                    if (_inputDodge.IsPressed)
                    {
                        if (inputX != 0)
                            _facing = inputX;
                        _inputDodge.ConsumeBuffer();
                        _state = States.Dodge;
                        _dodgeTimer = 0;
                    }
                }
            }
            // ATTACK STATE
            else if (_state == States.Attack)
            {
                if (_attackType == AttackTypes.Light)
                {
                    _animator.Change("attack", SpriteAnimator.LoopMode.ClampForever);
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

                    if (!_animator.IsRunning)
                    {
                        _animator.Change("idle");
                        _weaponAnimator.Play("empty");
                        _state = States.Normal;
                    }
                }
                // handle other weapon attack types
                else
                {
                    throw new System.NotImplementedException($"Attack type {System.Enum.GetName(typeof(AttackTypes), _attackType)} not implemented.");
                }
            }
            // DODGE STATE
            else if (_state == States.Dodge)
            {
                _animator.Change("dodge", SpriteAnimator.LoopMode.ClampForever);
                _dodgeTimer += Time.DeltaTime;
                _mover.Speed.X = _facing * DodgeSpeed;

                if (_dodgeTimer >= DodgeTime)
                {
                    _animator.Change("idle");
                    _state = States.Normal;
                }
            }
            // HURT STATE
            else if (_state == States.Hurt)
            {
                _animator.Change("hurt");
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
                _animator.Change("dead", SpriteAnimator.LoopMode.ClampForever);
                Hitbox.PhysicsLayer &= ~Mask.Player;
            }

            // gravity
            if (!_onGround && _state != States.Dodge && _state != States.Hurt)
            {
                _mover.Speed.Y += Gravity * Time.DeltaTime;
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

            // invincible timer
            if (_state != States.Hurt && _invincibilityTimer > 0)
            {
                if (Timer.OnInterval(0.05f))
                {
                    _animator.Color = _animator.Color == Color.Transparent ? Color.White : Color.Transparent;
                    _weaponAnimator.Color = _weaponAnimator.Color == Color.Transparent ? Color.White : Color.Transparent;
                }
                _invincibilityTimer -= Time.DeltaTime;
                if (_invincibilityTimer <= 0)
                {
                    _animator.Color = Color.White;
                    _weaponAnimator.Color = Color.White;
                }
            }

            // hurt check
            if (_invincibilityTimer <= 0 && Hitbox.CollidesWithAny(out var hurtHit) && _state != States.Dead)
            {
                Timer.PauseFor(0.1f);
                _animator.Change("hurt", SpriteAnimator.LoopMode.ClampForever);
                var health = _vars.Get<int>(Vars.PlayerHealth);
                var damage = hurtHit.Collider.GetComponent<Damage>();
                health -= damage?.Amount ?? 1;
                _vars.Set(Vars.PlayerHealth, health);
                if (health <= 0)
                {
                    _state = States.Dead;
                }
                else
                {
                    _state = States.Hurt;
                    _hurtTimer = HurtTime;
                    _invincibilityTimer = InvincibleTime;
                    _hurtDir = Math.Sign(Hitbox.AbsolutePosition.X - hurtHit.Collider.AbsolutePosition.X);
                }
            }

            // interaction
            var hit = Physics.Linecast(
                Entity.Position, Entity.Position + new Vector2(CastDistance, 0) * _facing, Mask.Interaction);
            if (hit.Collider != null)
            {
                _vars[Vars.HudPrompt] = $"[{(_usingGamePad ? "Y" : "E")}] Interact";
                if (_inputInteract.IsPressed)
                {
                    _inputInteract.ConsumeBuffer();
                    hit.Collider.GetComponents(_tempInteractableList);
                    foreach (var interactable in _tempInteractableList)
                        interactable.Interact(Entity);
                    _tempInteractableList.Clear();
                }
            }
            else
            {
                _vars[Vars.HudPrompt] = string.Empty;
            }

            _mover.Speed.Y = Mathf.Clamp(_mover.Speed.Y, -JumpSpeed, MaxFallSpeed);
        }
    }
}
