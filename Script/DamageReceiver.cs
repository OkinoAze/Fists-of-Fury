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
    public class DamageReceivedEventArgs(Vector2 direction, int damage = 1, float repel = 5, float heightSpeed = 20, HitType type = 0) : EventArgs
    {
        public readonly HitType Type = type;
        public readonly int Damage = damage;
        public readonly float Repel = repel;
        public readonly Vector2 Direction = direction;
        public readonly float HeightSpeed = heightSpeed;
    }



}
