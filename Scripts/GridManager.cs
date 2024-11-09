using System.Collections.Generic;
using Godot;

namespace PuzzleGameCourse;

public partial class GridManager : Node
{
    [Export]
    private TileMapLayer _highlightTileMapLayer;

    [Export]
    private TileMapLayer _baseTerrainTileMapLayer;

    private readonly HashSet<Vector2> _occupiedCells = new();

    public override void _Ready()
    {
    }

    public bool IsTilePositionValid(Vector2 tilePosition)
    {
        return !_occupiedCells.Contains(tilePosition);
    }

    public void MarkTileAsOccupied(Vector2 tilePosition)
    {
        _occupiedCells.Add(tilePosition);
    }

    public void HighlightValidTilesInRadius(Vector2 rootCell, int radius)
    {
        ClearHighlightedTiles();

        for (var x = rootCell.X - radius; x <= rootCell.X + radius; x++)
        {
            for (var y = rootCell.Y - radius; y <= rootCell.Y + radius; y++)
            {
                if (!IsTilePositionValid(new Vector2(x, y)))
                    continue;
                
                _highlightTileMapLayer.SetCell(new Vector2I((int)x, (int)y), 0, Vector2I.Zero);
            }
        }
    }

    public void ClearHighlightedTiles()
    {
        _highlightTileMapLayer.Clear();
    }

    public Vector2 GetMouseGridCellPosition()
    {
        var mousePosition = _highlightTileMapLayer.GetGlobalMousePosition();
        var gridPosition = mousePosition / 64;
        gridPosition = gridPosition.Floor();

        return gridPosition;
    }
}