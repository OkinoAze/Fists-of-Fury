using Godot;
using System;

public partial class Main : Node
{
    [Export]
    CharacterBody2D Player;
    [Export]
    Camera2D Camera;

    public override void _PhysicsProcess(double delta)
    {
        if (Player.Position.X > Camera.Position.X)
        {
            Camera.Position = new Vector2(Player.Position.X, Camera.Position.Y);

        }
        if (Input.IsActionPressed("ui_filedialog_refresh"))
        {
            GetTree().ReloadCurrentScene();
        }
    }
}
