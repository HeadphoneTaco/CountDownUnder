using UnityEngine;
using TMPro;

/// <summary>
/// Turns the night timer into a moving clock face. Both hands are pivoted at 0.5, 0 in
/// the Clock prefab, so rotating the RectTransform swings them from the centre correctly.
///
/// Reads the NightTimer for the in-fiction hour rather than tracking its own time, so the
/// dial and the actual deadline can never disagree.
/// </summary>
public class ClockUI : MonoBehaviour
{
    [Header("Hands")]
    [SerializeField] private RectTransform _hourHand;
    [SerializeField] private RectTransform _minuteHand;

    [Tooltip("Leave empty to find the NightTimer in the scene.")]
    [SerializeField] private NightTimer _timer;

    [Header("Optional Readout")]
    [SerializeField] private TMP_Text _timeLabel;

    [Header("Sunrise Warning")]
    [Tooltip("Pulse the clock once the night is nearly over.")]
    [SerializeField] private bool _pulseNearSunrise = true;

    [Range(0f, 1f)][SerializeField] private float _warningThreshold = 0.2f;
    [SerializeField] private float _pulseRate = 2f;
    [SerializeField] private float _pulseScale = 0.12f;

    private Vector3 _baseScale;
    private float _normalisedRemaining = 1f;

    private void Awake()
    {
        _baseScale = transform.localScale;
        if (_timer == null) _timer = FindFirstObjectByType<NightTimer>();

        if (_timer == null)
        {
            Debug.LogWarning("[ClockUI] No NightTimer in the scene, so the clock will not move.", this);
        }
    }

    private void OnEnable()
    {
        EventManager.NightTimeChanged += OnNightTimeChanged;
    }

    private void OnDisable()
    {
        EventManager.NightTimeChanged -= OnNightTimeChanged;
    }

    private void OnNightTimeChanged(float normalisedRemaining)
    {
        _normalisedRemaining = normalisedRemaining;
        if (_timer != null) UpdateHands(_timer.CurrentHour);
    }

    private void UpdateHands(float hour)
    {
        // Negative because UI rotation runs counter-clockwise and clocks do not.
        if (_hourHand != null) _hourHand.localRotation = Quaternion.Euler(0f, 0f, -(hour % 12f) * 30f);
        if (_minuteHand != null) _minuteHand.localRotation = Quaternion.Euler(0f, 0f, -(hour % 1f) * 360f);

        if (_timeLabel != null)
        {
            int h = Mathf.FloorToInt(hour) % 12;
            if (h == 0) h = 12;
            int m = Mathf.FloorToInt((hour % 1f) * 60f);
            _timeLabel.text = $"{h}:{m:00}";
        }
    }

    private void Update()
    {
        if (!_pulseNearSunrise) return;

        if (_normalisedRemaining > _warningThreshold)
        {
            transform.localScale = _baseScale;
            return;
        }

        // Tightens as sunrise approaches, so the urgency is legible without a countdown.
        float severity = 1f - (_normalisedRemaining / Mathf.Max(_warningThreshold, 0.0001f));
        float pulse = (Mathf.Sin(Time.unscaledTime * _pulseRate * Mathf.PI * 2f) + 1f) * 0.5f;
        transform.localScale = _baseScale * (1f + _pulseScale * severity * pulse);
    }
}
