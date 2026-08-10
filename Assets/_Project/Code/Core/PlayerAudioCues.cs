using UnityEngine;
using Core;

/// <summary>
/// Turns player damage and death events into sound. Purely a listener, so audio can be
/// added, removed, or retimed without touching gameplay code.
///
/// Put this in the game scene alongside the AudioManager. Leave any clip array empty and
/// that cue simply does not fire.
/// </summary>
public class PlayerAudioCues : MonoBehaviour
{
    [Header("Player Hurt")]
    [Tooltip("Impact effect. Picked at random when several are supplied.")]
    [SerializeField] private AudioClip[] _hurtImpact;

    [Tooltip("Voice line on being hit. Fires only sometimes, see Hurt Voice Chance.")]
    [SerializeField] private AudioClip[] _hurtVoice;

    [Tooltip("Odds of a voice line accompanying a hit. A line on every single hit gets grating fast.")]
    [Range(0f, 1f)][SerializeField] private float _hurtVoiceChance = 0.35f;

    [Tooltip("Minimum seconds between voice lines, so rapid hits cannot stutter them.")]
    [SerializeField] private float _voiceCooldown = 2f;

    [Header("Low Blood")]
    [Tooltip("Plays once when blood first drops below the threshold, not repeatedly while low.")]
    [SerializeField] private AudioClip[] _lowBloodVoice;

    [Range(0f, 1f)][SerializeField] private float _lowBloodThreshold = 0.25f;

    [Header("Death")]
    [SerializeField] private AudioClip[] _deathSting;
    [SerializeField] private AudioClip[] _deathVoice;

    [Tooltip("Seconds after the death sting before the voice line, so they do not collide.")]
    [SerializeField] private float _deathVoiceDelay = 0.6f;

    private float _lastVoiceTime = -999f;
    private bool _lowBloodAnnounced;
    private bool _dead;

    private void OnEnable()
    {
        EventManager.PlayerHurt += OnHurt;
        EventManager.PlayerHealthChange += OnHealthChanged;
        EventManager.PlayerDied += OnDied;
    }

    private void OnDisable()
    {
        EventManager.PlayerHurt -= OnHurt;
        EventManager.PlayerHealthChange -= OnHealthChanged;
        EventManager.PlayerDied -= OnDied;
    }

    private void OnHurt()
    {
        if (_dead) return;

        AudioManager audio = AudioManager.Instance;
        if (audio == null) return;

        audio.PlayRandomSound(_hurtImpact);

        if (Random.value <= _hurtVoiceChance) TryPlayVoice(_hurtVoice);
    }

    private void OnHealthChanged(float normalised)
    {
        // Latched so the line plays on the way down and re-arms only after healing back
        // up. Without the latch every subsequent hit while low would retrigger it.
        if (normalised <= _lowBloodThreshold && normalised > 0f)
        {
            if (!_lowBloodAnnounced)
            {
                _lowBloodAnnounced = true;
                TryPlayVoice(_lowBloodVoice);
            }
        }
        else if (normalised > _lowBloodThreshold)
        {
            _lowBloodAnnounced = false;
        }
    }

    private void OnDied()
    {
        if (_dead) return;
        _dead = true;

        AudioManager audio = AudioManager.Instance;
        if (audio == null) return;

        audio.PlayRandomSound(_deathSting);

        if (_deathVoice != null && _deathVoice.Length > 0)
        {
            // Unscaled, because a death sequence that freezes time would otherwise
            // swallow the line entirely.
            StartCoroutine(PlayVoiceAfter(_deathVoiceDelay));
        }
    }

    private System.Collections.IEnumerator PlayVoiceAfter(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        AudioManager audio = AudioManager.Instance;
        if (audio != null) audio.PlayRandomVoice(_deathVoice);
    }

    private void TryPlayVoice(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return;
        if (Time.unscaledTime - _lastVoiceTime < _voiceCooldown) return;

        AudioManager audio = AudioManager.Instance;
        if (audio == null) return;

        _lastVoiceTime = Time.unscaledTime;
        audio.PlayRandomVoice(clips);
    }
}
