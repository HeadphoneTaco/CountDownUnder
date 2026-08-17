using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Turns the run clock into visible daylight. Listens to the same
/// EventManager.NightTimeChanged broadcast the clock face uses, and warms the sky
/// and the global 2D light as sunrise closes in.
///
/// This is a warning, not a mood. The clock in the corner only helps a player who
/// looks at it, and by then they are already reading the pulse. The background
/// creeping bright is the same information delivered without asking them to look
/// away from the thing chasing them.
///
/// Reads the NightTimer through the event rather than polling it, so pausing,
/// AddTime and Skip To Sunrise are all handled for free.
///
/// Put one anywhere in the game scene. It does not need to sit on the light.
/// </summary>
public class DawnLighting : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Leave empty to find the NightTimer in the scene.")]
    [SerializeField] private NightTimer _timer;

    [Tooltip("The Global Light 2D. Leave empty to find the first global light in the scene.")]
    [SerializeField] private Light2D _globalLight;

    [Tooltip("Camera whose background colour is the sky. Leave empty to use the main camera.")]
    [SerializeField] private Camera _skyCamera;

    [Header("Timing")]
    [Tooltip("How many real seconds of sunrise there are. The ramp runs across the LAST this " +
             "many seconds of the night, so a bigger number starts the brightening earlier. " +
             "Compare it against Night Duration Seconds on the NightTimer: 60 out of 180 is " +
             "the final third of the run. Right-click this component for Log Dawn Window to " +
             "have it spell the split out for the values you have set.")]
    [Min(0f)][SerializeField] private float _dawnDurationSeconds = 60f;

    [Tooltip("Shape of the ramp inside that window. 1 is a straight line. Above 1 holds the " +
             "dark and dumps most of the change at the end, which reads as panic. Below 1 " +
             "front loads it, so the sky is already visibly wrong well before sunrise.")]
    [Range(0.25f, 4f)][SerializeField] private float _rampSharpness = 1f;

    [Tooltip("Ease the ends of the ramp so the change never has a visible start or stop.")]
    [SerializeField] private bool _smoothEnds = true;

    [Header("Sky")]
    [Tooltip("Drive the camera background colour toward the dawn colour.")]
    [SerializeField] private bool _affectSky = true;

    [Tooltip("Sky at sunrise. The night colour is whatever the camera is already set to, " +
             "read once on Awake, so tuning night happens on the camera as usual.")]
    [SerializeField] private Color _dawnSkyColour = new Color(0.86f, 0.55f, 0.42f, 0f);

    [Header("Global Light")]
    [Tooltip("Drive the Global Light 2D intensity and colour toward the dawn values.")]
    [SerializeField] private bool _affectGlobalLight = true;

    [Tooltip("Global light intensity at sunrise. The night intensity is read off the light " +
             "on Awake. Going much past 0.4 washes out the spot lights the level is built around.")]
    [Range(0f, 2f)][SerializeField] private float _dawnLightIntensity = 0.35f;

    [Tooltip("Global light tint at sunrise. A warm value here is what separates dawn from " +
             "someone simply turning the brightness up.")]
    [SerializeField] private Color _dawnLightColour = new Color(1f, 0.82f, 0.68f, 1f);

    private Color _nightSkyColour;
    private Color _nightLightColour;
    private float _nightLightIntensity;

    private float _progress;

    /// <summary>0 while the night is still safely long, 1 at sunrise.</summary>
    public float DawnProgress => _progress;

    private void Awake()
    {
        if (_timer == null) _timer = FindFirstObjectByType<NightTimer>();
        if (_skyCamera == null) _skyCamera = Camera.main;
        if (_globalLight == null) _globalLight = FindGlobalLight();

        if (_skyCamera != null) _nightSkyColour = _skyCamera.backgroundColor;

        if (_globalLight != null)
        {
            _nightLightColour = _globalLight.color;
            _nightLightIntensity = _globalLight.intensity;
        }
        else if (_affectGlobalLight)
        {
            Debug.LogWarning("[DawnLighting] No Global Light 2D found, so only the sky will change.", this);
        }

        if (_timer == null)
        {
            Debug.LogWarning("[DawnLighting] No NightTimer in the scene, so dawn will never arrive.", this);
        }
    }

    private void Start()
    {
        // The timer broadcasts in its own Start, and ordering between the two is not
        // guaranteed. Setting the state here means a late subscribe never leaves the
        // scene sitting at whatever the last run ended on.
        Apply(CurrentProgress());
    }

    private void OnEnable()
    {
        EventManager.NightTimeChanged += OnNightTimeChanged;
    }

    private void OnDisable()
    {
        EventManager.NightTimeChanged -= OnNightTimeChanged;

        // Hand the scene back the way it was found. Without this, exiting play mode
        // can leave the edited camera and light stuck at their dawn values.
        Apply(0f);
    }

    private void OnNightTimeChanged(float normalisedRemaining)
    {
        Apply(CurrentProgress());
    }

    private float CurrentProgress()
    {
        if (_timer == null) return 0f;
        return ProgressFrom(_timer.SecondsRemaining);
    }

    /// <summary>
    /// 0 until the night has burned down to the dawn window, then climbs to 1 at sunrise.
    /// Works in seconds rather than the 0 to 1 event value so the tuning number in the
    /// Inspector means the same thing as the one on the NightTimer next to it.
    /// </summary>
    private float ProgressFrom(float secondsRemaining)
    {
        if (_dawnDurationSeconds <= 0f) return secondsRemaining <= 0f ? 1f : 0f;
        if (secondsRemaining >= _dawnDurationSeconds) return 0f;

        float t = 1f - Mathf.Clamp01(secondsRemaining / _dawnDurationSeconds);
        t = Mathf.Pow(t, _rampSharpness);
        return _smoothEnds ? Mathf.SmoothStep(0f, 1f, t) : t;
    }

    private void Apply(float t)
    {
        _progress = t;

        if (_affectSky && _skyCamera != null)
        {
            _skyCamera.backgroundColor = Color.Lerp(_nightSkyColour, _dawnSkyColour, t);
        }

        if (_affectGlobalLight && _globalLight != null)
        {
            _globalLight.color = Color.Lerp(_nightLightColour, _dawnLightColour, t);
            _globalLight.intensity = Mathf.Lerp(_nightLightIntensity, _dawnLightIntensity, t);
        }
    }

    private static Light2D FindGlobalLight()
    {
        Light2D[] lights = FindObjectsByType<Light2D>(FindObjectsSortMode.None);
        foreach (Light2D light in lights)
        {
            if (light.lightType == Light2D.LightType.Global) return light;
        }

        return null;
    }

    /// <summary>
    /// Tuning aid. Turns the two numbers into the sentence you actually care about,
    /// so the window does not have to be worked out in your head every time.
    /// </summary>
    [ContextMenu("Log Dawn Window")]
    private void LogDawnWindow()
    {
        NightTimer timer = _timer != null ? _timer : FindFirstObjectByType<NightTimer>();
        if (timer == null)
        {
            Debug.LogWarning("[DawnLighting] No NightTimer to measure the window against.", this);
            return;
        }

        float night = timer.NightDurationSeconds;
        float dawn = Mathf.Min(_dawnDurationSeconds, night);
        float startsAt = night - dawn;
        float share = night <= 0f ? 0f : dawn / night * 100f;

        Debug.Log($"[DawnLighting] Night is {night:0.#}s. Sky holds dark for {startsAt:0.#}s, " +
                  $"then brightens over the final {dawn:0.#}s ({share:0}% of the run).", this);
    }

    /// <summary>
    /// Preview hook. Right-click the component and pick this to see the sunrise colours
    /// on the scene without playing three minutes of the level first.
    /// </summary>
    [ContextMenu("Preview Sunrise")]
    private void PreviewSunrise()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[DawnLighting] Preview Sunrise only works in play mode.", this);
            return;
        }

        Apply(1f);
    }
}
