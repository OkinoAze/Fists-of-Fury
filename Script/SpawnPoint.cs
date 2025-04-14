using Godot;
using System;
[Tool]
public partial class SpawnPoint : Marker2D
{
    [Export]
    Color LineColor = Colors.Red;
    public Marker2D MovePoint;
    [Export]
    public float Height = 0;
    [Export]
    public float HeightSpeed = 0;
    [Export]
    public Godot.Collections.Array<PackedScene> Enemies;
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
