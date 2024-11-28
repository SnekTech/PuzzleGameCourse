using Godot;
using PuzzleGameCourse.Autoload;

namespace PuzzleGameCourse.UI;

public partial class LevelCompleteScreen : CanvasLayer
{
    private Button nextLevelButton;

    public override void _Ready()
    {
        nextLevelButton = GetNode<Button>("%NextLevelButton");

        nextLevelButton.Pressed += OnNextLevelButtonPressed;
    }

    private void OnNextLevelButtonPressed()
    {
        LevelManager.Instance.ChangeToNextLevel();
    }
}