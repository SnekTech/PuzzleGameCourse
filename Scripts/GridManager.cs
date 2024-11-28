using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using PuzzleGameCourse.Autoload;
using PuzzleGameCourse.Component;
using PuzzleGameCourse.Terrain;

namespace PuzzleGameCourse;

public partial class GridManager : Node
{
    private const string IsBuildable = "is_buildable";
    private const string IsWood = "is_wood";
    private const string IsIgnored = "is_ignored";

    [Signal]
    public delegate void ResourceTilesUpdatedEventHandler(int collectedTiles);

    [Signal]
    public delegate void GridStateUpdatedEventHandler();

    [Export] private TileMapLayer _highlightTileMapLayer;
    [Export] private TileMapLayer _baseTerrainTileMapLayer;

    private readonly HashSet<Vector2I> _validBuildableTiles = [];
    private readonly HashSet<Vector2I> _allTilesInBuildingRadius = [];
    private readonly HashSet<Vector2I> _collectedResourceTiles = [];
    private readonly HashSet<Vector2I> _occupiedTiles = [];
    private List<TileMapLayer> _allTileMapLayersDFS;
    private readonly Dictionary<TileMapLayer, ElevationLayer> _tileMapLayerToElevationLayer = new();

    public override void _Ready()
    {
        GameEvents.Instance.Connect(GameEvents.SignalName.BuildingPlaced,
            Callable.From<BuildingComponent>(OnBuildingPlaced));
        GameEvents.Instance.Connect(GameEvents.SignalName.BuildingDestroyed,
            Callable.From<BuildingComponent>(OnBuildingDestroyed));
        _allTileMapLayersDFS = GetAllTileMapLayersDFS(_baseTerrainTileMapLayer);
        MapTileMapLayersToElevationLayers();
    }

    public (TileMapLayer, bool) GetTileCustomData(Vector2I tilePosition, string dataName)
    {
        foreach (var layer in _allTileMapLayersDFS)
        {
            var customData = layer.GetCellTileData(tilePosition);
            if (customData is null || (bool)customData.GetCustomData(IsIgnored))
                continue;

            return (layer, (bool)customData.GetCustomData(dataName));
        }

        return (null, false);
    }

    public bool IsTilePositionBuildable(Vector2I tilePosition)
    {
        return _validBuildableTiles.Contains(tilePosition);
    }

    public bool IsTilePositionInAnyBuildingRadius(Vector2I tilePosition)
    {
        return _allTilesInBuildingRadius.Contains(tilePosition);
    }

    public bool IsTileAreaBuildable(Rect2I tileArea)
    {
        var tiles = tileArea.ToTiles();
        if (tiles.Count == 0) return false;

        var (firstTileMapLayer, _) = GetTileCustomData(tiles[0], IsBuildable);
        var targetElevationLayer = firstTileMapLayer != null ? _tileMapLayerToElevationLayer[firstTileMapLayer] : null;

        return tiles.All(tilePosition =>
        {
            var (tileMapLayer, isBuildable) = GetTileCustomData(tilePosition, IsBuildable);
            var elevationLayer = tileMapLayer != null ? _tileMapLayerToElevationLayer[tileMapLayer] : null;
            return isBuildable && _validBuildableTiles.Contains(tilePosition) && elevationLayer == targetElevationLayer;
        });
    }

    public void HighLightBuildableTiles()
    {
        foreach (var tilePosition in _validBuildableTiles)
        {
            _highlightTileMapLayer.SetCell(tilePosition, 0, Vector2I.Zero);
        }
    }

    public void HighlightExpandedBuildableTiles(Rect2I tileArea, int radius)
    {
        var validTiles = GetValidTilesInRadius(tileArea, radius).ToHashSet();
        var expandedTiles = validTiles.Except(_validBuildableTiles).Except(_occupiedTiles);
        var altasCoords = new Vector2I(1, 0);
        foreach (var tilePosition in expandedTiles)
        {
            _highlightTileMapLayer.SetCell(tilePosition, 0, altasCoords);
        }
    }

    public void HighlightResourceTiles(Rect2I tileArea, int radius)
    {
        var resourceTiles = GetResourceTilesInRadius(tileArea, radius);
        var altasCoords = new Vector2I(1, 0);
        foreach (var tilePosition in resourceTiles)
        {
            _highlightTileMapLayer.SetCell(tilePosition, 0, altasCoords);
        }
    }

    public void ClearHighlightedTiles()
    {
        _highlightTileMapLayer.Clear();
    }

    public Vector2I GetMouseGridCellPosition()
    {
        var mousePosition = _highlightTileMapLayer.GetGlobalMousePosition();
        return ConvertWorldPositionToTilePosition(mousePosition);
    }

    public Vector2I ConvertWorldPositionToTilePosition(Vector2 worldPosition)
    {
        var tilePosition = worldPosition / 64;
        tilePosition = tilePosition.Floor();
        return new Vector2I((int)tilePosition.X, (int)tilePosition.Y);
    }

