using System.Collections.Generic;
using Godot;
using PuzzleGameCourse.Autoload;
using PuzzleGameCourse.Building;

namespace PuzzleGameCourse.Component;

public partial class BuildingComponent : Node2D
{
    [Export(PropertyHint.File, "*.tres")]
    public string BuildingResourcePath { get; private set; }

    public BuildingResource BuildingResource { get; private set; }

    public override void _Ready()
    {
        if (BuildingResourcePath != null)
        {
            BuildingResource = GD.Load<BuildingResource>(BuildingResourcePath);
        }

        AddToGroup(nameof(BuildingComponent));
        Callable.From(() => GameEvents.EmitBuildingPlaced(this)).CallDeferred();
    }

    public Vector2I GetGridCellPosition()
    {
        var gridPosition = GlobalPosition / 64;
        gridPosition = gridPosition.Floor();

        return new Vector2I((int)gridPosition.X, (int)gridPosition.Y);
    }

    public List<Vector2I> GetOccupiedCellPositions()
    {
        var result = new List<Vector2I>();
        var gridPosition = GetGridCellPosition();
        for (var x = gridPosition.X; x < gridPosition.X + BuildingResource.Dimensions.X; x++)
        {
            for (var y = gridPosition.Y; y < gridPosition.Y + BuildingResource.Dimensions.Y; y++)
            {
                result.Add(new Vector2I(x, y));
            }
        }

        return result;
    }

    public void Destroy()
    {
        GameEvents.EmitBuildingDestroyed(this);
        Owner.QueueFree();
    }
}