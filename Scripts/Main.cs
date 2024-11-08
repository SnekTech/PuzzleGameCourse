using Godot;

namespace PuzzleGameCourse;

public partial class Main : Node2D
{
    private Sprite2D _cursor;
    private PackedScene _buildingScene;
    private Button _placeBuildingButton;

    public override void _Ready()
    {
        _buildingScene = GD.Load<PackedScene>("res://scenes/building/Building.tscn");
        _cursor = GetNode<Sprite2D>("Cursor");
        _placeBuildingButton = GetNode<Button>("PlaceBuildingButton");
        
        _cursor.Visible = false;

        _placeBuildingButton.Pressed += OnButtonPressed;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_cursor.Visible && @event.IsActionPressed("left_click"))
        {
            PlaceBuildingAtMousePosition();
            _cursor.Visible = false;
        }
    }

    public override void _Process(double delta)
    {
        _cursor.GlobalPosition = GetMouseGridCellPosition() * 64;
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

    private void OnButtonPressed()
    {
        _cursor.Visible = true;
    }
}