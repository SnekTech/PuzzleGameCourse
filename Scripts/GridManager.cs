﻿using System.Collections.Generic;
using System.Linq;
using Godot;
using PuzzleGameCourse.Autoload;
using PuzzleGameCourse.Component;

namespace PuzzleGameCourse;

public partial class GridManager : Node
{
    [Export]
    private TileMapLayer _highlightTileMapLayer;

    [Export]
    private TileMapLayer _baseTerrainTileMapLayer;

    private readonly HashSet<Vector2I> _occupiedCells = new();

    public override void _Ready()
    {
        GameEvents.Instance.BuildingPlaced += OnBuildingPlaced;
    }

    public bool IsTilePositionValid(Vector2I tilePosition)
    {
        var customData = _baseTerrainTileMapLayer.GetCellTileData(tilePosition);
        if (customData is null)
            return false; // empty tile, invalid
        if (!(bool)customData.GetCustomData("buildable"))
            return false;

        return !_occupiedCells.Contains(tilePosition);
    }

    public void MarkTileAsOccupied(Vector2I tilePosition)
    {
        _occupiedCells.Add(tilePosition);
    }

    public void HighLightBuildableTiles()
    {
        ClearHighlightedTiles();

        var buildingComponents = GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>();
        foreach (var buildingComponent in buildingComponents)
        {
            HighlightValidTilesInRadius(buildingComponent.GetGridCellPosition(), buildingComponent.BuildableRadius);
        }
    }

    public void ClearHighlightedTiles()
    {
        _highlightTileMapLayer.Clear();
    }

    public Vector2I GetMouseGridCellPosition()
    {
        var mousePosition = _highlightTileMapLayer.GetGlobalMousePosition();
        var gridPosition = mousePosition / 64;
        gridPosition = gridPosition.Floor();

        return new Vector2I((int)gridPosition.X, (int)gridPosition.Y);
    }

    private void HighlightValidTilesInRadius(Vector2I rootCell, int radius)
    {
        for (var x = rootCell.X - radius; x <= rootCell.X + radius; x++)
        {
            for (var y = rootCell.Y - radius; y <= rootCell.Y + radius; y++)
            {
                var tilePosition = new Vector2I(x, y);
                if (!IsTilePositionValid(tilePosition))
                    continue;

                _highlightTileMapLayer.SetCell(tilePosition, 0, Vector2I.Zero);
            }
        }
    }

    private void OnBuildingPlaced(BuildingComponent buildingComponent)
    {
        MarkTileAsOccupied(buildingComponent.GetGridCellPosition());
    }
}