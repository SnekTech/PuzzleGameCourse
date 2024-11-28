using System.Linq;
using Godot;
using PuzzleGameCourse.Resources.Level;

namespace PuzzleGameCourse.Autoload;

public partial class LevelManager : Node
{
    public static LevelManager Instance { get; private set; }

    [Export] private LevelDefinitionResource[] levelDefinitions;

    private int _currentLevelIndex;

    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated)
        {
            Instance = this;
        }
    }

    public static LevelDefinitionResource[] GetLevelDefinitions()
    {
        return Instance.levelDefinitions.ToArray();
    }

    public void ChangeToLevel(int levelIndex)
    {
        if (levelIndex >= levelDefinitions.Length || levelIndex < 0) return;
        _currentLevelIndex = levelIndex;

        var levelDefinition = levelDefinitions[_currentLevelIndex];
        GetTree().ChangeSceneToFile(levelDefinition.LevelScenePath);
    }

    public void ChangeToNextLevel()
    {
        ChangeToLevel(_currentLevelIndex + 1);
    }
}