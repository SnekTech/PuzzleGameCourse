﻿using Godot;
using PuzzleGameCourse.Building;

namespace PuzzleGameCourse.UI;

public partial class BuildingSection : PanelContainer
{
    [Signal]
    public delegate void SelectButtonPressedEventHandler();
    
    private Label titleLabel;
    private Button selectButton;

    public override void _Ready()
    {
        titleLabel = GetNode<Label>("%Label");
        selectButton = GetNode<Button>("%Button");

        selectButton.Pressed += OnSelectButtonPressed;
    }

    public void SetBuildingResource(BuildingResource buildingResource)
    {
        titleLabel.Text = buildingResource.DisplayName;
        selectButton.Text = $"Select (Cost {buildingResource.ResourceCost})";
    }

    private void OnSelectButtonPressed()
    {
        EmitSignal(SignalName.SelectButtonPressed);
    }
}