using Godot;
using System;

public partial class Enemy : Character
{
    Player _Player;
    EnemySolt Solt = null;
    enum State
    {
        Idle,
        Walk,
        Attack,
        Skill,
        Hurt,
    }
    public override void _Ready()
    {
        _Player = GetTree().GetNodesInGroup("Player")[0] as Player;
        States = new IState[Enum.GetNames(typeof(State)).Length];
        MaxHealth = 10;
        Health = 5;
        AccessingResources();


        _DamageEmitter.AreaEntered += OnDamageEmitter_AreaEntered;
        _DamageReceiver.DamageReceived += OnDamageReceiver_DamageReceived;

        _ = new StateIdle(this);
        _ = new StateWalk(this);
        _ = new StateHurt(this);
        _ = new StateAttack(this);

    }

    public override void _PhysicsProcess(double delta)
    {
        if (StateID != (int)State.Hurt && Solt != null)
        {
            if (Position.DistanceTo(_Player.Position) < 20)
            {
                Direction = Vector2.Zero;
            }
            else
            {
                if (GlobalPosition.DistanceTo(_Player.GlobalPosition) < 1)
                {
                    Direction = Vector2.Zero;
                }
                else
                {
                    Direction = (Solt.GlobalPosition - GlobalPosition).Normalized();
                }

            }
        }
        StateMachineUpdate(delta);
    }

    private void OnDamageReceiver_DamageReceived(int damage, Vector2 direction, DamageReceiver.HitType hitType)
    {
        Direction = direction;
        SwitchState((int)State.Hurt);
        Health = Mathf.Clamp(Health - damage, 0, MaxHealth);
        if (Health <= 0)
        {
            _Player.FreeSolt(this);
            QueueFree();
        }
    }


    private void OnDamageEmitter_AreaEntered(Area2D area)
    {
        if (area is DamageReceiver a)
        {
            if (AttackRange((a.Owner as Node2D).Position))
            {
                Vector2 direction = (Vector2.Right * _DamageEmitter.Scale.X).Normalized();

                a.EmitSignal(DamageReceiver.SignalName.DamageReceived, Damage, direction);
            }
        }

    }

    private partial class StateIdle : Node, IState
    {
        Enemy Character;
        public int GetId { get; } = (int)State.Idle;
        public StateIdle(Enemy character)
        {
            Character = character;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            Character.Direction = Vector2.Zero;
            Character.AnimationPlayer.Play("Idle");

            Character.Solt ??= Character._Player.ReserveSlot(Character);

            return true;
        }

        public int Update(double delta)
        {
            return Exit();
        }
        public int Exit()
        {
            if (Character.Direction != Vector2.Zero)
            {
                return (int)State.Walk;
            }
            return GetId;
        }
    }
    private partial class StateWalk : Node, IState
    {
        Enemy Character;
        public int GetId { get; } = (int)State.Walk;
        public StateWalk(Enemy character)
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
            if (Character.Direction == Vector2.Zero)
            {
                return (int)State.Idle;
            }
            if (Character.AttackRange(Character._Player.Position))
            {
                // return (int)State.Attack;
            }

            return GetId;
        }
    }
    private partial class StateHurt : Node, IState
    {
        Enemy Character;
        public int GetId { get; } = (int)State.Hurt;
        public StateHurt(Enemy character)
        {
            Character = character;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            Character.AnimationPlayer.Play("Hurt");
            Character.Velocity = Character.Direction * Repel;
            return true;
        }

        public int Update(double delta)
        {
            Character.MoveAndSlide();
            return Exit();
        }
        public int Exit()
        {
            return GetId;
        }
    }
    private partial class StateAttack : Node, IState
    {
        Enemy Character;
        public int GetId { get; } = (int)State.Attack;
        public StateAttack(Enemy character)
        {
            Character = character;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            Character.AnimationPlayer.Play("Punch");
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
