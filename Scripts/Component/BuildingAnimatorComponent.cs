using System.Linq;
using Godot;

namespace PuzzleGameCourse.Component;

public partial class BuildingAnimatorComponent : Node2D
{
    [Signal]
    public delegate void DestroyAnimationFinishedEventHandler();
    
    private Tween activeTween;
    private Node2D animationRootNode;

    public override void _Ready()
    {
        SetupNodes();
    }

    public void PlayInAnimation()
    {
        if (animationRootNode == null) return;

        if (activeTween != null && activeTween.IsValid())
        {
            activeTween.Kill();
        }

        activeTween = CreateTween();
        activeTween.TweenProperty(animationRootNode, "position", Vector2.Zero, 0.3)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.In)
            .From(Vector2.Up * 128);
        activeTween.TweenProperty(animationRootNode, new NodePath(PropertyName.Position), Vector2.Up * 16, 0.1)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);
        activeTween.TweenProperty(animationRootNode, "position", Vector2.Up * Vector2.Zero, 0.1)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.In);
    }

    public void PlayDestroyAnimation()
    {
        if (animationRootNode == null) return;

        if (activeTween != null && activeTween.IsValid())
        {
            activeTween.Kill();
        }

        activeTween = CreateTween();
        activeTween.TweenProperty(animationRootNode, PropertyName.RotationDegrees.ToString(), -5, 0.1);
        activeTween.TweenProperty(animationRootNode, PropertyName.RotationDegrees.ToString(), 5, 0.1);
        activeTween.TweenProperty(animationRootNode, PropertyName.RotationDegrees.ToString(), -2, 0.1);
        activeTween.TweenProperty(animationRootNode, PropertyName.RotationDegrees.ToString(), 2, 0.1);
        activeTween.TweenProperty(animationRootNode, PropertyName.RotationDegrees.ToString(), 0, 0.1);

        activeTween.TweenProperty(animationRootNode, PropertyName.Position.ToString(), Vector2.Down * 300, 0.4)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.In);
        activeTween.Finished += () => EmitSignal(SignalName.DestroyAnimationFinished);
    }

    private void SetupNodes()
    {
        var spriteNode = GetChildren().FirstOrDefault() as Node2D;
        if (spriteNode == null) return;

        RemoveChild(spriteNode);
        Position = new Vector2(spriteNode.Position.X, spriteNode.Position.Y);
        animationRootNode = new Node2D();
        AddChild(animationRootNode);
        animationRootNode.AddChild(spriteNode);
        spriteNode.Position = new Vector2(0, 0);
    }
}