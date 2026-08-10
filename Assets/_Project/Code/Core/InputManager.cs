using UnityEngine;

/// <summary>
/// Reads the input asset and turns it into EventManager calls. Nothing else in the
/// project should touch PlayerInputs directly.
///
/// Two action maps are live:
///   Player - movement, mist, jump. Switched off while paused.
///   UI     - pause and menu navigation. Stays on so the menu can be closed again.
/// </summary>
public class InputManager : Singleton<InputManager>
{
    private PlayerInputs _inputs;
    private Vector2 _currentDI;
    private Vector2 _lastReadDI;
    private bool _gameplayEnabled = true;
    private bool _warnedNoPauseListener;

    /// <summary>Exposed for tooling and rebinding work. Read gameplay actions through the events, not through this.</summary>
    public PlayerInputs Inputs => _inputs;

    protected override void Awake()
    {
        base.Awake();

        // A duplicate is destroyed by the base class, but its Awake still finishes running.
        // Building a second PlayerInputs here would leak an unmanaged action state.
        if (Instance != this) return;

        _inputs = new PlayerInputs();
    }

    private void OnEnable()
    {
        if (_inputs == null) return;
        _inputs.Player.Enable();
        _inputs.UI.Enable();
        _gameplayEnabled = true;
    }

    private void OnDisable()
    {
        if (_inputs == null) return;
        _inputs.Disable();
    }

    private void OnDestroy()
    {
        if (Instance != this) return;
        _inputs?.Dispose();
        _inputs = null;
    }

    public void Update()
    {
        if (_inputs == null) return;

        // Pause is checked first and outside the gameplay gate, otherwise the game
        // could be paused with no way to unpause it.
        if (_inputs.UI.Pause.WasPressedThisFrame())
        {
            // An event with no listeners fails completely silently, which looks
            // identical to the key not being read at all. Say which one it is.
            if (EventManager.PauseToggleRequested == null && !_warnedNoPauseListener)
            {
                _warnedNoPauseListener = true;
                Debug.LogError("[InputManager] Pause was pressed and the input is working, but nothing is listening. " +
                               "This scene has no PauseManager. Add an empty GameObject with a PauseManager component.", this);
            }

            EventManager.PauseToggleRequested?.Invoke();
        }

        if (!_gameplayEnabled) return;

        _currentDI = _inputs.Player.Move.ReadValue<Vector2>();
        if (_lastReadDI != _currentDI)
        {
            OnMove(_currentDI);
            _lastReadDI = _currentDI;
        }

        if (_inputs.Player.Mist.WasReleasedThisFrame()) { EventManager.TransformationChanged?.Invoke(false); }
        if (_inputs.Player.Mist.WasPressedThisFrame()) { EventManager.TransformationChanged?.Invoke(true); }
        if (_inputs.Player.Jump.WasPressedThisFrame()) EventManager.JumpEvent?.Invoke();
    }

    public void OnMove(Vector2 di)
    {
        EventManager.DIEvent?.Invoke(di);
    }

    /// <summary>Called by PauseManager. Stops movement dead rather than leaving the last held direction applied.</summary>
    public void SetGameplayInputEnabled(bool enabled)
    {
        _gameplayEnabled = enabled;
        if (_inputs == null) return;

        if (enabled)
        {
            _inputs.Player.Enable();
        }
        else
        {
            _inputs.Player.Disable();

            // Release a held mist so the player does not resume mid transformation,
            // and zero the movement so they do not walk out from under the pause menu.
            EventManager.TransformationChanged?.Invoke(false);
            _lastReadDI = Vector2.zero;
            EventManager.DIEvent?.Invoke(Vector2.zero);
        }
    }
}
