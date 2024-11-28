using Godot;

namespace PuzzleGameCourse.UI;

public partial class MainMenu: Node
{
    private Button playButton;
    private Button quitButton;
    private Control mainMenuContainer;
    private LevelSelectScreen levelSelectScreen;

    public override void _Ready()
    {
        playButton = GetNode<Button>("%PlayButton");
        quitButton = GetNode<Button>("%QuitButton");
        mainMenuContainer = GetNode<Control>("%MainMenuContainer");
        levelSelectScreen = GetNode<LevelSelectScreen>("%LevelSelectScreen");

        mainMenuContainer.Visible = true;
        levelSelectScreen.Visible = false;

        playButton.Pressed += OnPlayButtonPressed;
        levelSelectScreen.BackPressed += OnLevelSelectBackPressed;
        quitButton.Pressed += OnQuitButtonPressed;
    }

    private void OnPlayButtonPressed()
    {
        mainMenuContainer.Visible = false;
        levelSelectScreen.Visible = true;
    }

    private void OnLevelSelectBackPressed()
    {
        mainMenuContainer.Visible = true;
        levelSelectScreen.Visible = false;
    }

    private void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }
}