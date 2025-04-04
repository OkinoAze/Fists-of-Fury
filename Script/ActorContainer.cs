using Godot;
using System;

public partial class ActorContainer : Node2D
{
    //TODO 角色生成
    public override void _Ready()
    {
        EntityManager.Instance.GenerateActor += OnGenerateActor;
        EntityManager.Instance.GenerateBullet += OnGenerateBullet;
    }

    void OnGenerateActor(object sender, EntityManager.GenerateActorEventArgs e)
    {
        var path = "res://Scene/Prefab/" + Enum.GetName(e.Type) + ".tscn";
        PackedScene characterScene = ResourceLoader.Load<PackedScene>(path);
        Enemy enemy = characterScene.Instantiate<Enemy>();
        AddChild(enemy);
        enemy.Position = e.Position;
        enemy.Height = e.Height;
        enemy.HeightSpeed = e.HeightSpeed;
        foreach (var item in e.Props)
        {
            enemy.PickUpProp(item);
        }
    }
    void OnGenerateBullet(object sender, EntityManager.GenerateBulletEventArgs e)
    {
        var path = "res://Scene/Prefab/Bullet.tscn";
        PackedScene bulletScene = ResourceLoader.Load<PackedScene>(path);
        var bullet = bulletScene.Instantiate<Bullet>();
        AddChild(bullet);
        bullet.Position = e.Position;
    }

}
