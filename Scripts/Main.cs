using System.Collections.Generic;
using Godot;

namespace PuzzleGameCourse;

public partial class Main : Node2D
{
    private Sprite2D _cursor;
    private PackedScene _buildingScene;
    private Button _placeBuildingButton;
    private TileMapLayer _highlightTileMapLayer;

    private Vector2? _hoveredGridCell;
    private HashSet<Vector2> _occupiedCells = new();

    public override void _Ready()
    {
        _buildingScene = GD.Load<PackedScene>("res://scenes/building/Building.tscn");
        _cursor = GetNode<Sprite2D>("Cursor");
        _placeBuildingButton = GetNode<Button>("PlaceBuildingButton");
        _highlightTileMapLayer = GetNode<TileMapLayer>("HighlightTileMapLayer");

        _cursor.Visible = false;

        _placeBuildingButton.Pressed += OnButtonPressed;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        var clickedCellIsOccupied = _occupiedCells.Contains(GetMouseGridCellPosition());
        if (_cursor.Visible && @event.IsActionPressed("left_click") && !clickedCellIsOccupied)
        {
            PlaceBuildingAtMousePosition();
            _cursor.Visible = false;
        }
    }

    public override void _Process(double delta)
    {
        var gridPosition = GetMouseGridCellPosition();
        _cursor.GlobalPosition = gridPosition * 64;

        if (_cursor.Visible && (!_hoveredGridCell.HasValue || gridPosition != _hoveredGridCell.Value))
        {
            _hoveredGridCell = gridPosition;
            UpdateHighlightTileMapLayer();
        }
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
        _occupiedCells.Add(gridPosition);

        _hoveredGridCell = null;
        UpdateHighlightTileMapLayer();
    }

    private void UpdateHighlightTileMapLayer()
    {
        _highlightTileMapLayer.Clear();
        
        if (!_hoveredGridCell.HasValue)
            return;

        for (var x = _hoveredGridCell.Value.X - 3; x <= _hoveredGridCell.Value.X + 3; x++)
        {
            for (var y = _hoveredGridCell.Value.Y - 3; y <= _hoveredGridCell.Value.Y + 3; y++)
            {
                _highlightTileMapLayer.SetCell(new Vector2I((int)x, (int)y), 0, Vector2I.Zero);
            }
        }
    }

    private void OnButtonPressed()
    {
        _cursor.Visible = true;
    }
}