using Godot;

namespace PuzzleGameCourse;

public partial class Main : Node
{
    private GridManager _gridManager;
    private Sprite2D _cursor;
    private PackedScene _towerScene;
    private PackedScene _villageScene;
    private Button _placeTowerButton;
    private Button _placeVillageButton;
    private Node2D _ySortRoot;

    private Vector2I? _hoveredGridCell;
    private PackedScene _toPlaceBuildingScene;

    public override void _Ready()
    {
        _towerScene = GD.Load<PackedScene>("res://scenes/building/Tower.tscn");
        _villageScene = GD.Load<PackedScene>("res://scenes/building/Village.tscn");
        _gridManager = GetNode<GridManager>("GridManager");
        _cursor = GetNode<Sprite2D>("Cursor");
        _placeTowerButton = GetNode<Button>("PlaceTowerButton");
        _placeVillageButton = GetNode<Button>("PlaceVillageButton");
        _ySortRoot = GetNode<Node2D>("YSortRoot");

        _cursor.Visible = false;

        _placeTowerButton.Pressed += OnPlaceTowerButtonPressed;
        _placeVillageButton.Pressed += OnPlaceVillageButtonPressed;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_hoveredGridCell.HasValue && @event.IsActionPressed("left_click") &&
            _gridManager.IsTilePositionBuildable(_hoveredGridCell.Value))
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
            _gridManager.HighlightExpandedBuildableTiles(_hoveredGridCell.Value, 3);
        }
    }

    private void PlaceBuildingAtHoveredCellPosition()
    {
        if (!_hoveredGridCell.HasValue)
            return;

        var building = _toPlaceBuildingScene.Instantiate<Node2D>();
        _ySortRoot.AddChild(building);

        building.GlobalPosition = _hoveredGridCell.Value * 64;

        _hoveredGridCell = null;
        _gridManager.ClearHighlightedTiles();
    }

    private void OnPlaceTowerButtonPressed()
    {
        _toPlaceBuildingScene = _towerScene;
        _cursor.Visible = true;
        _gridManager.HighLightBuildableTiles();
    }
    
    private void OnPlaceVillageButtonPressed()
    {
        _toPlaceBuildingScene = _villageScene;
        _cursor.Visible = true;
        _gridManager.HighLightBuildableTiles();
    }
}