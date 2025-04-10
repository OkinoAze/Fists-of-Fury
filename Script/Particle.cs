using Godot;
using System;

public partial class Particle : AnimatedSprite2D
{
    public override void _Ready()
    {
        AnimationFinished += OnAnimationFinished;
    }
    private void OnAnimationFinished()
    {
        CallDeferred("queue_free");
    }

}
