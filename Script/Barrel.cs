using Godot;
using System;

public partial class Barrel : StaticObject
{
    [Export]
    int Health = 1;
    Sprite2D Sprite;
    DamageReceiver DamageReceiver;
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
        DamageReceiver = GetNode<DamageReceiver>("DamageReceiver");

        DamageReceiver.DamageReceived += OnDamageReceiver_DamageReceived;

        _ = new StateIdle(this);
        _ = new StateDestroyed(this);
    }

    public override void _PhysicsProcess(double delta)
    {
        StateMachineUpdate(delta);
    }
    private void OnDamageReceiver_DamageReceived(Node2D sender, DamageReceiver.DamageReceivedEventArgs e)
    {
        Health -= e.Damage;
        if (e.Type == DamageReceiver.HitType.knockDown)
        {
            PlayAudio("hit-2");
        }
        else
        {
            PlayAudio("hit-1");
        }
        if (Health <= 0)
        {
            Sprite.Frame = 1;
            Sprite.FlipH = e.Direction.X < 0;
            HeightSpeed = MoveSpeed * 3;
            Velocity = e.Direction.Normalized() * MoveSpeed;
            SwitchState((int)State.Destroyed);
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
