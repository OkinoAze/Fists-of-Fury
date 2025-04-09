using Godot;
using System;
using System.IO;

public partial class ActorContainer : Node2D
{
    public override void _Ready()
    {
        EntityManager.Instance.GenerateActor += OnGenerateActor;
        EntityManager.Instance.GenerateBullet += OnGenerateBullet;
        EntityManager.Instance.GenerateProp += OnGenerateProp;
        EntityManager.Instance.GeneratePropName += OnGeneratePropName;

    }

    private void OnGeneratePropName(string propName, Vector2 position)
    {
        var path = "res://Scene/Prefab/" + propName + ".tscn";
        bool exists = ResourceLoader.Exists(path);
        if (exists)
        {
            var propScene = ResourceLoader.Load<PackedScene>(path, null, ResourceLoader.CacheMode.Reuse);
            var prop = propScene.Instantiate<Prop>();
            prop.Position = position;
            AddChild(prop);
        }

    }


    private void OnGenerateProp(Prop propInstance, Vector2 position)
    {
        var path = "res://Scene/Prefab/" + propInstance.GetType().Name + ".tscn";
        bool exists = ResourceLoader.Exists(path);
        if (exists)
        {
            var propScene = ResourceLoader.Load<PackedScene>(path, null, ResourceLoader.CacheMode.Reuse);
            var prop = propScene.Instantiate<Prop>();
            prop.Durability = propInstance.Durability;
            prop.Position = position;
            AddChild(prop);
        }

    }


    private void OnGenerateBullet(int damage, Vector2 direction, Vector2 position, Vector2 shotPosition)
    {
        var path = "res://Scene/Prefab/Bullet.tscn";
        var bulletScene = ResourceLoader.Load<PackedScene>(path, null, ResourceLoader.CacheMode.Reuse);
        Bullet bullet = bulletScene.Instantiate<Bullet>();


        bullet.Damage = damage;
        bullet.Position = position;
        bullet.Direction = direction;
        AddChild(bullet);
        bullet.Sprite.Position = shotPosition;
        bullet.Visible = false;

    }


    private Character OnGenerateActor(EntityManager.EnemyType type, Vector2 position, float height, float heightSpeed, Prop[] props)
    {
        var path = "res://Scene/Prefab/Enemy" + Enum.GetName(type) + ".tscn";
        bool exists = ResourceLoader.Exists(path);
        if (exists)
        {
            var characterScene = ResourceLoader.Load<PackedScene>(path, null, ResourceLoader.CacheMode.Reuse);
            Character enemy = characterScene.Instantiate<Character>();
            enemy.Position = position;
            enemy.Height = height;
            enemy.HeightSpeed = heightSpeed;
            AddChild(enemy);
            foreach (var item in props)
            {
                enemy.PickUpProp(item);
            }
            return enemy;
        }
        return null;
    }
}
