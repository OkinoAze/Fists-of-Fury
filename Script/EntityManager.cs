using Godot;
using System;

public partial class EntityManager : Node
{
    public delegate Enemy GenerateActorReceiver(PackedScene packedScene, Vector2 position, Vector2 movePoint, float height = 0, float heightSpeed = 0);
    public delegate void GenerateBulletReceiver(Character character, int damage, Vector2 direction, Vector2 position, Vector2 shotPosition);
    public delegate void GeneratePropReceiver(Prop propInstance, Vector2 position);
    public delegate void GeneratePropNameReceiver(string propName, Vector2 position);
    public delegate void GenerateParticleReceiver(Vector2 position, bool flipH = false);


    public GenerateActorReceiver GenerateActor;
    public GenerateBulletReceiver GenerateBullet;
    public GeneratePropReceiver GenerateProp;
    public GeneratePropNameReceiver GeneratePropName;
    public GenerateParticleReceiver GenerateParticle;


    public delegate void EnterBattleAreaReceiver(BattleArea battleArea);
    public EnterBattleAreaReceiver EnterBattleArea;
    public delegate void ExitBattleAreaReceiver(BattleArea battleArea);
    public ExitBattleAreaReceiver ExitBattleArea;
    public delegate void ShackCameraReceiver();
    public ShackCameraReceiver ShackCamera;
    public delegate void ReSpawnPlayerReceiver();
    public ReSpawnPlayerReceiver ReSpawnPlayer;


    public static EntityManager Instance { get; private set; }
    public enum EnemyType
    {
        Punk,
        Goon,
        Thug,
        Boss,
    }
    public enum WeaponType
    {
        Default,
        PropKnife,
        PropGun,
    }
    EntityManager()
    {
        Instance = this;
    }

}


