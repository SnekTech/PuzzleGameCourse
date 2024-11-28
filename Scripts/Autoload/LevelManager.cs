using Godot;

namespace PuzzleGameCourse.Autoload;

public partial class LevelManager : Node
{
    public static LevelManager Instance { get; private set; }

    [Export] private PackedScene[] levelScenes;

    private int _currentLevelIndex;

    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated)
        {
            Instance = this;
        }
    }

    public void ChangeToLevel(int levelIndex)
    {
        if (levelIndex >= levelScenes.Length || levelIndex < 0) return;
        _currentLevelIndex = levelIndex;

        var levelScene = levelScenes[_currentLevelIndex];
        GetTree().ChangeSceneToPacked(levelScene);
    }

    public void ChangeToNextLevel()
    {
        ChangeToLevel(_currentLevelIndex + 1);
    }
}