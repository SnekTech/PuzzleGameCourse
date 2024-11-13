using Godot;
using PuzzleGameCourse.Building;
using PuzzleGameCourse.UI;

namespace PuzzleGameCourse;

public partial class BuildingManager : Node
{
    [Export]
    private GridManager gridManager;

    [Export]
    private GameUI gameUI;

    [Export]
    private Node2D ySortRoot;

    [Export]
    private PackedScene buildingGhostScene;

    private int _currentResourceCount;
    private int _startingResourceCount = 4;
    private int _currentlyUsedResourceCount;
    private BuildingResource _toPlaceBuildingResource;
    private Vector2I? _hoveredGridCell;
    private BuildingGhost _buildingGhost;

    private int AvailableResourceCount => _startingResourceCount + _currentResourceCount - _currentlyUsedResourceCount;

    public override void _Ready()
    {
        gridManager.ResourceTilesUpdated += OnResourceTilesUpdated;
        gameUI.BuildingResourceSelected += OnBuildingResourceSelected;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (
            _hoveredGridCell.HasValue &&
            _toPlaceBuildingResource != null &&
            @event.IsActionPressed("left_click") &&
            IsBuildingPlaceableAtTile(_hoveredGridCell.Value)
        )
        {
            PlaceBuildingAtHoveredCellPosition();
        }
    }

    public override void _Process(double delta)
    {
        if (!IsInstanceValid(_buildingGhost))
            return;

        var gridPosition = gridManager.GetMouseGridCellPosition();
        _buildingGhost.GlobalPosition = gridPosition * 64;

        if (_toPlaceBuildingResource != null &&
            (!_hoveredGridCell.HasValue || gridPosition != _hoveredGridCell.Value))
        {
            _hoveredGridCell = gridPosition;
            UpdateGridDisplay();
        }
    }

    private void UpdateGridDisplay()
    {
        if (_hoveredGridCell == null)
            return;

        gridManager.ClearHighlightedTiles();
        gridManager.HighLightBuildableTiles();
        if (IsBuildingPlaceableAtTile(_hoveredGridCell.Value))
        {
            gridManager.HighlightExpandedBuildableTiles(_hoveredGridCell.Value,
                _toPlaceBuildingResource.BuildableRadius);
            gridManager.HighlightResourceTiles(_hoveredGridCell.Value, _toPlaceBuildingResource.ResourceRadius);
            _buildingGhost.SetValid();
        }
        else
        {
            _buildingGhost.SetInvalid();
        }
    }

    private void PlaceBuildingAtHoveredCellPosition()
    {
        if (!_hoveredGridCell.HasValue)
            return;

        var building = _toPlaceBuildingResource.BuildingScene.Instantiate<Node2D>();
        ySortRoot.AddChild(building);

        building.GlobalPosition = _hoveredGridCell.Value * 64;

        _hoveredGridCell = null;
        gridManager.ClearHighlightedTiles();

        _currentlyUsedResourceCount += _toPlaceBuildingResource.ResourceCost;
        _buildingGhost.QueueFree();
        _buildingGhost = null;
    }

    private bool IsBuildingPlaceableAtTile(Vector2I tilePosition)
    {
        return gridManager.IsTilePositionBuildable(tilePosition) &&
               AvailableResourceCount >= _toPlaceBuildingResource.ResourceCost;
    }

    private void OnResourceTilesUpdated(int resourceCount)
    {
        _currentResourceCount = resourceCount;
    }

    private void OnBuildingResourceSelected(BuildingResource buildingResource)
    {
        if (IsInstanceValid(_buildingGhost))
        {
            _buildingGhost.QueueFree();
        }

        _buildingGhost = buildingGhostScene.Instantiate<BuildingGhost>();
        ySortRoot.AddChild(_buildingGhost);

        var buildingSprite = buildingResource.SpriteScene.Instantiate<Sprite2D>();
        _buildingGhost.AddChild(buildingSprite);

        _toPlaceBuildingResource = buildingResource;
        UpdateGridDisplay();
    }
}