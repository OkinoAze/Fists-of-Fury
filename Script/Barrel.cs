using Godot;
using System;

public partial class Barrel : StaticObject
{
    [Export]
    int Health = 1;
    [Export]
    string HasProp = "";
    Sprite2D Sprite;
    DamageReceiver _DamageReceiver;
    AudioStreamPlayer AudioPlayer;
    enum State
    {
        Idle,
        Destroyed
    }
    public override void _Ready()
    {
        States = new IState[Enum.GetNames(typeof(State)).Length];

        Sprite = GetNode<Sprite2D>("Sprite2D");
        AudioPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
        _DamageReceiver = GetNode<DamageReceiver>("DamageReceiver");

        _DamageReceiver.DamageReceived += OnDamageReceiver_DamageReceived;

        _ = new StateIdle(this);
        _ = new StateDestroyed(this);

    }

    public override void _PhysicsProcess(double delta)
    {
        StateMachineUpdate(delta);
    }
    private void OnDamageReceiver_DamageReceived(DamageEmitter emitter, DamageReceiver.DamageReceivedEventArgs e)
    {
        Health -= e.Damage;
        emitter?.AttackSuccess();
        if (e.Type == DamageReceiver.HitType.knockDown || Health <= 0)
        {
            PlayAudio("hit-2");
            EntityManager.Instance.GenerateParticle(e.Position, e.Direction.X < 0);

        }
        else
        {
            PlayAudio("hit-1");
        }
        if (Health <= 0)
        {
            Sprite.Frame = 1;
            Sprite.FlipH = e.Direction.X < 0;
            Velocity = e.Direction.Normalized() * MoveSpeed;
        }
    }
    public void PlayAudio(string name)
    {
        AudioPlayer.Stream = ResourceLoader.Load<AudioStreamWav>("res://Music/SFX/" + name + ".wav");
        AudioPlayer.Play();
    }
    public partial class StateIdle : Node, IState
    {
        Barrel character;
        public int GetId { get; } = (int)State.Idle;
        public StateIdle(Barrel c)
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
            if (character.Health <= 0)
            {
                return (int)State.Destroyed;
            }
            return GetId;
        }
    }
    public partial class StateDestroyed : Node, IState
    {
        Barrel character;
        public int GetId { get; } = (int)State.Destroyed;
        public StateDestroyed(Barrel c)
        {
            character = c;
            character.States[GetId] = this;
        }
        public bool Enter()
        {
            character._DamageReceiver.Monitorable = false;
            character.HeightSpeed = character.MoveSpeed * 3;
            if (character.HasProp != "")
            {
                EntityManager.Instance.GeneratePropName(character.HasProp, character.Position);
            }
            return true;
        }

        public int Update(double delta)
        {
            character.Height += character.HeightSpeed * (float)delta;

            if (character.Height < 0)
            {
                character.QueueFree();
            }
            else
            {
                character.HeightSpeed -= Gravity * (float)delta;
            }

            character.Sprite.Position = Vector2.Up * character.Height;
            character.Position += character.Velocity * (float)delta;
            return Exit();
        }
        public int Exit()
        {
            return GetId;
        }
    }
}
