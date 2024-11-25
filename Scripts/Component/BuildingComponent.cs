using System.Collections.Generic;
using System.Linq;
using Godot;
using PuzzleGameCourse.Autoload;
using PuzzleGameCourse.Building;

namespace PuzzleGameCourse.Component;

public partial class BuildingComponent : Node2D
{
    [Export(PropertyHint.File, "*.tres")]
    public string BuildingResourcePath { get; private set; }

    public BuildingResource BuildingResource { get; private set; }

    private HashSet<Vector2I> _occupiedTiles = [];

    public override void _Ready()
    {
        if (BuildingResourcePath != null)
        {
            BuildingResource = GD.Load<BuildingResource>(BuildingResourcePath);
        }

        AddToGroup(nameof(BuildingComponent));
        Callable.From(Initialize).CallDeferred();
    }

    public Vector2I GetGridCellPosition()
    {
        var gridPosition = GlobalPosition / 64;
        gridPosition = gridPosition.Floor();

        return new Vector2I((int)gridPosition.X, (int)gridPosition.Y);
    }

    private void CalculateOccupiedCellPositions()
    {
        var gridPosition = GetGridCellPosition();
        for (var x = gridPosition.X; x < gridPosition.X + BuildingResource.Dimensions.X; x++)
        {
            for (var y = gridPosition.Y; y < gridPosition.Y + BuildingResource.Dimensions.Y; y++)
            {
                _occupiedTiles.Add(new Vector2I(x, y));
            }
        }
    }

    public HashSet<Vector2I> GetOccupiedCellPositions()
    {
        return _occupiedTiles.ToHashSet();
    }

    public bool IsTileInBuildingArea(Vector2I tilePosition)
    {
        return _occupiedTiles.Contains(tilePosition);
    }

    public void Destroy()
    {
        GameEvents.EmitBuildingDestroyed(this);
        Owner.QueueFree();
    }

    private void Initialize()
    {
        CalculateOccupiedCellPositions();
        GameEvents.EmitBuildingPlaced(this);
    }
}