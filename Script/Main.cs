using Godot;
using System;

public partial class Main : Node
{
    [Export]
    public Player _Player;
    [Export]
    public Camera2D Camera;
    bool StopCamera = false;
    public override void _Ready()
    {
        EntityManager.Instance.EnterBattleArea += OnEnterBattleArea;
        EntityManager.Instance.ExitBattleArea += OnExitBattleArea;
    }

    private void OnEnterBattleArea(BattleArea battleArea)
    {
        StopCamera = true;
    }

    private void OnExitBattleArea(BattleArea battleArea)
    {
        StopCamera = false;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (StopCamera = false && _Player.Position.X > Camera.Position.X)
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
