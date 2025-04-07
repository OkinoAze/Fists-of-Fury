using Godot;
using System;

public partial class EntityManager : Node
{
    public delegate void GenerateActorReceiver(EnemyType type, Vector2 position, float height, float heightSpeed, Prop[] props);
    public delegate void GenerateBulletReceiver(int damage, Vector2 direction, Vector2 position, Vector2 shotPosition);
    public delegate void GeneratePropReceiver(Prop propInstance, Vector2 position);
    public delegate void GeneratePropNameReceiver(string propName, Vector2 position);
    public GenerateActorReceiver GenerateActor;
    public GenerateBulletReceiver GenerateBullet;
    public GeneratePropReceiver GenerateProp;
    public GeneratePropNameReceiver GeneratePropName;

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


