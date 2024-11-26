using Godot;
using PuzzleGameCourse.Autoload;

namespace PuzzleGameCourse.UI;

public partial class MainMenu: Node
{
    private Button playButton;

    public override void _Ready()
    {
        playButton = GetNode<Button>("%PlayButton");

        playButton.Pressed += OnPlayButtonPressed;
    }

    private void OnPlayButtonPressed()
    {
        LevelManager.Instance.ChangeToLevel(0);
    }
}