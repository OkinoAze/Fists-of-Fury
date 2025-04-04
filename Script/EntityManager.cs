using Godot;
using System;
using System.Reflection;

public partial class EntityManager : Node
{
    public delegate void GenerateActorReceiver(EnemyType type, Vector2 position, float height, float heightSpeed, Prop[] props);
    public delegate void GenerateBulletReceiver(Character sender, int damage, Vector2 direction, Vector2 position, Vector2 shotPosition);
    public delegate void GeneratePropReceiver(Character sender, Vector2 position);
    public GenerateActorReceiver GenerateActor;
    public GenerateBulletReceiver GenerateBullet;
    public GeneratePropReceiver GenerateProp;


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


