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

    [Signal]
    public delegate void AvailableResourceCountChangedEventHandler(int availableResourceCount);

    [Export] private GridManager gridManager;
    [Export] private GameUI gameUI;
    [Export] private Node2D ySortRoot;
    [Export] private PackedScene buildingGhostScene;

    private enum State
    {
        Normal,
        PlacingBuilding
    }

    private int _startingResourceCount;
    private int _currentResourceCount;
    private int _currentlyUsedResourceCount;
    private BuildingResource _toPlaceBuildingResource;
    private Rect2I _hoveredGridArea = new(Vector2I.Zero, Vector2I.One);
    private BuildingGhost _buildingGhost;
    private Vector2 _buildingGhostDimensions;
    private State _currentState = State.Normal;

    private int AvailableResourceCount => _startingResourceCount + _currentResourceCount - _currentlyUsedResourceCount;

    public override void _Ready()
    {
        gridManager.ResourceTilesUpdated += OnResourceTilesUpdated;
        gameUI.BuildingResourceSelected += OnBuildingResourceSelected;

        Callable.From(() => EmitSignal(SignalName.AvailableResourceCountChanged, AvailableResourceCount))
            .CallDeferred();
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
        var mouseGridPosition = Vector2I.Zero;

        switch (_currentState)
        {
            case State.Normal:
                mouseGridPosition = gridManager.GetMouseGridCellPosition();
                break;
            case State.PlacingBuilding:
                mouseGridPosition = gridManager.GetMouseGridCellPositionWithDimensionsOffset(_buildingGhostDimensions);
                _buildingGhost.GlobalPosition = mouseGridPosition * 64;
                break;
        }

        var rootCell = _hoveredGridArea.Position;
        if (mouseGridPosition != rootCell)
        {
            _hoveredGridArea.Position = mouseGridPosition;
            UpdateHoveredGridArea();
        }
    }

    public void SetStartingResourceCount(int count)
    {
        _startingResourceCount = count;
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

        _buildingGhost.DoHoverAnimation();
    }

    private void PlaceBuildingAtHoveredCellPosition()
    {
        var building = _toPlaceBuildingResource.BuildingScene.Instantiate<Node2D>();
        ySortRoot.AddChild(building);

        building.GlobalPosition = _hoveredGridArea.Position * 64;

        _currentlyUsedResourceCount += _toPlaceBuildingResource.ResourceCost;

        ChangeState(State.Normal);
        EmitSignal(SignalName.AvailableResourceCountChanged, AvailableResourceCount);
    }

    private void DestroyBuildingAtHoveredCellPosition()
    {
        var rootCell = _hoveredGridArea.Position;
        var buildingComponent = GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>()
            .FirstOrDefault(buildingComponent => buildingComponent.BuildingResource.IsDeletable &&
                                                 buildingComponent.IsTileInBuildingArea(rootCell));
        if (buildingComponent == null)
            return;

        _currentlyUsedResourceCount -= buildingComponent.BuildingResource.ResourceCost;
        buildingComponent.Destroy();
        EmitSignal(SignalName.AvailableResourceCountChanged, AvailableResourceCount);
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
        var allTilesBuildable = gridManager.IsTileAreaBuildable(tileArea);
        return allTilesBuildable && AvailableResourceCount >= _toPlaceBuildingResource.ResourceCost;
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
        EmitSignal(SignalName.AvailableResourceCountChanged, AvailableResourceCount);
    }

    private void OnBuildingResourceSelected(BuildingResource buildingResource)
    {
        ChangeState(State.PlacingBuilding);
        _hoveredGridArea.Size = buildingResource.Dimensions;
        var buildingSprite = buildingResource.SpriteScene.Instantiate<Sprite2D>();
        _buildingGhost.AddSpriteNode(buildingSprite);
        _buildingGhost.SetDimensions(buildingResource.Dimensions);
        _buildingGhostDimensions = buildingResource.Dimensions;
        _toPlaceBuildingResource = buildingResource;
        UpdateGridDisplay();
    }
}