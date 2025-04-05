using Godot;
using System;

public partial class DamageEmitter : Area2D
{
    public delegate void AttackSuccessReceiver();
    public AttackSuccessReceiver AttackSuccess;

}
