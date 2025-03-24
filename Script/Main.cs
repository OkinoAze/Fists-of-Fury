using Godot;
using System;

public partial class Main : Node
{
    [Export]
    CharacterBody2D Player;
    [Export]
    Camera2D Camera;

    public override void _Ready()
    {

    }
    public override void _Process(double delta)
    {
        if (Player.Position.X > Camera.Position.X)
        {
            Camera.Position = new Vector2(Player.Position.X, Camera.Position.Y);

        }
    }
}
