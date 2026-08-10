using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The "ouch" layer. A full screen image, normally invisible, that stabs to full
/// opacity on EventManager.PlayerHurt and fades back out.
///
/// Separate from the blood bar on purpose. The bar answers "how much have I got
/// left", this answers "something just hit me" without asking the player to look
/// away from their character.
///
/// The image wants to be a soft edged vignette rather than a flat rectangle. A flat
/// red wash over the whole screen hides the obstacle that just hit you, which is
/// the one thing the player needs to see in that moment.
/// </summary>
public class DamageFlashUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Full screen vignette Image. Raycast Target should be OFF or it eats every button click.")]
    [SerializeField] private Image _flashImage;

    [Header("Hit Flash")]
    [Tooltip("Alpha the flash jumps to the instant a hit lands.")]
    [Range(0f, 1f)][SerializeField] private float _peakAlpha = 0.75f;

    [Tooltip("Seconds it holds at peak before fading. Keep this short, it is a punch not a curtain.")]
    [SerializeField] private float _holdDuration = 0.05f;

    [Tooltip("Seconds to fade from peak back to the resting level.")]
    [SerializeField] private float _fadeDuration = 0.4f;

    [SerializeField] private Color _flashColour = new Color(0.85f, 0.02f, 0.06f, 1f);

    [Header("Low Blood Throb")]
    [Tooltip("Keep a faint pulse on screen while blood is low, so danger is legible even between hits.")]
    [SerializeField] private bool _throbWhenLow = true;

    [Range(0f, 1f)][SerializeField] private float _throbThreshold = 0.25f;
    [Range(0f, 1f)][SerializeField] private float _throbMaxAlpha = 0.22f;
    [SerializeField] private float _throbRate = 1.4f;

    private float _hitAlpha;
    private float _holdTimer;
    private float _health = 1f;

    private void Awake()
    {
        if (_flashImage == null)
        {
            Debug.LogError("[DamageFlashUI] No flash image assigned.", this);
            enabled = false;
            return;
        }

        // A raycast blocking full screen image is the classic "none of my buttons work"
        // bug, and it is invisible in the Game view because the image is transparent.
        if (_flashImage.raycastTarget)
        {
            Debug.LogWarning($"[DamageFlashUI] '{_flashImage.name}' has Raycast Target on. It covers the screen, " +
                             "so it will swallow every UI click. Turning it off.", _flashImage);
            _flashImage.raycastTarget = false;
        }

        SetAlpha(0f);
    }

    private void OnEnable()
    {
        EventManager.PlayerHurt += OnHurt;
        EventManager.PlayerHealthChange += OnHealthChanged;
    }

    private void OnDisable()
    {
        EventManager.PlayerHurt -= OnHurt;
        EventManager.PlayerHealthChange -= OnHealthChanged;
    }

    private void OnHurt()
    {
        _hitAlpha = _peakAlpha;
        _holdTimer = _holdDuration;
    }

    private void OnHealthChanged(float normalised)
    {
        _health = Mathf.Clamp01(normalised);
    }

    private void Update()
    {
        float dt = Time.unscaledDeltaTime;

        if (_holdTimer > 0f)
        {
            _holdTimer -= dt;
        }
        else if (_hitAlpha > 0f && _fadeDuration > 0f)
        {
            _hitAlpha = Mathf.MoveTowards(_hitAlpha, 0f, (_peakAlpha / _fadeDuration) * dt);
        }
        else
        {
            _hitAlpha = 0f;
        }

        // The hit flash and the throb are combined with Max rather than added, so a hit
        // taken at low blood still reads as a distinct spike instead of clipping to solid red.
        SetAlpha(Mathf.Max(_hitAlpha, ThrobAlpha()));
    }

    private float ThrobAlpha()
    {
        if (!_throbWhenLow || _health > _throbThreshold || _throbThreshold <= 0f) return 0f;

        // Deeper into the red means a stronger throb.
        float severity = 1f - (_health / _throbThreshold);
        float pulse = (Mathf.Sin(Time.unscaledTime * _throbRate * Mathf.PI * 2f) + 1f) * 0.5f;
        return _throbMaxAlpha * severity * pulse;
    }

    private void SetAlpha(float alpha)
    {
        Color c = _flashColour;
        c.a = alpha;
        _flashImage.color = c;
        _flashImage.enabled = alpha > 0.001f;
    }
}
