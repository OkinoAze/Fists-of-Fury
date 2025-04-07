using Godot;
using System;

public partial class Main : Node
{
    [Export]
    public Player _Player;
    [Export]
    public Camera2D Camera;


    public override void _PhysicsProcess(double delta)
    {
        if (_Player.Position.X > Camera.Position.X)
        {
            Camera.Position = new Vector2(_Player.Position.X, Camera.Position.Y);

        }
        if (Input.IsActionPressed("ui_filedialog_refresh"))
        {
            EntityManager.Instance.GenerateBullet = null;
            EntityManager.Instance.GenerateActor = null;
            EntityManager.Instance.GenerateProp = null;
            EntityManager.Instance.GeneratePropName = null;
            GetTree().ReloadCurrentScene();
        }
    }
}
