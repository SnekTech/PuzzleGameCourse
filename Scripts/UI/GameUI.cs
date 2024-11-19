using Godot;
using PuzzleGameCourse.Building;

namespace PuzzleGameCourse.UI;

public partial class GameUI : CanvasLayer
{
    [Signal]
    public delegate void BuildingResourceSelectedEventHandler(BuildingResource buildingResource);

    [Export]
    private BuildingResource[] _buildingResources;

    private HBoxContainer _hBoxContainer;

    public override void _Ready()
    {
        _hBoxContainer = GetNode<HBoxContainer>("MarginContainer/HBoxContainer");
        
        CreateBuildingButtons();
    }

    private void CreateBuildingButtons()
    {
        foreach (var buildingResource in _buildingResources)
        {
            var buildingButton = new Button();
            buildingButton.Text = $"Place {buildingResource.DisplayName}";
            _hBoxContainer.AddChild(buildingButton);
            buildingButton.Pressed += () =>
            {
                EmitSignal(SignalName.BuildingResourceSelected, buildingResource);
            };
        }
    }
}