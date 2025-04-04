using Godot;
using System;

public partial class Bullet : StaticObject
{
    public int Damage = 3;

    public const float _AttackRange = 5;

    public Area2D _DamageEmitter;


    enum State
    {
        Idle,
        Moving,
        Destroyed
    }

    public override void _Ready()
    {
        States = new IState[Enum.GetNames(typeof(State)).Length];

        _DamageEmitter = GetNode<Area2D>("DamageEmitter");
        _DamageEmitter.AreaEntered += OnDamageEmitter_AreaEntered;

        _ = new StateIdle(this);
        _ = new StateMoving(this);
        _ = new StateDestroyed(this);
    }

    private void OnDamageEmitter_AreaEntered(Area2D area)
    {
        if (area is DamageReceiver a)
        {
            if (AttackRange((a.Owner as Node2D).Position))
            {

                Vector2 direction = (Vector2.Right * _DamageEmitter.Scale.X).Normalized();
                DamageReceiver.DamageReceivedEventArgs e;
                e = new(direction);
                a.DamageReceived(this, e);

            }
        }
    }


    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
    }
    public bool AttackRange(Vector2 position)
    {
        if (position.Y > Position.Y - _AttackRange && position.Y < Position.Y + _AttackRange)
        {
            return true;
        }
        return false;
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
            QueueFree();
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
