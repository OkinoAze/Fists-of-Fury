using Godot;
using System;

public partial class EntityManager : Node
{
    public event EventHandler<GenerateActorEventArgs> GenerateActor;
    public event EventHandler<GenerateBulletEventArgs> GenerateBullet;

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

    public class GenerateActorEventArgs(EnemyType type, Vector2 position, float height, float heightSpeed, Prop[] props)
    {
        public EnemyType Type = type;
        public Vector2 Position = position;
        public float Height = height;
        public float HeightSpeed = heightSpeed;
        public Prop[] Props = props;
    }
    public class GenerateBulletEventArgs(Vector2 position)
    {
        public Vector2 Position = position;
    }

}


