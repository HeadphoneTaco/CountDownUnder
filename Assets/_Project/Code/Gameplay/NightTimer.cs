using UnityEngine;

/// <summary>
/// The run clock. Counts one night down to sunrise, and kills the player when it runs out.
///
/// The second failure state alongside blood loss: feeding keeps you alive, but standing
/// around feeding costs you the night. Put one in the game scene.
/// </summary>
public class NightTimer : MonoBehaviour
{
    [Header("Length")]
    [Tooltip("Real seconds from dusk to sunrise.")]
    [SerializeField] private float _nightDurationSeconds = 180f;

    [Tooltip("Start counting immediately. Turn off to hold the clock until something calls Begin.")]
    [SerializeField] private bool _startOnAwake = true;

    [Header("In-Fiction Time")]
    [Tooltip("Clock time at dusk, in hours on a 12 hour dial.")]
    [SerializeField] private float _startHour = 21f;

    [Tooltip("Clock time at sunrise. The hands sweep from start to end across the night.")]
    [SerializeField] private float _endHour = 6f;

    private float _remaining;
    private bool _running;
    private bool _expired;

    /// <summary>1 at dusk, 0 at sunrise.</summary>
    public float NormalisedRemaining => _nightDurationSeconds <= 0f ? 0f : _remaining / _nightDurationSeconds;

    /// <summary>Where the hands should point, in hours, walking from start hour to end hour.</summary>
    public float CurrentHour
    {
        get
        {
            // The night crosses midnight, so the end hour is treated as being on the far
            // side of 12 rather than a smaller number than the start.
            float span = _endHour - _startHour;
            if (span <= 0f) span += 12f;
            return _startHour + span * (1f - NormalisedRemaining);
        }
    }

    private void Awake()
    {
        _remaining = _nightDurationSeconds;
        if (_startOnAwake) _running = true;
    }

    private void Start()
    {
        // After every OnEnable, so the clock UI is listening before the first value lands.
        Broadcast();
    }

    private void OnEnable()
    {
        EventManager.PlayerDied += OnRunEnded;
        EventManager.PlayerWon += OnRunWon;
    }

    private void OnDisable()
    {
        EventManager.PlayerDied -= OnRunEnded;
        EventManager.PlayerWon -= OnRunWon;
    }

    private void OnRunEnded(DeathCause cause) => _running = false;

    private void OnRunWon() => _running = false;

    public void Begin()
    {
        _running = true;
    }

    public void Pause() => _running = false;

    /// <summary>
    /// Testing hook. Right-click the NightTimer component in play mode and pick this
    /// rather than sitting through the whole night to check the ending.
    /// </summary>
    [ContextMenu("Skip To Sunrise")]
    public void SkipToSunrise()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[NightTimer] Skip To Sunrise only works in play mode.", this);
            return;
        }

        _remaining = 0.01f;
        _running = true;
        _expired = false;
    }

    /// <summary>Seconds left, for a countdown readout or a debug display.</summary>
    public float SecondsRemaining => _remaining;

    /// <summary>Full length of the night in real seconds, for systems that want to tune
    /// themselves against it rather than against a 0 to 1 value.</summary>
    public float NightDurationSeconds => _nightDurationSeconds;

    /// <summary>Give the player time back. Feeding could reward a few seconds, for instance.</summary>
    public void AddTime(float seconds)
    {
        _remaining = Mathf.Clamp(_remaining + seconds, 0f, _nightDurationSeconds);
        Broadcast();
    }

    private void Update()
    {
        if (!_running || _expired) return;

        // Scaled time, so pausing genuinely stops the clock. Using unscaled here would
        // let the night run out while the player sits in the pause menu.
        _remaining -= Time.deltaTime;

        if (_remaining <= 0f)
        {
            _remaining = 0f;
            _expired = true;
            _running = false;
            Broadcast();
            Sunrise();
            return;
        }

        Broadcast();
    }

    private void Broadcast()
    {
        EventManager.NightTimeChanged?.Invoke(NormalisedRemaining);
    }

    private void Sunrise()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player == null)
        {
            Debug.LogWarning("[NightTimer] Sunrise arrived but there is no PlayerController to kill.", this);
            return;
        }

        player.Kill(DeathCause.Sunrise);
    }
}
