using System.Collections.Generic;
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

    private readonly HashSet<Vector2I> _validBuildableTiles = new();

    public override void _Ready()
    {
        GameEvents.Instance.BuildingPlaced += OnBuildingPlaced;
    }

    public bool IsTilePositionValid(Vector2I tilePosition)
    {
        var customData = _baseTerrainTileMapLayer.GetCellTileData(tilePosition);
        if (customData is null)
            return false; // empty tile, invalid
        return (bool)customData.GetCustomData("buildable");
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

    private void UpdateValidBuildableTiles(BuildingComponent buildingComponent)
    {
        var rootCell = buildingComponent.GetGridCellPosition();
        var radius = buildingComponent.BuildableRadius;
        for (var x = rootCell.X - radius; x <= rootCell.X + radius; x++)
        {
            for (var y = rootCell.Y - radius; y <= rootCell.Y + radius; y++)
            {
                var tilePosition = new Vector2I(x, y);
                if (!IsTilePositionValid(tilePosition))
                    continue;

                _validBuildableTiles.Add(tilePosition);
            }
        }
        _validBuildableTiles.Remove(rootCell);
    }

    private void OnBuildingPlaced(BuildingComponent buildingComponent)
    {
        UpdateValidBuildableTiles(buildingComponent);
    }
}