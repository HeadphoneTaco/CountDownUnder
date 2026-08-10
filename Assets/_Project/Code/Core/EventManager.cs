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

    /// <summary>The run was completed successfully. Raised by the win trigger.</summary>
    public static Action PlayerWon;

    /// <summary>The player has latched onto a victim and started draining.</summary>
    public static Action PlayerEatStarted;

    /// <summary>Eating stopped, whether the victim ran dry or the player was interrupted.</summary>
    public static Action PlayerEatEnded;

    /// <summary>A victim has been drained completely. Fired by the victim, not the player.</summary>
    public static Action VictimDrained;
    public static Action PauseToggleRequested;
    public static Action<bool> GamePauseChanged;
    public static Action<float, float> VolumeChanged;
}
