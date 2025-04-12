using Godot;
using System;

public partial class DamageEmitter : Area2D
{
    public delegate void AttackSuccessReceiver(Character character);
    public AttackSuccessReceiver AttackSuccess;

}
