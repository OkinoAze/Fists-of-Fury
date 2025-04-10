using Godot;
using System;
using System.Collections.Generic;
[Tool]
public partial class BattleArea : Area2D
{
    [Export]
    Color LineColor = Colors.Red;
    [Export]
    public bool AutoStart = false;
    public int RemainingEnemies = 0;
    public List<SpawnPoint> SpawnPoints = [];
    public int MaxEnemies = 5;

    public bool Spawn = false;


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
        EntityManager.Instance.EnterBattleArea(this);
    }


    public override void _PhysicsProcess(double delta)
    {
        if (!Engine.IsEditorHint() && AutoStart == true && Monitoring == false)
        {
            var EnemyCount = GetTree().GetNodesInGroup("Enemy").Count;

            if (EnemyCount == 0)
            {
                Spawn = true;

            }

            if (Monitoring == false && Spawn == true)
            {
                RemainingEnemies = 0;
                foreach (var item in SpawnPoints)
                {
                    if (item.Enemies.Count > 0 && EnemyCount <= MaxEnemies)
                    {
                        var e = EntityManager.Instance.GenerateActor(item.Enemies[0], item.GlobalPosition, item.MovePoint.GlobalPosition);
                        item.Enemies.RemoveAt(0);
                    }
                    RemainingEnemies += item.Enemies.Count;
                }

                Spawn = false;
            }

            EnemyCount = GetTree().GetNodesInGroup("Enemy").Count;

            if (RemainingEnemies == 0 && EnemyCount == 0)
            {
                EntityManager.Instance.ExitBattleArea(this);
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
