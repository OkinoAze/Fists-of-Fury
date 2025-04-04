using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Player : Character
{

    public string[] AttackAnimationGroup = ["Punch", "Punch2", "Kick", "Kick2"];
    public int[] CanNotInputStates = [(int)State.Hurt, (int)State.KnockDown, (int)State.KnockFly, (int)State.KnockFall];
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
        States = new IState[Enum.GetNames(typeof(State)).Length];
        var slots = GetNodeOrNull("EnemySlots")?.GetChildren();

        InvincibleStates = [(int)State.Hurt, (int)State.KnockDown, (int)State.KnockFly, (int)State.KnockFall, (int)State.CrouchDown];
        MaxHealth = 10;
        Health = 10;

        AvatarName = "Player";

        foreach (var item in slots)
        {
            EnemySlots.Add((EnemySlot)item);
        }

        AccessingResources();
        _DamageEmitter.AreaEntered += OnDamageEmitter_AreaEntered;
        _DamageReceiver.DamageReceived += OnDamageReceiver_DamageReceived;


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


    }

    public override void _PhysicsProcess(double delta)
    {
        if (!CanNotInputStates.Contains(StateID))
        {
            Direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

        }
        StateMachineUpdate(delta);
        var a = new List<int> { 1, 2, 3 }.Any(x => x == 1);
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
                    AttackID = 0;
                    e = new(direction, 2, 100, 100, DamageReceiver.HitType.knockDown);
                    a.DamageReceived(this, e);
                    return;
                }
                if (Weapon == null)
                {
                    AttackID++;
                    if (AttackID >= AttackAnimationGroup.Length)
                    {
                        AttackID = 0;
                    }
                }
                else
                {
                    AttackID = 0;
                    if (Weapon.Propertie == Prop.Properties.MeleeWeapon)
                    {
                        e = new(direction, 3);
                        a.DamageReceived(this, e);
                        return;
                    }
                    else if (Weapon.Propertie == Prop.Properties.RangedWeapon)
                    {
                        return;
                    }
                }
                e = new(direction);
                a.DamageReceived(this, e);
            }
        }

    }


    private void OnDamageReceiver_DamageReceived(object sender, DamageReceiver.DamageReceivedEventArgs e)
    {
        if (!InvincibleStates.Contains(StateID))
        {
            Direction = e.Direction;
            Repel = e.Repel;
            HeightSpeed = e.HeightSpeed;
            if (e.Type == DamageReceiver.HitType.Normal)
            {
                SwitchState((int)State.Hurt);
            }
            else if (e.Type == DamageReceiver.HitType.knockDown)
            {
                SwitchState((int)State.KnockFly);
            }
            Health = Mathf.Clamp(Health - e.Damage, 0, MaxHealth);
        }
    }

    new public void PickUpProp(Prop prop)
    {
        Health = Mathf.Clamp(Health + prop.RestoreHelath, 0, MaxHealth);
        if (prop.Propertie == Prop.Properties.MeleeWeapon || prop.Propertie == Prop.Properties.RangedWeapon)
        {
            Weapon = prop;
        }
    }

    public EnemySlot ReserveSlot(Enemy enemy)
    {
        var availableSlots = EnemySlots.Where(x => x.IsFree());
        if (availableSlots.Count() == 0)
        {
            return null;
        }
        var solt = availableSlots.OrderBy(x => x.GlobalPosition.DistanceTo(enemy.GlobalPosition)).First();
        solt.Occupy(enemy);
        return solt;
    }
    public void FreeSlot(Enemy enemy)
    {
        var targetSlots = EnemySlots.Where(x => x.Occupant == enemy);
        if (targetSlots.Count() == 1)
        {
            targetSlots.First().FreeUp();
        }
    }

    private partial class StateIdle : Node, IState
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
                else if (character.Weapon.Propertie == Prop.Properties.MeleeWeapon)
                {
                    return (int)State.MeleeWeaponAttack;
                }
                else if (character.Weapon.Propertie == Prop.Properties.RangedWeapon)
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
                if (character.Weapon == null || character.Weapon.Propertie == Prop.Properties.MeleeWeapon)
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
    private partial class StateWalk : Node, IState
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
            if (character.Direction.X < 0)
            {
                character.CharacterSprite.FlipH = true;
                character.WeaponSprite.FlipH = true;
                character._DamageEmitter.Scale = new Vector2(-1, 1);
            }
            else if (character.Direction.X > 0)
            {
                character.CharacterSprite.FlipH = false;
                character.WeaponSprite.FlipH = false;
                character._DamageEmitter.Scale = new Vector2(1, 1);
            }

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
                else if (character.Weapon.Propertie == Prop.Properties.MeleeWeapon)
                {
                    return (int)State.MeleeWeaponAttack;
                }
                else if (character.Weapon.Propertie == Prop.Properties.RangedWeapon)
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
                if (character.Weapon == null || character.Weapon.Propertie == Prop.Properties.MeleeWeapon)
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
    public partial class StateAttack : Node, IState
    {
        Player character;
        int id = 0;
        public int GetId { get; } = (int)State.Attack;
        public StateAttack(Player c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character.Direction = Vector2.Zero;

            if (true)
            {
                if (character.AttackID == id)
                {
                    id = 0;
                    character.AttackID = id;
                }
                else
                {
                    id = character.AttackID;
                }
            }
            character.AnimationPlayer.Play(character.AttackAnimationGroup[id]);
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
    private partial class StateSkill : Node, IState
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
            return true;
        }

        public int Update(double delta)
        {


            return Exit();
        }
        public int Exit()
        {
            if (true)
            {
                return (int)State.Idle;
            }
            return GetId;
        }
    }

    public partial class StateJump : Node, IState
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

    public partial class StateJumpKick : Node, IState
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
            return true;
        }

        public int Update(double delta)
        {
            character.Height += character.HeightSpeed * (float)delta;
            character.HeightSpeed -= Character.Gravity * (float)delta;
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
    private partial class StateHurt : Node, IState
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
            character.AttackID = 0;
            character.AnimationPlayer.Play("Hurt");
            character.PlayAudio("hit-1");
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
    private partial class StateKnockFly : Node, IState
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
            character.AnimationPlayer.Play("KnockFly");
            character.PlayAudio("hit-2");
            character.Velocity = character.Direction * character.Repel;
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
    private partial class StateKnockFall : Node, IState
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
    private partial class StateKnockDown : Node, IState
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
            if (Input.IsActionPressed("jump"))
            {
                //TODO 切换到蹲伏状态
            }
            return GetId;
        }
    }
    private partial class StateCrouchDown : Node, IState
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
            character.AnimationPlayer.Play("CrouchDown");
            character.Velocity = Vector2.Zero;
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
    private partial class StateMeleeWeaponAttack : Node, IState
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
            character.AnimationPlayer.Play("KinfeAttack");
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
    private partial class StateRangedWeaponAttack : Node, IState
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
            character.PlayAudio("gunshot");
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
