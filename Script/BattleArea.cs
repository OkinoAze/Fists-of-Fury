using Godot;
using System;
using System.Collections.Generic;
[Tool]
public partial class BattleArea : Area2D
{
    [Export]
    public Color LineColor = Colors.Red;
    public List<SpawnPoint> SpawnPoints;
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
        if (body is Player player)
        {
            Monitoring = false;

        }
    }


    public override void _PhysicsProcess(double delta)
    {
        if (Monitoring == false)
        {
            foreach (var item in SpawnPoints)
            {
                EntityManager.Instance.GenerateActor(EntityManager.EnemyType.Punk, item.GlobalPosition);
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
