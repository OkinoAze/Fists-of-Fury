using Godot;
using System;

public partial class Barrel : StaticBody2D
{
    [Export]
    int Health = 1;
    [Export]
    float Speed = 30f;
    float Height = 0f;
    float HeightSpeed = 0f;
    float Gravity = 360;
    Vector2 Velocity = Vector2.Zero;
    Sprite2D Sprite;
    DamageReceiver DamageReceiver;
    bool Destroyed = false;
    Player _Player;

    public override void _Ready()
    {
        Sprite = GetNode<Sprite2D>("Sprite2D");
        _Player = GetTree().GetNodesInGroup("Player")[0] as Player;
        DamageReceiver = GetNode<DamageReceiver>("DamageReceiver");
        DamageReceiver.DamageReceived += OnDamageReceiver_DamageReceived;
    }
    public override void _Process(double delta)
    {
        if (Destroyed)
        {
            Height += HeightSpeed * (float)delta;

            if (Height < 0)
            {
                QueueFree();
            }
            else
            {
                HeightSpeed -= Gravity * (float)delta;
            }

            Sprite.Position = Vector2.Up * Height;
            Position += Velocity * (float)delta;
        }
    }
    private void OnDamageReceiver_DamageReceived(int damage, Vector2 direction)
    {
        Health -= damage;
        if (Health <= 0)
        {
            Destroyed = true;
            Sprite.Frame = 1;
            Sprite.FlipH = direction.X < 0;
            HeightSpeed = Speed * 3;
            Velocity = direction.Normalized() * Speed;
        }
    }

}
