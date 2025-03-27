using Godot;
using System;

public partial class DamageReceiver : Area2D
{
    public EventHandler<DamageReceivedEventArgs> DamageReceived;

    public enum HitType
    {
        Normal,
        knockDown,
        Power,
    }
    public class DamageReceivedEventArgs : EventArgs
    {
        public readonly HitType Type;
        public readonly int Damage;
        public readonly float Repel;
        public readonly Vector2 Direction;
        public readonly float HeightSpeed;


        public DamageReceivedEventArgs(Vector2 direction, int damage = 1, float repel = 50, float heightSpeed = 0, HitType type = 0)
        {
            Type = type;
            Damage = damage;
            Direction = direction;
            Repel = repel;
            HeightSpeed = heightSpeed;
        }
    }



}
