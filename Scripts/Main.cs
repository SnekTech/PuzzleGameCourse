using Godot;
using PuzzleGameCourse.Building;
using PuzzleGameCourse.UI;

namespace PuzzleGameCourse;

public partial class Main : Node
{
    private GridManager _gridManager;
    private Sprite2D _cursor;
    private Node2D _ySortRoot;
    private GameUI _gameUI;

    private Vector2I? _hoveredGridCell;
    private BuildingResource _toPlaceBuildingResource;

    public override void _Ready()
    {
        _gridManager = GetNode<GridManager>("GridManager");
        _cursor = GetNode<Sprite2D>("Cursor");
        _ySortRoot = GetNode<Node2D>("YSortRoot");
        _gameUI = GetNode<GameUI>("GameUI");

        _cursor.Visible = false;

        _gameUI.BuildingResourceSelected += OnBuildingResourceSelected;
        _gridManager.ResourceTilesUpdated += OnResourceTilesUpdated;
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

        if (_toPlaceBuildingResource != null && _cursor.Visible &&
            (!_hoveredGridCell.HasValue || gridPosition != _hoveredGridCell.Value))
        {
            _hoveredGridCell = gridPosition;
            _gridManager.ClearHighlightedTiles();
            _gridManager.HighlightExpandedBuildableTiles(_hoveredGridCell.Value,
                _toPlaceBuildingResource.BuildableRadius);
            _gridManager.HighlightResourceTiles(_hoveredGridCell.Value, _toPlaceBuildingResource.ResourceRadius);
        }
    }

    private void PlaceBuildingAtHoveredCellPosition()
    {
        if (!_hoveredGridCell.HasValue)
            return;

        var building = _toPlaceBuildingResource.BuildingScene.Instantiate<Node2D>();
        _ySortRoot.AddChild(building);

        building.GlobalPosition = _hoveredGridCell.Value * 64;

        _hoveredGridCell = null;
        _gridManager.ClearHighlightedTiles();
    }

    private void OnBuildingResourceSelected(BuildingResource buildingResource)
    {
        _toPlaceBuildingResource = buildingResource;
        _cursor.Visible = true;
        _gridManager.HighLightBuildableTiles();
    }

    private void OnResourceTilesUpdated(int resourceCount)
    {
        GD.Print(resourceCount);
    }
}