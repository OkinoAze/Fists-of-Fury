using Godot;
using System;

public partial class EntityManager : Node
{
    public delegate void GenerateActorReceiver(EnemyType type, Vector2 position, float height, float heightSpeed, Prop[] props);
    public delegate void GenerateBulletReceiver(Character sender, int damage, Vector2 direction, Vector2 position, Vector2 shotPosition);
    public delegate void GeneratePropReceiver(Character sender, Prop prop, Vector2 position);
    public GenerateActorReceiver GenerateActor;
    public GenerateBulletReceiver GenerateBullet;
    public GeneratePropReceiver GenerateProp;

    public enum Props
    {
        Chicken,
        Knife,
        Gun,

    }
    public Prop GetProp(Props prop)
    {
        var path = "res://Scene/Props/" + Enum.GetName(prop) + ".tres";
        return ResourceLoader.Load<Prop>(path);
    }
    public static EntityManager Instance { get; private set; }
    public enum EnemyType
    {
        Punk,
        Goon,
        Thug,
        Boss,
    }
    EntityManager()
    {
        Instance = this;
    }

}


