using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Player : Character
{
    string[] AttackAnimationGroup = ["Punch", "Punch2", "Kick", "Kick2"];
    int[] CanNotInputStates = [(int)State.Hurt, (int)State.KnockDown, (int)State.KnockFly, (int)State.KnockFall, (int)State.CrouchDown];
    List<EnemySlot> EnemySlots = [];
    enum State
    {
        Idle,
        Walk,
        Attack,
        Jump,
        JumpKick,
        Skill,
        Hurt,
        CrouchDown,
        KnockFly,
        KnockFall,
        KnockDown,
        MeleeWeaponAttack,
        RangedWeaponAttack,

    }

    public override void _Ready()
    {
        InvincibleStates = [(int)State.Hurt, (int)State.KnockDown, (int)State.KnockFly, (int)State.KnockFall, (int)State.CrouchDown];

        States = new IState[Enum.GetNames(typeof(State)).Length];

        _ = new StateIdle(this);
        _ = new StateWalk(this);
        _ = new StateAttack(this);
        _ = new StateSkill(this);
        _ = new StateJump(this);
        _ = new StateJumpKick(this);
        _ = new StateHurt(this);
        _ = new StateKnockFly(this);
        _ = new StateKnockFall(this);
        _ = new StateKnockDown(this);
        _ = new StateCrouchDown(this);
        _ = new StateMeleeWeaponAttack(this);
        _ = new StateRangedWeaponAttack(this);

        AvatarName = "Player";

        AccessingResources();
        _DamageEmitter.AreaEntered += OnDamageEmitter_AreaEntered;
        _DamageEmitter.AttackSuccess += OnDamageEmitter_AttackSuccess;
        _DamageReceiver.DamageReceived += OnDamageReceiver_DamageReceived;




        var slots = GetNodeOrNull("EnemySlots")?.GetChildren();
        foreach (var item in slots)
        {
            EnemySlots.Add((EnemySlot)item);
        }

    }


    public override void _PhysicsProcess(double delta)
    {
        if (!CanNotInputStates.Contains(StateID))
        {
            Direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        }
        if (InvincibleStates.Contains(StateID))
        {
            _DamageReceiver.Monitorable = false;
        }
        else
        {
            _DamageReceiver.Monitorable = true;
        }
        StateMachineUpdate(delta);

    }


    private void OnDamageEmitter_AttackSuccess()
    {
        if (Weapon?.Property == Prop.Properties.MeleeWeapon && StateID == (int)State.MeleeWeaponAttack)
        {
            Weapon.Durability--;
        }

    }

    private void OnDamageEmitter_AreaEntered(Area2D area)
    {
        if (area is DamageReceiver a)
        {
            if (AttackRange((a.Owner as Node2D).Position))
            {

                Vector2 direction = (Vector2.Right * _DamageEmitter.Scale.X).Normalized();
                DamageReceiver.DamageReceivedEventArgs e;
                if (StateID == (int)State.JumpKick || AttackID == AttackAnimationGroup.Length - 1)
                {
                    e = new(direction, 2, 100, 100, DamageReceiver.HitType.knockDown);
                    a.DamageReceived(_DamageEmitter, e);
                    return;
                }

                if (StateID == (int)State.MeleeWeaponAttack)
                {
                    e = new(direction, Weapon.Damage);
                }
                else
                {
                    e = new(direction);
                }
                a.DamageReceived(_DamageEmitter, e);
            }
        }

    }


    private void OnDamageReceiver_DamageReceived(DamageEmitter emitter, DamageReceiver.DamageReceivedEventArgs e)
    {

        Direction = e.Direction;
        Repel = e.Repel;
        HeightSpeed = e.HeightSpeed;
        if (e.Type == DamageReceiver.HitType.Normal)
        {
            AnimationPlayer.Stop();
            SwitchState((int)State.Hurt);
        }
        else if (e.Type == DamageReceiver.HitType.knockDown)
        {
            SwitchState((int)State.KnockFly);
        }
        Health = Mathf.Clamp(Health - e.Damage, 0, MaxHealth);
        emitter?.AttackSuccess();
    }



    public EnemySlot ReserveSlot(Enemy enemy)
    {
        var availableSlots = EnemySlots.Where(x => x.IsFree());
        if (availableSlots.Count() == 0)
        {
            return null;
        }
        var slot = availableSlots.OrderBy(x => x.GlobalPosition.DistanceTo(enemy.GlobalPosition)).First();
        slot.Occupy(enemy);
        return slot;
    }
    public void FreeSlot(Enemy enemy)
    {
        var targetSlots = EnemySlots.Where(x => x.Occupant == enemy);
        if (targetSlots.Count() == 1)
        {
            targetSlots.First().FreeUp();
        }
    }

    partial class StateIdle : Node, IState
    {
        Player character;
        public int GetId { get; } = (int)State.Idle;
        public StateIdle(Player c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.Direction = Vector2.Zero;
            character.AnimationPlayer.Play("Idle");
            return true;
        }

        public int Update(double delta)
        {
            return Exit();
        }
        public int Exit()
        {
            if (Input.IsActionJustPressed("attack"))
            {
                if (character.Weapon == null)
                {
                    return (int)State.Attack;
                }
                else if (character.Weapon.Property == Prop.Properties.MeleeWeapon)
                {
                    return (int)State.MeleeWeaponAttack;
                }
                else if (character.Weapon.Property == Prop.Properties.RangedWeapon)
                {
                    return (int)State.RangedWeaponAttack;
                }
            }
            if (Input.IsActionJustPressed("skill"))
            {
                return (int)State.Skill;
            }
            if (Input.IsActionJustPressed("jump") || character.Height > 0)
            {
                if (character.Weapon == null || character.Weapon.Property == Prop.Properties.MeleeWeapon)
                {
                    return (int)State.Jump;
                }
                else
                {
                    character.Height = 0;
                    character.CharacterSprite.Position = Vector2.Up * character.Height;
                }
            }
            if (character.Direction != Vector2.Zero)
            {
                return (int)State.Walk;
            }
            return GetId;
        }
    }
    partial class StateWalk : Node, IState
    {
        Player character;
        public int GetId { get; } = (int)State.Walk;
        public StateWalk(Player c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.AnimationPlayer.Play("Walk");
            return true;
        }

        public int Update(double delta)
        {
            character.FaceToDirection();

            character.Velocity = character.Direction * character.MoveSpeed;
            character.MoveAndSlide();

            return Exit();
        }
        public int Exit()
        {
            if (Input.IsActionJustPressed("attack"))
            {
                if (character.Weapon == null)
                {
                    return (int)State.Attack;
                }
                else if (character.Weapon.Property == Prop.Properties.MeleeWeapon)
                {
                    return (int)State.MeleeWeaponAttack;
                }
                else if (character.Weapon.Property == Prop.Properties.RangedWeapon)
                {
                    return (int)State.RangedWeaponAttack;
                }
            }
            if (Input.IsActionJustPressed("skill"))
            {
                return (int)State.Skill;
            }
            if (Input.IsActionJustPressed("jump"))
            {
                if (character.Weapon == null || character.Weapon.Property == Prop.Properties.MeleeWeapon)
                {
                    return (int)State.Jump;
                }
            }
            if (character.Direction == Vector2.Zero)
            {
                return (int)State.Idle;
            }
            return GetId;
        }
    }
    public void AttackEnd()
    {
        if (AttackID == 1 && CanPickUpProp != null)
        {
            AttackID = 0;
            SwitchState((int)State.CrouchDown);
            PickUpProp(CanPickUpProp);
        }
        else
        {
            SwitchState((int)State.Idle);
        }
    }
    partial class StateAttack : Node, IState
    {
        Player character;
        public int GetId { get; } = (int)State.Attack;
        public StateAttack(Player c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.Direction = Vector2.Zero;
            if (character.AttackBufferTimer.TimeLeft > 0)
            {
                character.AttackID++;
                if (character.AttackID >= character.AttackAnimationGroup.Length)
                {
                    character.AttackID = 0;
                }
            }
            else
            {
                character.AttackID = 0;
            }
            character.AnimationPlayer.Play(character.AttackAnimationGroup[character.AttackID]);
            character.PlayAudio("miss");
            character.AttackBufferTimer.Start();

            return true;
        }

        public int Update(double delta)
        {

            return Exit();
        }
        public int Exit()
        {
            return GetId;
        }
    }
    partial class StateSkill : Node, IState
    {
        Player character;
        public int GetId { get; } = (int)State.Skill;
        public StateSkill(Player c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.Direction = Vector2.Zero;
            character.AnimationPlayer.Play("Idle");
            character.DropWeapon();
            return true;
        }

        public int Update(double delta)
        {


            return Exit();
        }
        public int Exit()
        {
            return (int)State.Idle;
            return GetId;
        }
    }

    partial class StateJump : Node, IState
    {
        Player character;
        public int GetId { get; } = (int)State.Jump;
        public StateJump(Player c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.AnimationPlayer.Play("Jump");
            character.PlayAudio("miss");
            character.HeightSpeed = character.JumpSpeed;
            return true;
        }

        public int Update(double delta)
        {
            character.Height += character.HeightSpeed * (float)delta;
            character.HeightSpeed -= Gravity * (float)delta;
            character.CharacterSprite.Position = Vector2.Up * character.Height;

            character.MoveAndSlide();
            return Exit();
        }
        public int Exit()
        {
            if (character.Height <= 0)
            {
                character.CharacterSprite.Position = Vector2.Zero;
                character.Height = 0;
                return (int)State.Idle;
            }
            if (Input.IsActionJustPressed("attack"))
            {
                return (int)State.JumpKick;
            }
            return GetId;
        }
    }

    partial class StateJumpKick : Node, IState
    {
        Player character;
        public int GetId { get; } = (int)State.JumpKick;
        public StateJumpKick(Player c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.AnimationPlayer.Play("JumpKick");
            character.PlayAudio("miss");
            return true;
        }

        public int Update(double delta)
        {
            character.Height += character.HeightSpeed * (float)delta;
            character.HeightSpeed -= Gravity * (float)delta;
            character.CharacterSprite.Position = Vector2.Up * character.Height;

            character.MoveAndSlide();
            return Exit();
        }
        public int Exit()
        {
            if (character.Height <= 0)
            {
                character._DamageEmitter.Monitoring = false;
                character.Height = 0;
                character.CharacterSprite.Position = Vector2.Zero;
                return (int)State.Idle;
            }
            return GetId;
        }
    }
    public void HurtEnd()
    {
        if (Health <= 0 || Height > 0)
        {
            SwitchState((int)State.KnockFall);
        }
        else
        {
            SwitchState((int)State.Idle);
        }
    }
    partial class StateHurt : Node, IState
    {
        Player character;
        public int GetId { get; } = (int)State.Hurt;
        public StateHurt(Player c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character._DamageEmitter.Monitoring = false;
            character.DropWeapon();
            character.AnimationPlayer.Play("Hurt");
            if (character.Health <= 0)
            {
                character.PlayAudio("hit-2");
                //TODO 摇晃动画,粒子特效
            }
            else
            {
                character.PlayAudio("hit-1");
            }
            character.Velocity = character.Direction * character.Repel;
            return true;
        }

        public int Update(double delta)
        {


            character.MoveAndSlide();

            return Exit();
        }
        public int Exit()
        {
            return GetId;
        }
    }
    partial class StateKnockFly : Node, IState
    {
        Player character;
        public int GetId { get; } = (int)State.KnockFly;
        public StateKnockFly(Player c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character._DamageEmitter.Monitoring = false;
            character.DropWeapon();
            character.AnimationPlayer.Play("KnockFly");
            character.PlayAudio("hit-2");
            //TODO 摇晃动画,粒子特效

            character.Velocity = character.Direction * character.Repel;
            character.DropWeapon();
            return true;
        }

        public int Update(double delta)
        {
            character.Velocity += character.Direction * character.Repel * (float)delta;
            character.MoveAndSlide();
            return Exit();
        }
        public int Exit()
        {
            if (character.IsOnWall())
            {
                character.Direction *= -1;
                return (int)State.KnockFall;
            }
            return GetId;
        }
    }
    partial class StateKnockFall : Node, IState
    {
        Player character;
        public int GetId { get; } = (int)State.KnockFall;
        public StateKnockFall(Player c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.Velocity = character.Direction * character.Repel / 10;
            character.AnimationPlayer.Play("KnockFall");
            return true;
        }

        public int Update(double delta)
        {
            character.Height += character.HeightSpeed * (float)delta;
            character.HeightSpeed -= Gravity * (float)delta;
            character.CharacterSprite.Position = Vector2.Up * character.Height;


            character.MoveAndSlide();
            return Exit();
        }
        public int Exit()
        {
            if (character.Height <= 0)
            {
                character.CharacterSprite.Position = Vector2.Zero;
                character.Height = 0;
                return (int)State.KnockDown;
            }
            return GetId;
        }
    }
    public void KnockDownEnd()
    {
        if (Health <= 0)
        {
            //TODO 角色死亡
        }
        else
        {
            SwitchState((int)State.CrouchDown);
        }
    }
    partial class StateKnockDown : Node, IState
    {
        Player character;
        public int GetId { get; } = (int)State.KnockDown;
        public StateKnockDown(Player c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.Direction = Vector2.Zero;
            character.AnimationPlayer.Play("KnockDown");
            return true;
        }

        public int Update(double delta)
        {
            return Exit();
        }
        public int Exit()
        {
            return GetId;
        }
    }
    partial class StateCrouchDown : Node, IState
    {
        Player character;
        public int GetId { get; } = (int)State.CrouchDown;
        public StateCrouchDown(Player c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.Velocity = Vector2.Zero;
            character.AnimationPlayer.Play("CrouchDown");
            return true;
        }

        public int Update(double delta)
        {

            return Exit();
        }
        public int Exit()
        {
            return GetId;
        }
    }
    partial class StateMeleeWeaponAttack : Node, IState
    {
        Player character;
        public int GetId { get; } = (int)State.MeleeWeaponAttack;
        public StateMeleeWeaponAttack(Player c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.Direction = Vector2.Zero;
            if (character.Weapon.Durability > 0)
            {
                character.AnimationPlayer.Play("KnifeAttack");
                character.PlayAudio("miss");
            }
            else
            {
                character.DropWeapon();
                character.SwitchState((int)State.Idle);
            }
            return true;
        }

        public int Update(double delta)
        {

            return Exit();
        }
        public int Exit()
        {

            return GetId;
        }
    }
    partial class StateRangedWeaponAttack : Node, IState
    {
        Player character;
        public int GetId { get; } = (int)State.RangedWeaponAttack;
        public StateRangedWeaponAttack(Player c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.Direction = Vector2.Zero;
            character.AnimationPlayer.Play("GunShot");
            if (character.Weapon.Durability > 0)
            {
                character.Weapon.Durability--;
                character.PlayAudio("click");
                var d = (Vector2.Right * character._DamageEmitter.Scale.X).Normalized();
                var p = new Vector2(character.Weapon.ShotPosition.X * d.X, character.Weapon.ShotPosition.Y);
                EntityManager.Instance.GenerateBullet(character.Weapon.Damage, d, new Vector2(character.Position.X + p.X, character.Position.Y), new Vector2(0, p.Y));
            }
            else
            {
                character.DropWeapon();
                character.SwitchState((int)State.Idle);
            }
            return true;
        }

        public int Update(double delta)
        {
            return Exit();
        }
        public int Exit()
        {
            return GetId;
        }
    }
}
