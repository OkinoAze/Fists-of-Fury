using Godot;
using System;

public partial class Barrel : StaticBody2D
{
    int Health = 10;
    Sprite2D Sprite;
    public override void _Ready()
    {
        Sprite = GetNode<Sprite2D>("Sprite2D");
        GetNode<DamageReceiver>("DamageReceiver").DamageReceived += OnDamageReceived;
    }

    private void OnDamageReceived(int damage)
    {
        QueueFree();
    }

}
