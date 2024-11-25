using System.Collections.Generic;
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
    private Rect2I _hoveredGridArea = new(Vector2I.Zero, Vector2I.One);
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
                    IsBuildingPlaceableAtArea(_hoveredGridArea)
                )
                {
                    PlaceBuildingAtHoveredCellPosition();
                }

                break;
        }
    }

    public override void _Process(double delta)
    {
        var mouseGridPosition = gridManager.GetMouseGridCellPosition();
        var rootCell = _hoveredGridArea.Position;
        if (mouseGridPosition != rootCell)
        {
            _hoveredGridArea.Position = mouseGridPosition;
            UpdateHoveredGridArea();
        }

        switch (_currentState)
        {
            case State.Normal:
                break;
            case State.PlacingBuilding:
                _buildingGhost.GlobalPosition = mouseGridPosition * 64;
                break;
        }
    }

    private void UpdateGridDisplay()
    {
        gridManager.ClearHighlightedTiles();
        gridManager.HighLightBuildableTiles();
        if (IsBuildingPlaceableAtArea(_hoveredGridArea))
        {
            gridManager.HighlightExpandedBuildableTiles(_hoveredGridArea,
                _toPlaceBuildingResource.BuildableRadius);
            gridManager.HighlightResourceTiles(_hoveredGridArea, _toPlaceBuildingResource.ResourceRadius);
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

        building.GlobalPosition = _hoveredGridArea.Position * 64;

        _currentlyUsedResourceCount += _toPlaceBuildingResource.ResourceCost;

        ChangeState(State.Normal);
    }

    private void DestroyBuildingAtHoveredCellPosition()
    {
        var rootCell = _hoveredGridArea.Position;
        var buildingComponent = GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>()
            .FirstOrDefault(buildingComponent => buildingComponent.BuildingResource.IsDeletable &&
                                                 buildingComponent.GetGridCellPosition() == rootCell);
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

    private bool IsBuildingPlaceableAtArea(Rect2I tileArea)
    {
        var tilesInArea = GetTilePositionsInTileArea(tileArea);
        var allTilesBuildable = tilesInArea.All(tilePosition => gridManager.IsTilePositionBuildable(tilePosition));
        return allTilesBuildable && AvailableResourceCount >= _toPlaceBuildingResource.ResourceCost;
    }

    private List<Vector2I> GetTilePositionsInTileArea(Rect2I tileArea)
    {
        var result = new List<Vector2I>();
        for (var x = tileArea.Position.X; x < tileArea.End.X; x++)
        {
            for (var y = tileArea.Position.Y; y < tileArea.End.Y; y++)
            {
                result.Add(new Vector2I(x, y));
            }
        }

        return result;
    }

    private void UpdateHoveredGridArea()
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
        _hoveredGridArea.Size = buildingResource.Dimensions;
        var buildingSprite = buildingResource.SpriteScene.Instantiate<Sprite2D>();
        _buildingGhost.AddChild(buildingSprite);
        _toPlaceBuildingResource = buildingResource;
        UpdateGridDisplay();
    }
}