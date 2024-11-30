﻿using System.Linq;
using Godot;

namespace PuzzleGameCourse.CustomExtensions;

public static class NodeExtensions
{
    public static T GetFirstNodeOfType<T>(this Node node) where T : Node
    {
        var children = node.GetChildren();
        var firstNode = children.FirstOrDefault(child => child is T);
        return firstNode as T;
    }
}