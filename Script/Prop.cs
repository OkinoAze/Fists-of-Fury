using Godot;
using System;

public partial class Prop : Resource
{

    [ExportGroup("物品属性")]
    [Export]
    public int ID = 0;
    [Export]
    public Properties Property = 0;
    public enum Properties
    {
        Restore,
        MeleeWeapon,
        RangedWeapon,
    }
    [ExportGroup("拾取增益")]
    [Export]
    public int RestoreHealth = 0;
    [ExportGroup("武器属性")]
    [Export]
    public int Damage = 0;
    [Export]
    public int Durability = 0;
    [Export]
    public Vector2 ShotPosition = Vector2.Zero;

}
