using Godot;
using System;

public partial class Bullet : ShotObject
{
    new public float MoveSpeed = 150;

    enum State
    {
        Idle,
        Moving,
        Destroyed
    }

    public override void _Ready()
    {
        States = new IState[Enum.GetNames(typeof(State)).Length];
        _ = new StateIdle(this);
        _ = new StateMoving(this);
        _ = new StateDestroyed(this);

        AccessingResources();

        _DamageEmitter.AreaEntered += OnDamageEmitter_AreaEntered;
        _DamageEmitter.AttackSuccess += OnDamageEmitter_AttackSuccess;

    }

    private void OnDamageEmitter_AttackSuccess()
    {

    }

    private void OnDamageEmitter_AreaEntered(Area2D area)
    {
        if (area is DamageReceiver a)
        {
            if (AttackRange((a.Owner as Node2D).Position))
            {
                DamageReceiver.DamageReceivedEventArgs e;
                e = new(_DamageEmitter.GetNode<CollisionShape2D>("CollisionShape2D").GlobalPosition, Direction, Damage, 30);
                a.DamageReceived(_DamageEmitter, e);
                SwitchState((int)State.Destroyed);
            }
        }
    }


    public override void _PhysicsProcess(double delta)
    {
        Velocity = Direction * MoveSpeed;
        StateMachineUpdate(delta);
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
            character.Visible = true;
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
            character.Position += character.Velocity * (float)delta;
            return Exit();
        }
        public int Exit()
        {
            if (character.Direction == Vector2.Zero)
            {
                return (int)State.Idle;
            }
            if (character.OnScreen.IsOnScreen() == false)
            {
                return (int)State.Destroyed;
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
            character.QueueFree();
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
