using UnityEngine;

/// <summary>
/// Owns the paused state of the game. Nothing else should touch Time.timeScale.
///
/// Pausing does three things:
///   1. Freezes time, which stops every FixedUpdate and any deltaTime driven logic.
///   2. Disables the gameplay action map so held inputs cannot queue up behind the menu.
///   3. Announces the change so UI can open and close itself.
///
/// Drop one of these in the game scene. It is a Singleton, so a stray second copy
/// deletes itself rather than fighting over timeScale.
/// </summary>
public class PauseManager : Singleton<PauseManager>
{
    /// <summary>Read this instead of comparing Time.timeScale to zero.</summary>
    public static bool IsPaused { get; private set; }

    [Tooltip("Turn off during cutscenes, the death sequence, or anywhere a pause would break state.")]
    [SerializeField] private bool _pauseAllowed = true;

    [Tooltip("Also pause AudioListener. Any UI click sounds need 'Ignore Listener Pause' ticked on their AudioSource or they go silent too.")]
    [SerializeField] private bool _pauseAudio = true;

    protected override void Awake()
    {
        base.Awake();

        // IsPaused is static, so it survives a scene load. If the player quit to menu
        // from a paused game, the next scene would start frozen. Clear it here.
        IsPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    private void OnEnable()
    {
        EventManager.PauseToggleRequested += TogglePause;
    }

    private void OnDisable()
    {
        EventManager.PauseToggleRequested -= TogglePause;

        // Leaving the scene while paused would strand timeScale at zero for whatever loads next.
        // This does the reset by hand rather than calling SetPaused, because on application
        // quit touching InputManager.Instance would resurrect a singleton that is already gone.
        if (!IsPaused) return;
        IsPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    public void TogglePause()
    {
        SetPaused(!IsPaused);
    }

    public void SetPaused(bool paused)
    {
        if (paused && !_pauseAllowed) return;
        if (IsPaused == paused) return;

        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
        if (_pauseAudio) AudioListener.pause = paused;

        if (InputManager.Instance != null) InputManager.Instance.SetGameplayInputEnabled(!paused);

        // Pausing with no UI listening freezes the game with nothing on screen, which
        // is indistinguishable from a hang. Better to name the missing piece.
        if (paused && EventManager.GamePauseChanged == null)
        {
            Debug.LogWarning("[PauseManager] Paused, but no UI is listening, so the screen will just freeze. " +
                             "Either there is no PauseMenuUI in the scene, or there is one that never reached " +
                             "OnEnable because its GameObject starts inactive. A pause menu script has to live " +
                             "on an object that stays active and toggle a child panel.", this);
        }

        EventManager.GamePauseChanged?.Invoke(paused);
    }

    /// <summary>Lock out pausing, for example during the death sequence.</summary>
    public void SetPauseAllowed(bool allowed)
    {
        _pauseAllowed = allowed;
        if (!allowed && IsPaused) SetPaused(false);
    }
}
