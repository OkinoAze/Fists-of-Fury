using Godot;
using System;

public partial class DamageReceiver : Area2D
{
    public enum HitType
    {
        Normal,
        knockDown,
        Power,
    }
    [Signal]
    public delegate void DamageReceivedEventHandler(int damage, Vector2 direction, HitType hitType = HitType.Normal);

}
