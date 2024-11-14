﻿using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using PuzzleGameCourse.Autoload;
using PuzzleGameCourse.Component;

namespace PuzzleGameCourse;

public partial class GridManager : Node
{
    private const string IsBuildable = "is_buildable";
    private const string IsWood = "is_wood";

    [Signal]
    public delegate void ResourceTilesUpdatedEventHandler(int collectedTiles);

    [Signal]
    public delegate void GridStateUpdatedEventHandler();

    [Export] private TileMapLayer _highlightTileMapLayer;
    [Export] private TileMapLayer _baseTerrainTileMapLayer;

    private readonly HashSet<Vector2I> _validBuildableTiles = new();
    private readonly HashSet<Vector2I> _collectedResourceTiles = new();
    private readonly HashSet<Vector2I> _occupiedTiles = new();
    private List<TileMapLayer> _allTileMapLayersDFS;

    public override void _Ready()
    {
        GameEvents.Instance.BuildingPlaced += OnBuildingPlaced;
        GameEvents.Instance.BuildingDestroyed += OnBuildingDestroyed;
        _allTileMapLayersDFS = GetAllTileMapLayersDFS(_baseTerrainTileMapLayer);
    }

    public bool TileHasCustomData(Vector2I tilePosition, string dataName)
    {
        foreach (var tileMapLayer in _allTileMapLayersDFS)
        {
            var customData = tileMapLayer.GetCellTileData(tilePosition);
            if (customData is null)
                continue;

            return (bool)customData.GetCustomData(dataName);
        }

        return false;
    }

    public bool IsTilePositionBuildable(Vector2I tilePosition)
    {
        return _validBuildableTiles.Contains(tilePosition);
    }

    public void HighLightBuildableTiles()
    {
        foreach (var tilePosition in _validBuildableTiles)
        {
            _highlightTileMapLayer.SetCell(tilePosition, 0, Vector2I.Zero);
        }
    }

    public void HighlightExpandedBuildableTiles(Vector2I rootCell, int radius)
    {
        var validTiles = GetValidTilesInRadius(rootCell, radius).ToHashSet();
        var expandedTiles = validTiles.Except(_validBuildableTiles).Except(_occupiedTiles);
        var altasCoords = new Vector2I(1, 0);
        foreach (var tilePosition in expandedTiles)
        {
            _highlightTileMapLayer.SetCell(tilePosition, 0, altasCoords);
        }
    }

    public void HighlightResourceTiles(Vector2I rootCell, int radius)
    {
        var resourceTiles = GetResourceTilesInRadius(rootCell, radius);
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

    private List<TileMapLayer> GetAllTileMapLayersDFS(TileMapLayer rootTileMapLayer)
    {
        var result = new List<TileMapLayer>();
        var children = rootTileMapLayer.GetChildren();
        children.Reverse();
        foreach (var child in children)
        {
            if (child is TileMapLayer childLayer)
            {
                result.AddRange(GetAllTileMapLayersDFS(childLayer));
            }
        }

        result.Add(rootTileMapLayer);
        return result;
    }

    private void UpdateValidBuildableTiles(BuildingComponent buildingComponent)
    {
        var rootCell = buildingComponent.GetGridCellPosition();
        _occupiedTiles.Add(rootCell);
        var validTiles = GetValidTilesInRadius(rootCell, buildingComponent.BuildingResource.BuildableRadius);
        _validBuildableTiles.UnionWith(validTiles);
        _validBuildableTiles.ExceptWith(_occupiedTiles);
        EmitSignal(SignalName.GridStateUpdated);
    }

    private void UpdateCollectedResourceTiles(BuildingComponent buildingComponent)
    {
        var rootCell = buildingComponent.GetGridCellPosition();
        var resourceTiles = GetResourceTilesInRadius(rootCell, buildingComponent.BuildingResource.ResourceRadius);
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

    private List<Vector2I> GetTilesInRadius(Vector2I rootCell, int radius, Func<Vector2I, bool> filterFn)
    {
        var result = new List<Vector2I>();
        for (var x = rootCell.X - radius; x <= rootCell.X + radius; x++)
        {
            for (var y = rootCell.Y - radius; y <= rootCell.Y + radius; y++)
            {
                var tilePosition = new Vector2I(x, y);
                if (!filterFn(tilePosition))
                    continue;

                result.Add(tilePosition);
            }
        }

        return result;
    }

    private List<Vector2I> GetValidTilesInRadius(Vector2I rootCell, int radius)
    {
        return GetTilesInRadius(rootCell, radius, tilePosition => TileHasCustomData(tilePosition, IsBuildable));
    }

    private List<Vector2I> GetResourceTilesInRadius(Vector2I rootCell, int radius)
    {
        return GetTilesInRadius(rootCell, radius, tilePosition => TileHasCustomData(tilePosition, IsWood));
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