using Godot;

namespace PuzzleGameCourse.Building;

public partial class BuildingGhost : Node2D
{
    private Node2D topLeft, topRight, bottomLeft, bottomRight;
    private Node2D spriteRoot;
    private Node2D upDownRoot;

    private Tween _spriteTween;

    public override void _Ready()
    {
        topLeft = GetNode<Node2D>("TopLeft");
        topRight = GetNode<Node2D>("TopRight");
        bottomLeft = GetNode<Node2D>("BottomLeft");
        bottomRight = GetNode<Node2D>("BottomRight");
        spriteRoot = GetNode<Node2D>("SpriteRoot");
        upDownRoot = GetNode<Node2D>("%UpDownRoot");

        var upDownTween = CreateTween();
        upDownTween.SetLoops();
        upDownTween.TweenProperty(upDownRoot, "position", Vector2.Down * 6, 0.3)
            .SetEase(Tween.EaseType.InOut)
            .SetTrans(Tween.TransitionType.Quad);
        upDownTween.TweenProperty(upDownRoot, "position", Vector2.Up * 6, 0.3)
            .SetEase(Tween.EaseType.InOut)
            .SetTrans(Tween.TransitionType.Quad);
    }

    public void SetInvalid()
    {
        Modulate = Colors.Red;
        upDownRoot.Modulate = Modulate;
    }

    public void SetValid()
    {
        Modulate = Colors.White;
        upDownRoot.Modulate = Modulate;
    }

    public void SetDimensions(Vector2I dimensions)
    {
        bottomLeft.Position = dimensions * new Vector2I(0, 64);
        bottomRight.Position = dimensions * new Vector2I(64, 64);
        topRight.Position = dimensions * new Vector2I(64, 0);
    }

    public void AddSpriteNode(Node2D spriteNode)
    {
        upDownRoot.AddChild(spriteNode);
    }

    public void DoHoverAnimation()
    {
        if (_spriteTween != null && _spriteTween.IsValid())
        {
            _spriteTween.Kill();
        }

        _spriteTween = CreateTween();
        _spriteTween.TweenProperty(spriteRoot, "global_position", GlobalPosition, 0.3)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);
    }
}