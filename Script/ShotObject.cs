using Godot;
using System;

public partial class ShotObject : StaticObject
{
    public int Damage = 1;
    protected const float _AttackRange = 5;
    public DamageEmitter _DamageEmitter;
    public Sprite2D Sprite;
    protected VisibleOnScreenNotifier2D OnScreen;


    private void OnDamageEmitter_AttackSuccess()
    {

    }
    public void AccessingResources()
    {
        OnScreen = GetNode<VisibleOnScreenNotifier2D>("VisibleOnScreenNotifier2D");
        Sprite = GetNode<Sprite2D>("Sprite2D");
        _DamageEmitter = Sprite.GetNode<DamageEmitter>("DamageEmitter");
    }


    public bool AttackRange(Vector2 position)
    {
        if (position.Y > Position.Y - _AttackRange && position.Y < Position.Y + _AttackRange)
        {
            return true;
        }
        return false;
    }

}
