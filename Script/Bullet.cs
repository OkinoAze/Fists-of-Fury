using Godot;
using System;

public partial class Bullet : StaticBody2D
{
    public int Damage = 3;
    public float MoveSpeed = 100;
    public float JumpSpeed = 0;
    public float HeightSpeed = 0;
    public float Height = 0;
    public const float Gravity = 320;
    public const float _AttackRange = 5;
    public Vector2 Direction = Vector2.Zero;
    public Area2D _DamageEmitter;
    public IState[] States = new IState[Enum.GetNames(typeof(State)).Length];

    enum State
    {
        Idle,
        Moving,
        Destroyed
    }

    public override void _Ready()
    {
        _ = new StateIdle(this);
        _ = new StateMoving(this);
        _ = new StateDestroyed(this);
    }
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
    }
    public interface IState
    {
        public int GetId { get; }

        public bool Enter()
        {
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

    public partial class StateIdle : Node, IState
    {
        Bullet character;
        public int GetId { get; } = (int)State.Idle;
        public StateIdle(Bullet c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            return true;
        }

        public int Update(double delta)
        {
            return Exit();
        }
        public int Exit()
        {
            if (character.Direction != Vector2.Zero)
            {

                return (int)State.Moving;
            }
            return GetId;
        }
    }
    public partial class StateMoving : Node, IState
    {
        Bullet character;
        public int GetId { get; } = (int)State.Moving;
        public StateMoving(Bullet c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            return true;
        }

        public int Update(double delta)
        {
            return Exit();
        }
        public int Exit()
        {
            if (character.Direction == Vector2.Zero)
            {
                return (int)State.Idle;
            }
            return GetId;
        }
    }
    public partial class StateDestroyed : Node, IState
    {
        Bullet character;
        public int GetId { get; } = (int)State.Destroyed;
        public StateDestroyed(Bullet c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
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
