using Godot;
using System;

public partial class DamageReceiver : Area2D
{
    [Signal]
    public delegate void DamageReceivedEventHandler(int damage);

}
