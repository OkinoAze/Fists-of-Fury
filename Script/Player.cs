using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Player : Character
{
    public int AttackID = 0;

    public string[] AttackAnimationGroup = ["Punch", "Punch2", "Kick", "Kick2"];

    List<EnemySolt> EnemySolts = [];
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

    }

    public override void _Ready()
    {
        States = new IState[Enum.GetNames(typeof(State)).Length];
        var solts = GetNodeOrNull("EnemySlots")?.GetChildren();
        foreach (var item in solts)
        {
            EnemySolts.Add((EnemySolt)item);
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

        _ = new StateKnockDown(this);

    }

    public override void _PhysicsProcess(double delta)
    {

        Direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        StateMachineUpdate(delta);
        var a = new List<int> { 1, 2, 3 }.Any(x => x == 1);
    }

    private void OnDamageEmitter_AreaEntered(Area2D area)
    {
        //TODO 攻击切换动画
        if (area is DamageReceiver a)
        {
            if (AttackRange((a.Owner as Node2D).Position))
            {
                AttackID++;
                if (AttackID >= AttackAnimationGroup.Length)
                {
                    AttackID = 0;
                }
                Vector2 direction = (Vector2.Right * _DamageEmitter.Scale.X).Normalized();
                DamageReceiver.DamageReceivedEventArgs e;
                if (StateID == (int)State.JumpKick)
                {
                    e = new(direction, 1, 100, 100, DamageReceiver.HitType.knockDown);
                    a.DamageReceived(this, e);
                    return;
                }
                e = new(direction);
                a.DamageReceived(this, e);
            }
        }

    }


    private void OnDamageReceiver_DamageReceived(object sender, DamageReceiver.DamageReceivedEventArgs e)
    {
        Health = Mathf.Clamp(Health - e.Damage, 0, MaxHealth);
        if (e.Type == DamageReceiver.HitType.knockDown || Health <= 0)
        {
            SwitchState((int)State.KnockDown);
        }
    }

    public EnemySolt ReserveSlot(Enemy enemy)
    {
        var availableSlots = EnemySolts.Where(x => x.IsFree());
        if (availableSlots.Count() == 0)
        {
            return null;
        }
        var solt = availableSlots.OrderBy(x => x.GlobalPosition.DistanceTo(enemy.GlobalPosition)).First();
        solt.Occupy(enemy);
        return solt;
    }
    public void FreeSolt(Enemy enemy)
    {
        var targetSlots = EnemySolts.Where(x => x.Occupant == enemy);
        if (targetSlots.Count() == 1)
        {
            targetSlots.First().FreeUp();
        }
    }

    private partial class StateIdle : Node, IState
    {
        Player Character;
        public int GetId { get; } = (int)State.Idle;
        public StateIdle(Player character)
        {
            Character = character;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            Character.Direction = Vector2.Zero;
            Character.AnimationPlayer.Play("Idle");
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
                return (int)State.Attack;
            }
            if (Input.IsActionJustPressed("skill"))
            {
                return (int)State.Skill;
            }
            if (Input.IsActionJustPressed("jump"))
            {
                return (int)State.Jump;
            }
            if (Character.Direction != Vector2.Zero)
            {
                return (int)State.Walk;
            }
            return GetId;
        }
    }
    private partial class StateWalk : Node, IState
    {
        Player Character;
        public int GetId { get; } = (int)State.Walk;
        public StateWalk(Player character)
        {
            Character = character;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            Character.AnimationPlayer.Play("Walk");
            return true;
        }

        public int Update(double delta)
        {
            if (Character.Direction.X < 0)
            {
                Character.CharacterSprite.FlipH = true;
                Character._DamageEmitter.Scale = new Vector2(-1, 1);
            }
            else if (Character.Direction.X > 0)
            {
                Character.CharacterSprite.FlipH = false;
                Character._DamageEmitter.Scale = new Vector2(1, 1);
            }

            Character.Velocity = Character.Direction * Character.MoveSpeed;
            Character.MoveAndSlide();

            return Exit();
        }
        public int Exit()
        {
            if (Input.IsActionJustPressed("attack"))
            {
                return (int)State.Attack;
            }
            if (Input.IsActionJustPressed("skill"))
            {
                return (int)State.Skill;
            }
            if (Input.IsActionJustPressed("jump"))
            {
                return (int)State.Jump;
            }
            if (Character.Direction == Vector2.Zero)
            {
                return (int)State.Idle;
            }
            return GetId;
        }
    }
    public partial class StateAttack : Node, IState
    {
        Player Character;
        int id = 0;
        public int GetId { get; } = (int)State.Attack;
        public StateAttack(Player character)
        {
            Character = character;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            Character.Direction = Vector2.Zero;
            if (Character.AttackID == id)
            {
                id = 0;
                Character.AttackID = id;
            }
            else
            {
                id = Character.AttackID;
            }
            Character.AnimationPlayer.Play(Character.AttackAnimationGroup[id]);
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
        Player Character;
        public int GetId { get; } = (int)State.Skill;
        public StateSkill(Player character)
        {
            Character = character;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            Character.Direction = Vector2.Zero;
            Character.AnimationPlayer.Play("Idle");
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
        Player Character;
        public int GetId { get; } = (int)State.Jump;
        public StateJump(Player character)
        {
            Character = character;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            Character.AnimationPlayer.Play("Jump");
            Character.HeightSpeed = Character.JumpSpeed;
            return true;
        }

        public int Update(double delta)
        {
            Character.Height += Character.HeightSpeed * (float)delta;
            Character.HeightSpeed -= Character.Gravity * (float)delta;
            Character.CharacterSprite.Position = Vector2.Up * Character.Height;

            Character.MoveAndSlide();
            return Exit();
        }
        public int Exit()
        {
            if (Character.Height <= 0)
            {
                Character.CharacterSprite.Position = Vector2.Zero;
                Character.Height = 0;
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
        Player Character;
        public int GetId { get; } = (int)State.JumpKick;
        public StateJumpKick(Player character)
        {
            Character = character;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            Character.AnimationPlayer.Play("JumpKick");
            return true;
        }

        public int Update(double delta)
        {
            Character.Height += Character.HeightSpeed * (float)delta;
            Character.HeightSpeed -= Character.Gravity * (float)delta;
            Character.CharacterSprite.Position = Vector2.Up * Character.Height;

            Character.MoveAndSlide();
            return Exit();
        }
        public int Exit()
        {
            if (Character.Height <= 0)
            {
                Character._DamageEmitter.Monitoring = false;
                Character.Height = 0;
                Character.CharacterSprite.Position = Vector2.Zero;
                return (int)State.Idle;
            }
            return GetId;
        }
    }
    private partial class StateHurt : Node, IState
    {
        Player Character;
        public int GetId { get; } = (int)State.Hurt;
        public StateHurt(Player character)
        {
            Character = character;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            Character.Direction = Vector2.Zero;
            Character.AnimationPlayer.Play("Hurt");
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
    public void knockDownEnd()
    {
        if (Health <= 0)
        {
            //TODO 角色死亡后续
        }
        else
        {
            SwitchState((int)State.Idle);
        }
    }
    private partial class StateKnockDown : Node, IState
    {
        Player Character;
        public int GetId { get; } = (int)State.KnockDown;
        public StateKnockDown(Player character)
        {
            Character = character;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            Character.Direction = Vector2.Zero;
            Character.AnimationPlayer.Play("KnockDown");
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
}
