using Godot;
using System;

public partial class Bullet : StaticObject
{
    public int Damage = 1;
    public const float _AttackRange = 5;
    new public float MoveSpeed = 100;
    public Area2D _DamageEmitter;
    public Sprite2D Sprite;
    public VisibleOnScreenNotifier2D OnScreen;

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

        OnScreen = GetNode<VisibleOnScreenNotifier2D>("VisibleOnScreenNotifier2D");
        Sprite = GetNode<Sprite2D>("Sprite2D");
        _DamageEmitter = Sprite.GetNode<Area2D>("DamageEmitter");


        _DamageEmitter.AreaEntered += OnDamageEmitter_AreaEntered;


    }

    private void OnDamageEmitter_AreaEntered(Area2D area)
    {
        if (area is DamageReceiver a)
        {
            if (AttackRange((a.Owner as Node2D).Position))
            {

                Vector2 direction = (Vector2.Right * _DamageEmitter.Scale.X).Normalized();
                DamageReceiver.DamageReceivedEventArgs e;
                e = new(direction, Damage, 30);
                a.DamageReceived(this, e);

                SwitchState((int)State.Destroyed);
            }
        }
    }


    public override void _PhysicsProcess(double delta)
    {
        Velocity = Direction * MoveSpeed;
        StateMachineUpdate(delta);
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
