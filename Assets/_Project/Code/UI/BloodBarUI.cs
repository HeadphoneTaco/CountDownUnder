using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Drives the blood meter from EventManager.PlayerHealthChange.
///
/// Works with a Slider, a filled Image, a TMP readout, or any combination, so it does
/// not care which of those the HUD art settles on. Assign whichever exist.
///
/// The front reading chases the true value quickly so the bar feels responsive, and an
/// optional chip bar behind it drains a beat later. The gap between them is the damage
/// you just took, which reads at a glance without having been watching the bar. Think
/// of a fuel gauge with a lazy second needle trailing the real one.
/// </summary>
public class BloodBarUI : MonoBehaviour
{
    [Header("Slider (GameVer3 uses this)")]
    [Tooltip("BloodSlider. Min/Max are forced to 0 and 1 and interaction is switched off.")]
    [SerializeField] private Slider _mainSlider;

    [Tooltip("Optional trailing slider behind the main one, showing damage just taken.")]
    [SerializeField] private Slider _chipSlider;

    [Header("Filled Image (alternative)")]
    [Tooltip("Optional. Image Type must be Filled, Fill Method Horizontal.")]
    [SerializeField] private Image _mainFill;

    [SerializeField] private Image _chipFill;

    [Header("Text Readout")]
    [Tooltip("Optional. A TMP object for a numeric readout.")]
    [SerializeField] private TMP_Text _readout;

    [Tooltip("{0} is the percentage, already rounded. For example \"{0}%\" or \"BLOOD {0}\".")]
    [SerializeField] private string _readoutFormat = "{0}%";

    [Header("Timing")]
    [Tooltip("How fast the front reading chases the true value, in fill units per second.")]
    [SerializeField] private float _mainFillSpeed = 2.5f;

    [Tooltip("Seconds the chip bar waits after a hit before it starts catching up.")]
    [SerializeField] private float _chipDelay = 0.35f;

    [Tooltip("How fast the chip bar catches up once it starts moving.")]
    [SerializeField] private float _chipFillSpeed = 0.6f;

    [Header("Colour")]
    [Tooltip("Leave off to keep whatever colours the art already has.")]
    [SerializeField] private bool _recolourOnLowBlood = true;

    [SerializeField] private Color _healthyColour = new Color(0.72f, 0.05f, 0.10f, 1f);
    [SerializeField] private Color _criticalColour = new Color(1f, 0.35f, 0.35f, 1f);

    [Tooltip("Level at or below which the bar starts pulsing. 0.25 means the last quarter.")]
    [Range(0f, 1f)][SerializeField] private float _criticalThreshold = 0.25f;

    [Tooltip("Pulses per second while critical. 0 holds a flat colour instead.")]
    [SerializeField] private float _criticalPulseRate = 2.2f;

    [Header("Damage Flash")]
    [Tooltip("Blink the bar for a moment after taking a hit, so the eye is pulled to it.")]
    [SerializeField] private bool _flashOnHurt = true;

    [Tooltip("Seconds the bar keeps blinking after a hit.")]
    [SerializeField] private float _hurtFlashDuration = 1.2f;

    [Tooltip("Blinks per second during that window.")]
    [SerializeField] private float _hurtFlashRate = 6f;

    [SerializeField] private Color _hurtFlashColour = Color.white;

    private float _targetFill = 1f;
    private float _displayedFill = 1f;
    private float _chipTimer;
    private Graphic _mainSliderFillGraphic;
    private float _hurtFlashTimer;
    private Color _restingFillColour = Color.white;
    private Color _restingReadoutColour = Color.white;

    private void Awake()
    {
        if (_mainSlider == null && _mainFill == null && _readout == null)
        {
            Debug.LogError("[BloodBarUI] No slider, fill image, or readout assigned, so nothing will display.", this);
            enabled = false;
            return;
        }

        PrepareSlider(_mainSlider);
        PrepareSlider(_chipSlider);

        if (_mainSlider != null && _mainSlider.fillRect != null)
        {
            _mainSliderFillGraphic = _mainSlider.fillRect.GetComponent<Graphic>();
        }

        // Remember whatever the art was authored with, so the flash has something to
        // return to when low blood recolouring is switched off.
        if (_mainSliderFillGraphic != null) _restingFillColour = _mainSliderFillGraphic.color;
        else if (_mainFill != null) _restingFillColour = _mainFill.color;
        if (_readout != null) _restingReadoutColour = _readout.color;

        // A Simple image ignores fillAmount entirely, which looks like the script is broken.
        if (_mainFill != null && _mainFill.type != Image.Type.Filled)
        {
            Debug.LogWarning($"[BloodBarUI] '{_mainFill.name}' Image Type is {_mainFill.type}. " +
                             "Set it to Filled or fillAmount does nothing.", _mainFill);
        }
    }

