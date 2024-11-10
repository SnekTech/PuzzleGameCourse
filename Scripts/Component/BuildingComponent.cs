﻿using Godot;
using PuzzleGameCourse.Autoload;

namespace PuzzleGameCourse.Component;

public partial class BuildingComponent : Node2D
{
    [Export]
    public int BuildableRadius { get; private set; }

    public override void _Ready()
    {
        AddToGroup(nameof(BuildingComponent));
        GameEvents.EmitBuildingPlaced(this);
    }

    public Vector2I GetGridCellPosition()
    {
        var gridPosition = GlobalPosition / 64;
        gridPosition = gridPosition.Floor();

        return new Vector2I((int)gridPosition.X, (int)gridPosition.Y);
    }
}