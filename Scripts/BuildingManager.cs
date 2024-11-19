using System.Linq;
using Godot;
using PuzzleGameCourse.Building;
using PuzzleGameCourse.Component;
using PuzzleGameCourse.UI;

namespace PuzzleGameCourse;

public partial class BuildingManager : Node
{
    private static readonly StringName ActionLeftClick = "left_click";
    private static readonly StringName ActionRightClick = "right_click";
    private static readonly StringName ActionCancel = "cancel";

    [Export] private int startingResourceCount = 4;
    [Export] private GridManager gridManager;
    [Export] private GameUI gameUI;
    [Export] private Node2D ySortRoot;
    [Export] private PackedScene buildingGhostScene;

    private enum State
    {
        Normal,
        PlacingBuilding
    }

    private int _currentResourceCount;
    private int _currentlyUsedResourceCount;
    private BuildingResource _toPlaceBuildingResource;
    private Vector2I _hoveredGridCell;
    private BuildingGhost _buildingGhost;
    private State _currentState = State.Normal;

    private int AvailableResourceCount => startingResourceCount + _currentResourceCount - _currentlyUsedResourceCount;

    public override void _Ready()
    {
        gridManager.ResourceTilesUpdated += OnResourceTilesUpdated;
        gameUI.BuildingResourceSelected += OnBuildingResourceSelected;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        switch (_currentState)
        {
            case State.Normal:
                if (@event.IsActionPressed(ActionRightClick))
                {
                    DestroyBuildingAtHoveredCellPosition();
                }

                break;
            case State.PlacingBuilding:
                if (@event.IsActionPressed(ActionCancel))
                {
                    ChangeState(State.Normal);
                }
                else if (
                    _toPlaceBuildingResource != null &&
                    @event.IsActionPressed(ActionLeftClick) &&
                    IsBuildingPlaceableAtTile(_hoveredGridCell)
                )
                {
                    PlaceBuildingAtHoveredCellPosition();
                }

                break;
        }
    }

    public override void _Process(double delta)
    {
        var gridPosition = gridManager.GetMouseGridCellPosition();
        if (gridPosition != _hoveredGridCell)
        {
            _hoveredGridCell = gridPosition;
            UpdateHoveredGridCell();
        }

        switch (_currentState)
        {
            case State.Normal:
                break;
            case State.PlacingBuilding:
                _buildingGhost.GlobalPosition = gridPosition * 64;
                break;
        }
    }

    private void UpdateGridDisplay()
    {
        gridManager.ClearHighlightedTiles();
        gridManager.HighLightBuildableTiles();
        if (IsBuildingPlaceableAtTile(_hoveredGridCell))
        {
            gridManager.HighlightExpandedBuildableTiles(_hoveredGridCell,
                _toPlaceBuildingResource.BuildableRadius);
            gridManager.HighlightResourceTiles(_hoveredGridCell, _toPlaceBuildingResource.ResourceRadius);
            _buildingGhost.SetValid();
        }
        else
        {
            _buildingGhost.SetInvalid();
        }
    }

    private void PlaceBuildingAtHoveredCellPosition()
    {
        var building = _toPlaceBuildingResource.BuildingScene.Instantiate<Node2D>();
        ySortRoot.AddChild(building);

        building.GlobalPosition = _hoveredGridCell * 64;

        _currentlyUsedResourceCount += _toPlaceBuildingResource.ResourceCost;

        ChangeState(State.Normal);
    }

    private void DestroyBuildingAtHoveredCellPosition()
    {
        var buildingComponent = GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>()
            .FirstOrDefault(buildingComponent => buildingComponent.GetGridCellPosition() == _hoveredGridCell);
        if (buildingComponent == null)
            return;

        _currentlyUsedResourceCount -= buildingComponent.BuildingResource.ResourceCost;
        buildingComponent.Destroy();
        GD.Print(AvailableResourceCount);
    }

    private void ClearBuildingGhost()
    {
        gridManager.ClearHighlightedTiles();

        if (IsInstanceValid(_buildingGhost))
        {
            _buildingGhost.QueueFree();
        }

        _buildingGhost = null;
    }

    private bool IsBuildingPlaceableAtTile(Vector2I tilePosition)
    {
        return gridManager.IsTilePositionBuildable(tilePosition) &&
               AvailableResourceCount >= _toPlaceBuildingResource.ResourceCost;
    }

    private void UpdateHoveredGridCell()
    {
        switch (_currentState)
        {
            case State.Normal:
                break;
            case State.PlacingBuilding:
                UpdateGridDisplay();
                break;
        }
    }

    private void ChangeState(State toState)
    {
        switch (_currentState)
        {
            case State.Normal:
                break;
            case State.PlacingBuilding:
                ClearBuildingGhost();
                _toPlaceBuildingResource = null;
                break;
        }

        _currentState = toState;

        switch (_currentState)
        {
            case State.Normal:
                break;
            case State.PlacingBuilding:
                _buildingGhost = buildingGhostScene.Instantiate<BuildingGhost>();
                ySortRoot.AddChild(_buildingGhost);
                break;
        }
    }

    private void OnResourceTilesUpdated(int resourceCount)
    {
        _currentResourceCount = resourceCount;
    }

    private void OnBuildingResourceSelected(BuildingResource buildingResource)
    {
        ChangeState(State.PlacingBuilding);
        var buildingSprite = buildingResource.SpriteScene.Instantiate<Sprite2D>();
        _buildingGhost.AddChild(buildingSprite);
        _toPlaceBuildingResource = buildingResource;
        UpdateGridDisplay();
    }
}