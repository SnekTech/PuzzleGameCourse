﻿using System.Linq;
using Godot;

namespace PuzzleGameCourse.Component;

public partial class BuildingAnimatorComponent : Node2D
{
    private Tween activeTween;
    private Node2D animationRootNode;

    public override void _Ready()
    {
        SetupNodes();
        PlayInAnimation();
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
        activeTween.TweenProperty(animationRootNode, new NodePath(Node2D.PropertyName.Position), Vector2.Up * 16, 0.1)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);
        activeTween.TweenProperty(animationRootNode, "position", Vector2.Up * Vector2.Zero, 0.1)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.In);
    }

    private void SetupNodes()
    {
        var spriteNode = GetChildren().FirstOrDefault() as Node2D;
        if (spriteNode == null) return;

        RemoveChild(spriteNode);
        Position = new Vector2(Position.X, spriteNode.Position.Y);
        animationRootNode = new Node2D();
        AddChild(animationRootNode);
        animationRootNode.AddChild(spriteNode);
        spriteNode.Position = new Vector2(spriteNode.Position.X, 0);
    }
}