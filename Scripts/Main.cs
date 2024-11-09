using Godot;

namespace PuzzleGameCourse;

public partial class Main : Node
{
    private GridManager _gridManager;
    private Sprite2D _cursor;
    private PackedScene _buildingScene;
    private Button _placeBuildingButton;

    private Vector2I? _hoveredGridCell;

    public override void _Ready()
    {
        _buildingScene = GD.Load<PackedScene>("res://scenes/building/Building.tscn");
        _gridManager = GetNode<GridManager>("GridManager");
        _cursor = GetNode<Sprite2D>("Cursor");
        _placeBuildingButton = GetNode<Button>("PlaceBuildingButton");

        _cursor.Visible = false;

        _placeBuildingButton.Pressed += OnButtonPressed;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_hoveredGridCell.HasValue && @event.IsActionPressed("left_click") &&
            _gridManager.IsTilePositionValid(_hoveredGridCell.Value))
        {
            PlaceBuildingAtHoveredCellPosition();
            _cursor.Visible = false;
        }
    }

    public override void _Process(double delta)
    {
        var gridPosition = _gridManager.GetMouseGridCellPosition();
        _cursor.GlobalPosition = gridPosition * 64;

        if (_cursor.Visible && (!_hoveredGridCell.HasValue || gridPosition != _hoveredGridCell.Value))
        {
            _hoveredGridCell = gridPosition;
            _gridManager.HighlightValidTilesInRadius(_hoveredGridCell.Value, 3);
        }
    }

    private void PlaceBuildingAtHoveredCellPosition()
    {
        if (!_hoveredGridCell.HasValue)
            return;

        var building = _buildingScene.Instantiate<Node2D>();
        AddChild(building);

        building.GlobalPosition = _hoveredGridCell.Value * 64;
        _gridManager.MarkTileAsOccupied(_hoveredGridCell.Value);

        _hoveredGridCell = null;
        _gridManager.ClearHighlightedTiles();
    }

    private void OnButtonPressed()
    {
        _cursor.Visible = true;
    }
}