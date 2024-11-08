using Godot;

namespace PuzzleGameCourse;

public partial class Main : Node2D
{
    private Sprite2D _sprite;
    private PackedScene _buildingScene;

    public override void _Ready()
    {
        _buildingScene = GD.Load<PackedScene>("res://scenes/building/Building.tscn");
        _sprite = GetNode<Sprite2D>("Cursor");
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("left_click"))
        {
            PlaceBuildingAtMousePosition();
        }
    }

    public override void _Process(double delta)
    {
        _sprite.GlobalPosition = GetMouseGridCellPosition() * 64;
    }

    private Vector2 GetMouseGridCellPosition()
    {
        var mousePosition = GetGlobalMousePosition();
        var gridPosition = mousePosition / 64;
        gridPosition = gridPosition.Floor();

        return gridPosition;
    }

    private void PlaceBuildingAtMousePosition()
    {
        var building = _buildingScene.Instantiate<Node2D>();
        AddChild(building);
        
        var gridPosition = GetMouseGridCellPosition();
        building.GlobalPosition = gridPosition * 64;
    }
}