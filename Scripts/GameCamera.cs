using Godot;

namespace PuzzleGameCourse;

public partial class GameCamera : Camera2D
{
    private const int TileSize = 64;
    private const float PanSpeed = 400;
    private static readonly StringName ActionPanLeft = "pan_left";
    private static readonly StringName ActionPanRight = "pan_right";
    private static readonly StringName ActionPanUp = "pan_up";
    private static readonly StringName ActionPanDown = "pan_down";
    
    public override void _Process(double delta)
    {
        GlobalPosition = GetScreenCenterPosition();
        var movementVector = Input.GetVector(ActionPanLeft, ActionPanRight, ActionPanUp, ActionPanDown);
        GlobalPosition += movementVector * PanSpeed * (float)delta;
    }

    public void SetBoundingRect(Rect2I boundingRect)
    {
        LimitLeft = boundingRect.Position.X * TileSize;
        LimitRight = boundingRect.End.X * TileSize;
        LimitTop = boundingRect.Position.Y * TileSize;
        LimitBottom = boundingRect.End.Y * TileSize;
    }
}