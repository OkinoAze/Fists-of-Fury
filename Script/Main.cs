using Godot;
using System;

public partial class Main : Node
{
    [Export]
    public Player _Player;
    [Export]
    public Camera2D Camera;
    [Export]
    float strength = 1.5f;
    bool ShackCamera = false;
    bool StopCamera = false;
    [Export]
    float ShackTime = 0.2f;
    float Time = 0;
    public override void _Ready()
    {
        EntityManager.Instance.EnterBattleArea += OnEnterBattleArea;
        EntityManager.Instance.ExitBattleArea += OnExitBattleArea;
        EntityManager.Instance.ShackCamera += OnShackCamera;
    }

    private void OnShackCamera()
    {
        ShackCamera = true;
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
        if (StopCamera == false && _Player.Position.X > Camera.Position.X)
        {
            Camera.Position = new Vector2(_Player.Position.X, Camera.Position.Y);
        }
        if (ShackCamera == true && Time < ShackTime)
        {
            Time += (float)delta;
            Camera.Offset = new Vector2((float)GD.RandRange(-strength, strength), (float)GD.RandRange(-strength, strength));
        }
        else
        {
            ShackCamera = false;
            Time = 0;
            Camera.Offset = new Vector2(0, 0);
        }
        if (Input.IsActionPressed("ui_filedialog_refresh"))
        {
            EntityManager.Instance.GenerateBullet = null;
            EntityManager.Instance.GenerateActor = null;
            EntityManager.Instance.GenerateProp = null;
            EntityManager.Instance.GeneratePropName = null;
            EntityManager.Instance.GenerateParticle = null;
            EntityManager.Instance.ShackCamera = null;
            GetTree().ReloadCurrentScene();
        }
    }
}
