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
        _ = new StateIdle(this);
        _ = new StateWalk(this);
        _ = new StateAttack(this);
        _ = new StateSkill(this);
        _ = new StateJump(this);
        _ = new StateFall(this);

    }
    public override void _PhysicsProcess(double delta)
    {
        Direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        StateMachineUpdate(delta);

    }
    private partial class StateIdle : Node, IState
    {
        Character Character;
        public int GetId { get; } = (int)State.Idle;
        public StateIdle(Character character)
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
        Character Character;
        public int GetId { get; } = (int)State.Walk;
        public StateWalk(Character character)
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
            }
            else if (Character.Direction.X > 0)
            {
                Character.CharacterSprite.FlipH = false;
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
        protected Character Character;
        public int GetId { get; } = (int)State.Attack;
        public StateAttack(Character character)
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
        Character Character;
        public int GetId { get; } = (int)State.Skill;
        public StateSkill(Character character)
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
        protected Character Character;
        public int GetId { get; } = (int)State.Jump;
        public StateJump(Character character)
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
        protected Character Character;
        public int GetId { get; } = (int)State.Fall;
        public StateFall(Character character)
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