    private void PrepareSlider(Slider slider)
    {
        if (slider == null) return;

        slider.minValue = 0f;
        slider.maxValue = 1f;

        // An interactable Slider lets the player drag their own health bar around,
        // and it steals clicks from anything underneath it.
        slider.interactable = false;
        if (slider.targetGraphic != null) slider.targetGraphic.raycastTarget = false;
    }

    private void OnEnable()
    {
        EventManager.PlayerHealthChange += OnHealthChanged;
        EventManager.PlayerHurt += OnHurt;
    }

    private void OnDisable()
    {
        EventManager.PlayerHealthChange -= OnHealthChanged;
        EventManager.PlayerHurt -= OnHurt;
    }

    private void OnHurt()
    {
        if (_flashOnHurt) _hurtFlashTimer = _hurtFlashDuration;
    }

    private void OnHealthChanged(float normalised)
    {
        float clamped = Mathf.Clamp01(normalised);

        // Only a loss should hold the chip bar back. Healing closes the gap immediately,
        // otherwise the chip bar reads as damage the player never took.
        if (clamped < _targetFill) _chipTimer = _chipDelay;
        else SetChip(clamped);

        _targetFill = clamped;
    }

    private void Update()
    {
        // Unscaled, so the bar keeps settling while the pause menu is open rather than
        // freezing mid animation.
        float dt = Time.unscaledDeltaTime;

        _displayedFill = Mathf.MoveTowards(_displayedFill, _targetFill, _mainFillSpeed * dt);

        if (_mainSlider != null) _mainSlider.value = _displayedFill;
        if (_mainFill != null) _mainFill.fillAmount = _displayedFill;
        if (_readout != null) _readout.text = string.Format(_readoutFormat, Mathf.CeilToInt(_displayedFill * 100f));

        bool wasFlashing = _hurtFlashTimer > 0f;
        if (wasFlashing) _hurtFlashTimer -= dt;

        // Only touch colours when something actually wants to change them, so a bar with
        // both options off keeps exactly the colours the art was authored with. The
        // wasFlashing term matters on the frame the blink expires: without it the bar
        // would keep whichever half of the blink it happened to stop on.
        if (_recolourOnLowBlood || wasFlashing)
        {
            Color fill = ApplyHurtFlash(_recolourOnLowBlood ? CurrentColour() : _restingFillColour);
            if (_mainSliderFillGraphic != null) _mainSliderFillGraphic.color = fill;
            if (_mainFill != null) _mainFill.color = fill;

            if (_readout != null)
            {
                _readout.color = ApplyHurtFlash(_recolourOnLowBlood ? CurrentColour() : _restingReadoutColour);
            }
        }

        if (_chipSlider == null && _chipFill == null) return;

        if (_chipTimer > 0f) _chipTimer -= dt;
        else SetChip(Mathf.MoveTowards(CurrentChip(), _targetFill, _chipFillSpeed * dt));
    }

    private float CurrentChip()
    {
        if (_chipSlider != null) return _chipSlider.value;
        if (_chipFill != null) return _chipFill.fillAmount;
        return _targetFill;
    }

    private void SetChip(float value)
    {
        if (_chipSlider != null) _chipSlider.value = value;
        if (_chipFill != null) _chipFill.fillAmount = value;
    }

    /// <summary>
    /// Square wave rather than a sine, because a hard on/off blink reads as an alarm
    /// while a smooth fade reads as a slow colour change and is easy to miss.
    /// </summary>
    private Color ApplyHurtFlash(Color baseColour)
    {
        if (_hurtFlashTimer <= 0f) return baseColour;

        bool on = Mathf.Repeat(_hurtFlashTimer * _hurtFlashRate, 1f) < 0.5f;
        return on ? _hurtFlashColour : baseColour;
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

        if (_mainSlider != null) _mainSlider.value = _targetFill;
        if (_mainFill != null) _mainFill.fillAmount = _targetFill;
        if (_readout != null) _readout.text = string.Format(_readoutFormat, Mathf.CeilToInt(_targetFill * 100f));
        SetChip(_targetFill);
    }
}
