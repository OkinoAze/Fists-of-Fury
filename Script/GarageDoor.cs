using Godot;
using System;
using System.Collections.Generic;

public partial class GarageDoor : Sprite2D
{
    [Export]
    public BattleArea BattleArea;
    AnimationPlayer _AnimationPlayer;
    List<SpawnPoint> SpawnPoints;
    List<Enemy> Enemies = [];
    public override void _Ready()
    {
        _AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        SpawnPoints = [GetNode<SpawnPoint>("SpawnPoint"), GetNode<SpawnPoint>("SpawnPoint2")];
        EntityManager.Instance.EnterBattleArea += OnEnterBattleArea;
    }

    private void OnEnterBattleArea(BattleArea battleArea)
    {
        if (battleArea == BattleArea)
        {
            foreach (var spawnPoint in SpawnPoints)
            {
                if (spawnPoint.Enemies.Count > 0)
                {
                    var e = EntityManager.Instance.GenerateActor(spawnPoint.Enemies[0], spawnPoint.GlobalPosition, spawnPoint.MovePoint.GlobalPosition);
                    e.ZIndex = -1;
                    e.Visible = true;
                    e.ProcessMode = ProcessModeEnum.Disabled;
                    BattleArea.ActiveEnemies.Add(e);
                    Enemies.Add(e);
                    spawnPoint.Enemies = spawnPoint.Enemies.Slice(1, spawnPoint.Enemies.Count);
                }
            }
            _AnimationPlayer.Play("OpenDoor");
            BattleArea.AutoStart = true;
        }
    }
    public async void OpenDoorEnd()
    {
        foreach (var item in Enemies)
        {
            item.ZIndex = 0;
            item.ProcessMode = ProcessModeEnum.Inherit;
        }
        _AnimationPlayer.Play("CloseDoor");
        await ToSignal(GetTree().CreateTimer(2), SceneTreeTimer.SignalName.Timeout);
        ProcessMode = ProcessModeEnum.Disabled;
    }
}
