using Godot;
using System;

public partial class Player : Character
{
    enum State
    {
        Idle,
        Walk,
        Attack,
        Jump,
        Fall,
        Skill,

    }

    public override void _Ready()
    {
        States = new IState[Enum.GetNames(typeof(State)).Length];

        AccessingResources();
        DamageEmitter.AreaEntered += OnDamageEmitter_AreaEntered;

        _ = new StateIdle(this);
        _ = new StateWalk(this);
        _ = new StateAttack(this);
        _ = new StateSkill(this);
        _ = new StateJump(this);
        _ = new StateFall(this);

    }

    private void OnDamageEmitter_AreaEntered(Area2D area)
    {
        Vector2 direction = Vector2.Right * DamageEmitter.Scale.X;
        (area as DamageReceiver)?.EmitSignal(DamageReceiver.SignalName.DamageReceived, Damage, direction);
    }


    public override void _PhysicsProcess(double delta)
    {

        Direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        StateMachineUpdate(delta);

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
                Character.DamageEmitter.Scale = new Vector2(-1, 1);
            }
            else if (Character.Direction.X > 0)
            {
                Character.CharacterSprite.FlipH = false;
                Character.DamageEmitter.Scale = new Vector2(1, 1);
            }

            Character.Velocity = Character.Direction * Character.Speed;
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
        public int GetId { get; } = (int)State.Attack;
        public StateAttack(Player character)
        {
            Character = character;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            Character.Direction = Vector2.Zero;
            Character.AnimationPlayer.Play("Punch");
            return true;
        }

        public int Update(double delta)
        {

            return Exit();
        }
        public int Exit()
        {
            if (Input.IsActionJustPressed("skill"))
            {
                return (int)State.Skill;
            }
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
    public partial class StateFall : Node, IState
    {
        Player Character;
        public int GetId { get; } = (int)State.Fall;
        public StateFall(Player character)
        {
            Character = character;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            Character.AnimationPlayer.Play("Fall");
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
