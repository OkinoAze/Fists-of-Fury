using Godot;
using System;

public partial class EnemySolt : Node2D
{
    public Enemy Occupant = null;
    public bool IsFree()
    {
        return Occupant == null;
    }
    public void FreeUp()
    {
        Occupant = null;
    }

    public void Occupy(Enemy enemy)
    {
        Occupant = enemy;
    }
}
