using UnityEngine;
using System;

public static class EventManager
{
    public static Action<Vector2> DIEvent;
    public static Action<bool> TransformationChanged;
    public static Action JumpEvent;
    public static Action<float> PlayerHealthChange;
    public static Action PlayerHurt;
    public static Action PlayerDied;
    public static Action PauseToggleRequested;
    public static Action<bool> GamePauseChanged;
    public static Action<float, float> VolumeChanged;
}
