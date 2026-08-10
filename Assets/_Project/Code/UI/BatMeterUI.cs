using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives the bat flight meter from EventManager.BatTimeChanged.
///
/// Deliberately simpler than BloodBarUI. Bat time is a resource you spend and refill
/// constantly, so it wants to track the true value tightly. A trailing chip bar or a
/// damage blink would just add noise to something that moves every frame anyway.
/// </summary>
public class BatMeterUI : MonoBehaviour
{
    [Header("Display")]
    [Tooltip("BatSlider. Min/max forced to 0 and 1, interaction switched off.")]
    [SerializeField] private Slider _slider;

    [Tooltip("Alternative to the slider. Image Type must be Filled.")]
    [SerializeField] private Image _fill;

    [Header("Behaviour")]
    [Tooltip("Seconds to catch up to the true value. 0 tracks it exactly.")]
    [SerializeField] private float _smoothing = 0.05f;

    [Tooltip("Hide the meter when bat time is full, so the HUD is quiet until the resource matters.")]
    [SerializeField] private bool _hideWhenFull;

    [Header("Colour")]
    [SerializeField] private bool _recolourWhenLow = true;
    [SerializeField] private Color _normalColour = new Color(0.55f, 0.4f, 0.85f, 1f);
    [SerializeField] private Color _lowColour = new Color(0.95f, 0.3f, 0.3f, 1f);

    [Range(0f, 1f)][SerializeField] private float _lowThreshold = 0.25f;

    private float _target = 1f;
    private float _displayed = 1f;
    private Graphic _fillGraphic;
    private CanvasGroup _group;

    private void Awake()
    {
        if (_slider == null && _fill == null)
        {
            Debug.LogError("[BatMeterUI] No slider or fill image assigned, so nothing will display.", this);
            enabled = false;
            return;
        }

        if (_slider != null)
        {
            _slider.minValue = 0f;
            _slider.maxValue = 1f;

            // An interactable slider would let the player drag their own flight time,
            // and it would steal clicks from anything underneath.
            _slider.interactable = false;
            if (_slider.targetGraphic != null) _slider.targetGraphic.raycastTarget = false;
            if (_slider.fillRect != null) _fillGraphic = _slider.fillRect.GetComponent<Graphic>();
        }

        if (_fill != null) _fillGraphic = _fill;

        if (_hideWhenFull)
        {
            _group = GetComponent<CanvasGroup>();
            if (_group == null) _group = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void OnEnable()
    {
        EventManager.BatTimeChanged += OnBatTimeChanged;
    }

    private void OnDisable()
    {
        EventManager.BatTimeChanged -= OnBatTimeChanged;
    }

    private void OnBatTimeChanged(float normalised)
    {
        _target = Mathf.Clamp01(normalised);
    }

    private void Update()
    {
        // Unscaled, so the meter settles rather than freezing mid animation under pause.
        float dt = Time.unscaledDeltaTime;

        _displayed = _smoothing <= 0f
            ? _target
            : Mathf.MoveTowards(_displayed, _target, dt / _smoothing);

        if (_slider != null) _slider.value = _displayed;
        if (_fill != null) _fill.fillAmount = _displayed;

        if (_recolourWhenLow && _fillGraphic != null)
        {
            _fillGraphic.color = _displayed <= _lowThreshold ? _lowColour : _normalColour;
        }

        if (_group != null) _group.alpha = _displayed >= 0.999f ? 0f : 1f;
    }
}
