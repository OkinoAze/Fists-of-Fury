using Godot;
using System;

public partial class Enemy : Character
{
    Player _Player;
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

        AccessingResources();

        _DamageEmitter.AreaEntered += OnDamageEmitter_AreaEntered;
        (_DamageReceiver as DamageReceiver).DamageReceived += OnDamageReceiver_DamageReceived;

        _ = new StateIdle(this);
        _ = new StateWalk(this);

    }

    private void OnDamageReceiver_DamageReceived(int damage, Vector2 direction)
    {
        throw new NotImplementedException();
    }


    private void OnDamageEmitter_AreaEntered(Area2D area)
    {
        Vector2 direction = Vector2.Right * _DamageEmitter.Scale.X;

        (area as DamageReceiver)?.EmitSignal("DamageReceived", Damage, direction);

    }



    public override void _PhysicsProcess(double delta)
    {
        Direction = (_Player.Position - Position).Normalized();
        StateMachineUpdate(delta);
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
}
