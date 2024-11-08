using Godot;

namespace PuzzleGameCourse;

public partial class Main : Node2D
{
    private Sprite2D _sprite;
    public override void _Ready()
    {
        _sprite = GetNode<Sprite2D>("Cursor");
    }

    public override void _Process(double delta)
    {
        var mousePosition = GetGlobalMousePosition();
        var gridPosition = mousePosition / 64;
        gridPosition = gridPosition.Floor();

        _sprite.GlobalPosition = gridPosition * 64;
    }
}