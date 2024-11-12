using System.Collections.Generic;
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

    private readonly HashSet<Vector2I> _validBuildableTiles = new();
    private List<TileMapLayer> _allTileMapLayersDFS;

    public override void _Ready()
    {
        GameEvents.Instance.BuildingPlaced += OnBuildingPlaced;
        _allTileMapLayersDFS = GetAllTileMapLayersDFS(_baseTerrainTileMapLayer);
    }

    public bool IsTilePositionValid(Vector2I tilePosition)
    {
        foreach (var tileMapLayer in _allTileMapLayersDFS)
        {
            var customData = tileMapLayer.GetCellTileData(tilePosition);
            if (customData is null)
                continue;
            
            return (bool)customData.GetCustomData("buildable");
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
        ClearHighlightedTiles();
        HighLightBuildableTiles();

        var validTiles = GetValidTilesInRadius(rootCell, radius).ToHashSet();
        var expandedTiles = validTiles.Except(_validBuildableTiles).Except(GetOccupiedTiles());
        var altasCoords = new Vector2I(1, 0);
        foreach (var tilePosition in expandedTiles)
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
        var gridPosition = mousePosition / 64;
        gridPosition = gridPosition.Floor();

        return new Vector2I((int)gridPosition.X, (int)gridPosition.Y);
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
        var validTiles = GetValidTilesInRadius(rootCell, buildingComponent.BuildingResource.BuildableRadius);
        _validBuildableTiles.UnionWith(validTiles);

        _validBuildableTiles.ExceptWith(GetOccupiedTiles());
    }

    private List<Vector2I> GetValidTilesInRadius(Vector2I rootCell, int radius)
    {
        var result = new List<Vector2I>();
        for (var x = rootCell.X - radius; x <= rootCell.X + radius; x++)
        {
            for (var y = rootCell.Y - radius; y <= rootCell.Y + radius; y++)
            {
                var tilePosition = new Vector2I(x, y);
                if (!IsTilePositionValid(tilePosition))
                    continue;

                result.Add(tilePosition);
            }
        }

        return result;
    }

    private IEnumerable<Vector2I> GetOccupiedTiles()
    {
        var buildingComponents = GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>();
        var occupiedTiles = buildingComponents.Select(x => x.GetGridCellPosition());
        return occupiedTiles;
    }

    private void OnBuildingPlaced(BuildingComponent buildingComponent)
    {
        UpdateValidBuildableTiles(buildingComponent);
    }
}