using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Drives the blood meter from EventManager.PlayerHealthChange.
///
/// Two fills sit on top of each other. The front fill snaps toward the new value
/// quickly so the bar feels responsive, and a "chip" fill behind it drains a beat
/// later in a lighter colour. The gap between them is the damage you just took,
/// which reads at a glance without needing to have been watching the bar.
///
/// Think of a fuel gauge with a lazy needle behind the real one.
/// </summary>
public class BloodBarUI : MonoBehaviour
{
    [Header("Fills")]
    [Tooltip("Front fill. Image Type must be Filled, Fill Method Horizontal. Optional if only using the text readout.")]
    [SerializeField] private Image _mainFill;

    [Tooltip("Optional trailing fill behind the main one. Same rect, same Filled setup, drawn first in the hierarchy.")]
    [SerializeField] private Image _chipFill;

    [Header("Text Readout")]
    [Tooltip("Optional. Drop the existing BloodAmount TMP object here to get a number while the bar art is still being made.")]
    [SerializeField] private TMP_Text _readout;

    [Tooltip("{0} is the percentage, already rounded. For example \"{0}%\" or \"BLOOD {0}\".")]
    [SerializeField] private string _readoutFormat = "{0}%";

    [Header("Timing")]
    [Tooltip("How fast the front fill chases the true value, in fill units per second.")]
    [SerializeField] private float _mainFillSpeed = 2.5f;

    [Tooltip("Seconds the chip fill waits after a hit before it starts catching up.")]
    [SerializeField] private float _chipDelay = 0.35f;

    [Tooltip("How fast the chip fill catches up once it starts moving.")]
    [SerializeField] private float _chipFillSpeed = 0.6f;

    [Header("Colour")]
    [SerializeField] private Color _healthyColour = new Color(0.72f, 0.05f, 0.10f, 1f);
    [SerializeField] private Color _criticalColour = new Color(1f, 0.35f, 0.35f, 1f);

    [Tooltip("Fill level at or below which the bar starts pulsing. 0.25 means the last quarter.")]
    [Range(0f, 1f)][SerializeField] private float _criticalThreshold = 0.25f;

    [Tooltip("Pulses per second while critical. Set to 0 to hold a flat colour instead.")]
    [SerializeField] private float _criticalPulseRate = 2.2f;

    private float _targetFill = 1f;
    private float _displayedFill = 1f;
    private float _chipTimer;

    private void Awake()
    {
        if (_mainFill == null && _readout == null)
        {
            Debug.LogError("[BloodBarUI] Neither a fill image nor a text readout is assigned, so nothing will display.", this);
            enabled = false;
            return;
        }

        // A Simple image ignores fillAmount entirely, which looks like the script is broken.
        if (_mainFill != null && _mainFill.type != Image.Type.Filled)
        {
            Debug.LogWarning($"[BloodBarUI] '{_mainFill.name}' Image Type is {_mainFill.type}. " +
                             "Set it to Filled or fillAmount does nothing.", _mainFill);
        }
    }

    private void OnEnable()
    {
        EventManager.PlayerHealthChange += OnHealthChanged;
    }

    private void OnDisable()
    {
        EventManager.PlayerHealthChange -= OnHealthChanged;
    }

    private void OnHealthChanged(float normalised)
    {
        float clamped = Mathf.Clamp01(normalised);

        // Only a loss should hold the chip back. Healing should close the gap immediately,
        // otherwise the chip bar reads as damage the player never took.
        if (clamped < _targetFill) _chipTimer = _chipDelay;
        else if (_chipFill != null) _chipFill.fillAmount = clamped;

        _targetFill = clamped;
    }

    private void Update()
    {
        // Unscaled, so the bar keeps settling while the pause menu is open rather than
        // freezing mid animation.
        float dt = Time.unscaledDeltaTime;

        _displayedFill = Mathf.MoveTowards(_displayedFill, _targetFill, _mainFillSpeed * dt);

        Color colour = CurrentColour();

        if (_mainFill != null)
        {
            _mainFill.fillAmount = _displayedFill;
            _mainFill.color = colour;
        }

        if (_readout != null)
        {
            _readout.text = string.Format(_readoutFormat, Mathf.CeilToInt(_displayedFill * 100f));
            _readout.color = colour;
        }

        if (_chipFill == null) return;

        if (_chipTimer > 0f) _chipTimer -= dt;
        else _chipFill.fillAmount = Mathf.MoveTowards(_chipFill.fillAmount, _targetFill, _chipFillSpeed * dt);
    }

    private Color CurrentColour()
    {
        if (_displayedFill > _criticalThreshold) return _healthyColour;

        if (_criticalPulseRate <= 0f) return _criticalColour;

        float pulse = (Mathf.Sin(Time.unscaledTime * _criticalPulseRate * Mathf.PI * 2f) + 1f) * 0.5f;
        return Color.Lerp(_healthyColour, _criticalColour, pulse);
    }

    /// <summary>Snap the bar to a value with no animation. Use on respawn or level load.</summary>
    public void SetImmediate(float normalised)
    {
        _targetFill = Mathf.Clamp01(normalised);
        _displayedFill = _targetFill;
        _chipTimer = 0f;
        if (_mainFill != null) _mainFill.fillAmount = _targetFill;
        if (_chipFill != null) _chipFill.fillAmount = _targetFill;
        if (_readout != null) _readout.text = string.Format(_readoutFormat, Mathf.CeilToInt(_targetFill * 100f));
    }
}
