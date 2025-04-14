using Godot;
using System;
using System.Collections.Generic;
[Tool]
public partial class BattleArea : Area2D
{
    [Export]
    Color LineColor = Colors.Red;
    [Export]
    public bool AutoStart = true;
    public int RemainingEnemies = 0;
    public List<SpawnPoint> SpawnPoints = [];
    [Export]
    public Godot.Collections.Array<Character> ActiveEnemies = [];
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
        EntityManager.Instance.CallDeferred("CallEnterBattleArea", this);
        SetDeferred("monitoring", false);
    }


    public override void _PhysicsProcess(double delta)
    {
        if (!Engine.IsEditorHint() && AutoStart == true && Monitoring == false)
        {

            if (ActiveEnemies.Count == 0)
            {
                Spawn = true;
            }

            if (Monitoring == false && Spawn == true)
            {
                RemainingEnemies = 0;
                if (SpawnPoints.Count > 0)
                {
                    foreach (var item in SpawnPoints)
                    {
                        if (item.Enemies.Count > 0 && ActiveEnemies.Count <= MaxEnemies)
                        {
                            var e = EntityManager.Instance.GenerateActor(item.Enemies[0], item.GlobalPosition, item.MovePoint.GlobalPosition, item.Height, item.HeightSpeed);
                            ActiveEnemies.Add(e);
                            item.Enemies = item.Enemies.Slice(1, item.Enemies.Count);

                        }
                        RemainingEnemies += item.Enemies.Count;

                    }
                }

                Spawn = false;
            }
            if (ActiveEnemies.Count > 0)
            {
                Godot.Collections.Array<Character> l = [];
                foreach (var item in ActiveEnemies)
                {
                    if (item != null)
                    {
                        l.Add(item);
                    }
                }
                ActiveEnemies = l;
            }

            if (RemainingEnemies == 0 && ActiveEnemies.Count == 0)
            {
                EntityManager.Instance.ExitBattleArea(this);
                QueueFree();
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
