using Godot;
using System;
using System.Collections.Generic;
[Tool]
public partial class BattleArea : Area2D
{
    [Export]
    Color LineColor = Colors.Red;
    public List<SpawnPoint> SpawnPoints = [];
    public List<Enemy> EnemyInstances = [];
    public override void _Ready()
    {
        var children = GetChildren();
        foreach (var item in children)
        {
            if (item is SpawnPoint point)
            {
                SpawnPoints.Add(point);
            }
        }
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        SetDeferred("monitoring", false);
    }


    public override void _PhysicsProcess(double delta)
    {
        if (Monitoring == false)
        {
            foreach (var item in SpawnPoints)
            {
                if (item?.Enemies.Count > 0)
                {
                    var e = EntityManager.Instance.GenerateActor(item.Enemies[0], item.GlobalPosition, item.MovePoint.GlobalPosition);
                    item.Enemies.RemoveAt(0);
                    GD.Print(e);
                }
            }
        }
    }
    public override void _Draw()
    {
        if (Engine.IsEditorHint())
        {
            DrawRect(new Rect2(new Vector2(-50, -32), new Vector2(100, 64)), LineColor, false);
        }
    }
}
