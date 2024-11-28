using Godot;
using PuzzleGameCourse.Resources.Level;

namespace PuzzleGameCourse.UI;

public partial class LevelSelectSection : PanelContainer
{
    [Signal]
    public delegate void LevelSelectedEventHandler(int levelIndex);
    
    private Button button;
    private Label resourceCountLabel;
    private Label levelNumberLabel;

    private int _levelIndex;

    public override void _Ready()
    {
        button = GetNode<Button>("%Button");
        resourceCountLabel = GetNode<Label>("%ResourceCountLabel");
        levelNumberLabel = GetNode<Label>("%LevelNumberLabel");

        button.Pressed += OnButtonPressed;
    }

    public void SetLevelDefinition(LevelDefinitionResource levelDefinitionResource)
    {
        resourceCountLabel.Text = levelDefinitionResource.StartingResourceCount.ToString();
    }

    public void SetLevelIndex(int index)
    {
        _levelIndex = index;
        levelNumberLabel.Text = $"Level {index + 1}";
    }

    private void OnButtonPressed()
    {
        EmitSignal(SignalName.LevelSelected, _levelIndex);
    }
}