    private List<TileMapLayer> GetAllTileMapLayersDFS(Node2D rootNode)
    {
        var result = new List<TileMapLayer>();
        var children = rootNode.GetChildren();
        children.Reverse();
        foreach (var child in children)
        {
            if (child is Node2D childNode)
            {
                result.AddRange(GetAllTileMapLayersDFS(childNode));
            }
        }

        if (rootNode is TileMapLayer tileMapLayer)
        {
            result.Add(tileMapLayer);
        }

        return result;
    }

    private void MapTileMapLayersToElevationLayers()
    {
        foreach (var layer in _allTileMapLayersDFS)
        {
            ElevationLayer elevationLayer;
            Node startNode = layer;
            do
            {
                var parent = startNode.GetParent();
                elevationLayer = parent as ElevationLayer;
                startNode = parent;
            } while (elevationLayer == null && startNode != null);

            _tileMapLayerToElevationLayer[layer] = elevationLayer;
        }
    }

    private void UpdateValidBuildableTiles(BuildingComponent buildingComponent)
    {
        _occupiedTiles.UnionWith(buildingComponent.GetOccupiedCellPositions());
        var rootCell = buildingComponent.GetGridCellPosition();
        var tileArea = new Rect2I(rootCell, buildingComponent.BuildingResource.Dimensions);

        var allTiles = GetTilesInRadius(tileArea, buildingComponent.BuildingResource.BuildableRadius, _ => true);
        _allTilesInBuildingRadius.UnionWith(allTiles);

        var validTiles = GetValidTilesInRadius(tileArea, buildingComponent.BuildingResource.BuildableRadius);
        _validBuildableTiles.UnionWith(validTiles);
        _validBuildableTiles.ExceptWith(_occupiedTiles);
        EmitSignal(SignalName.GridStateUpdated);
    }

    private void UpdateCollectedResourceTiles(BuildingComponent buildingComponent)
    {
        var rootCell = buildingComponent.GetGridCellPosition();
        var tileArea = new Rect2I(rootCell, buildingComponent.BuildingResource.Dimensions);
        var resourceTiles = GetResourceTilesInRadius(tileArea, buildingComponent.BuildingResource.ResourceRadius);
        var oldResourceTileCount = _collectedResourceTiles.Count;
        _collectedResourceTiles.UnionWith(resourceTiles);
        if (oldResourceTileCount != _collectedResourceTiles.Count)
        {
            EmitSignal(SignalName.ResourceTilesUpdated, _collectedResourceTiles.Count);
        }

        EmitSignal(SignalName.GridStateUpdated);
    }

    private void RecalculateGrid(BuildingComponent excludeBuildingComponent)
    {
        _occupiedTiles.Clear();
        _validBuildableTiles.Clear();
        _allTilesInBuildingRadius.Clear();
        _collectedResourceTiles.Clear();

        var buildingComponents = GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>()
            .Where(buildingComponent => buildingComponent != excludeBuildingComponent);

        foreach (var buildingComponent in buildingComponents)
        {
            UpdateValidBuildableTiles(buildingComponent);
            UpdateCollectedResourceTiles(buildingComponent);
        }

        EmitSignal(SignalName.ResourceTilesUpdated, _collectedResourceTiles.Count);
        EmitSignal(SignalName.GridStateUpdated);
    }

    private bool IsTileInsideCircle(Vector2 centerPosition, Vector2 tilePosition, float radius)
    {
        var dx = centerPosition.X - (tilePosition.X + 0.5f);
        var dy = centerPosition.Y - (tilePosition.Y + 0.5f);
        var distanceSquared = dx * dx + dy * dy;
        return distanceSquared <= radius * radius;
    }

    private List<Vector2I> GetTilesInRadius(Rect2I tileArea, int radius, Func<Vector2I, bool> filterFn)
    {
        var result = new List<Vector2I>();
        var tileAreaF = tileArea.ToRect2F();
        var tileAreaCenter = tileAreaF.GetCenter();
        var radiusMod = Mathf.Max(tileAreaF.Size.X, tileAreaF.Size.Y) / 2;

        for (var x = tileArea.Position.X - radius; x < tileArea.End.X + radius; x++)
        {
            for (var y = tileArea.Position.Y - radius; y < tileArea.End.Y + radius; y++)
            {
                var tilePosition = new Vector2I(x, y);
                if (!IsTileInsideCircle(tileAreaCenter, tilePosition, radius + radiusMod) || !filterFn(tilePosition))
                    continue;

                result.Add(tilePosition);
            }
        }

        return result;
    }

    private List<Vector2I> GetValidTilesInRadius(Rect2I tileArea, int radius)
    {
        return GetTilesInRadius(tileArea, radius, tilePosition => GetTileCustomData(tilePosition, IsBuildable).Item2);
    }

    private List<Vector2I> GetResourceTilesInRadius(Rect2I tileArea, int radius)
    {
        return GetTilesInRadius(tileArea, radius, tilePosition => GetTileCustomData(tilePosition, IsWood).Item2);
    }

    private void OnBuildingPlaced(BuildingComponent buildingComponent)
    {
        UpdateValidBuildableTiles(buildingComponent);
        UpdateCollectedResourceTiles(buildingComponent);
    }

    private void OnBuildingDestroyed(BuildingComponent buildingComponent)
    {
        RecalculateGrid(buildingComponent);
    }
}