using UnityEngine;
using System;

public static class EventManager
{
    public static Action<Vector2> DIEvent;
    public static Action<bool> TransformationChanged;
    public static Action JumpEvent;
    public static Action<float> PlayerHealthChange;
    public static Action PlayerHurt;
}
