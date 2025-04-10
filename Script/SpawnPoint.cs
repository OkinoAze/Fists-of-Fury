using Godot;
using System;
[Tool]
public partial class SpawnPoint : Marker2D
{
    [Export]
    public Color LineColor = Colors.Red;
    public Marker2D MovePoint;
    public
    public override void _Ready()
    {
        MovePoint = GetNode<Marker2D>("MovePoint");
    }
    public override void _PhysicsProcess(double delta)
    {
        if (Engine.IsEditorHint())
        {
            QueueRedraw();
        }

    }
    public override void _Draw()
    {
        if (Engine.IsEditorHint())
        {
            DrawSetTransform(default, default, new Vector2(7, 2));
            DrawCircle(Vector2.Zero, 1, LineColor, false);
            DrawSetTransform(default);
            DrawLine(Vector2.Zero, MovePoint.Position, LineColor);
        }
    }
}
