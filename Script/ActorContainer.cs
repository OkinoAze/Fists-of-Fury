using Godot;
using System;

public partial class ActorContainer : Node2D
{
    //TODO 角色生成
    public enum EnemyType
    {
        Punk,
        Goon,
        Thug,
        Boss,
    }
    public void GenerateActor(EnemyType type, Vector2 position, float height, float heightSpeed, Prop[] props)
    {
        var path = "res://Scene/Prefab/" + Enum.GetName(type) + ".tscn";
        PackedScene characterScene = ResourceLoader.Load<PackedScene>(path);
        Enemy enemy = characterScene.Instantiate<Enemy>();
        AddChild(enemy);
        enemy.Position = position;
        enemy.Height = height;
        enemy.HeightSpeed = heightSpeed;
        foreach (var item in props)
        {
            enemy.PickUpProp(item);
        }
    }

}
