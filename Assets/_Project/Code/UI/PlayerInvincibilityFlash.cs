using UnityEngine;

/// <summary>
/// Blinks the player while their invincibility frames are running, so the window where
/// hits do nothing is visible rather than something the player has to infer.
///
/// Reads PlayerController.IsInvincible directly instead of running its own timer. Tying
/// the visual to the same value the damage check uses means the blink cannot drift out
/// of step with the mechanic when InvincibilityTime is retuned.
///
/// Alpha is modulated rather than toggling the renderer off, because the person and bat
/// forms switch their objects on and off during transformation. Fighting over enabled
/// would make the player vanish at the wrong moment.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerInvincibilityFlash : MonoBehaviour
{
    [Tooltip("Blinks per second. Around 8 to 12 reads as an arcade style flicker.")]
    [SerializeField] private float _blinksPerSecond = 10f;

    [Tooltip("Alpha at the dim end of the blink. 0 is a hard flicker, 0.3 is a softer pulse.")]
    [Range(0f, 1f)][SerializeField] private float _dimAlpha = 0.25f;

    [Tooltip("Tint applied while invincible. Leave white to only change opacity.")]
    [SerializeField] private Color _tint = Color.white;

    [Tooltip("Include renderers on inactive children, so the bat form is covered too.")]
    [SerializeField] private bool _includeInactive = true;

    [Tooltip("The sprites to blink. Leave empty to auto-collect from this object and its children. " +
             "Assign explicitly if auto-collection picks up something it should not.")]
    [SerializeField] private SpriteRenderer[] _renderers;

    private PlayerController _player;
    private Color[] _originalColours;
    private bool _wasInvincible;

    private void Awake()
    {
        _player = GetComponent<PlayerController>();

        if (_renderers == null || _renderers.Length == 0) _renderers = AutoCollect();

        if (_renderers.Length == 0)
        {
            Debug.LogWarning("[PlayerInvincibilityFlash] No SpriteRenderers to blink.", this);
            enabled = false;
            return;
        }

        _originalColours = new Color[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
        {
            _originalColours[i] = _renderers[i].color;
        }
    }

    /// <summary>
    /// Collects the player's own sprites, skipping anything parented under a Camera.
    /// The Main Camera is a child of the player here and carries a full screen backdrop
    /// quad, so a plain GetComponentsInChildren sweep would blink the background as well.
    /// </summary>
    private SpriteRenderer[] AutoCollect()
    {
        SpriteRenderer[] all = GetComponentsInChildren<SpriteRenderer>(_includeInactive);
        var kept = new System.Collections.Generic.List<SpriteRenderer>(all.Length);
        var skipped = new System.Collections.Generic.List<string>();

        foreach (SpriteRenderer r in all)
        {
            if (IsUnderCamera(r.transform)) skipped.Add(r.name);
            else kept.Add(r);
        }

        if (skipped.Count > 0)
        {
            Debug.Log($"[PlayerInvincibilityFlash] Blinking {kept.Count} sprite(s). " +
                      $"Skipped {string.Join(", ", skipped)} because they sit under a Camera. " +
                      "Assign the Renderers list by hand if that guess is wrong.", this);
        }

        return kept.ToArray();
    }

    private static bool IsUnderCamera(Transform t)
    {
        for (Transform p = t; p != null; p = p.parent)
        {
            if (p.GetComponent<Camera>() != null) return true;
        }
        return false;
    }

    private void Update()
    {
        bool invincible = _player.IsInvincible;

        if (invincible)
        {
            // Square wave, so the blink is a hard on/off rather than a soft throb.
            bool bright = Mathf.Repeat(Time.time * _blinksPerSecond, 1f) < 0.5f;
            Apply(bright ? 1f : _dimAlpha, _tint);
        }
        else if (_wasInvincible)
        {
            // Restore exactly once, on the frame the window closes, so nothing else that
            // wants to drive sprite colour is fought with for the rest of the run.
            Restore();
        }

        _wasInvincible = invincible;
    }

    private void OnDisable()
    {
        if (_wasInvincible) Restore();
        _wasInvincible = false;
    }

    private void Apply(float alpha, Color tint)
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] == null) continue;

            Color c = _originalColours[i] * tint;
            c.a = _originalColours[i].a * alpha;
            _renderers[i].color = c;
        }
    }

    private void Restore()
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] != null) _renderers[i].color = _originalColours[i];
        }
    }
}
