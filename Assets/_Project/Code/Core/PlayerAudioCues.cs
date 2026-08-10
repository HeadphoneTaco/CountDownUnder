using UnityEngine;
using Core;

/// <summary>
/// Turns player damage, eating, and death events into sound. Purely a listener, so audio can be
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

    [Header("Eating")]
    [Tooltip("Played once the moment the bite lands.")]
    [SerializeField] private AudioClip[] _eatStart;

    [Tooltip("Odds of the bite effect playing. 1 is every bite, 0.5 is roughly every other one.")]
    [Range(0f, 1f)][SerializeField] private float _eatStartChance = 0.5f;

    [Tooltip("Loops for as long as draining continues. Wants a clip that tiles cleanly, such as the sucking effect.")]
    [SerializeField] private AudioClip _eatLoop;

    [Tooltip("The victim's reaction when they are drained dry.")]
    [SerializeField] private AudioClip[] _victimDeath;

    [Tooltip("Satisfied line after finishing a victim. Fires on Eat Voice Chance.")]
    [SerializeField] private AudioClip[] _eatVoice;

    [Range(0f, 1f)][SerializeField] private float _eatVoiceChance = 0.6f;

    [Tooltip("Seconds after the victim dies before the line, so it does not land on top of the scream.")]
    [SerializeField] private float _eatVoiceDelay = 0.5f;

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
        EventManager.PlayerEatStarted += OnEatStarted;
        EventManager.PlayerEatEnded += OnEatEnded;
        EventManager.VictimDrained += OnVictimDrained;
    }

    private void OnDisable()
    {
        EventManager.PlayerHurt -= OnHurt;
        EventManager.PlayerHealthChange -= OnHealthChanged;
        EventManager.PlayerDied -= OnDied;
        EventManager.PlayerEatStarted -= OnEatStarted;
        EventManager.PlayerEatEnded -= OnEatEnded;
        EventManager.VictimDrained -= OnVictimDrained;

        // Leaving the scene mid bite would otherwise strand the loop playing forever,
        // since the manager outlives this object. InstanceIfExists rather than Instance,
        // because the creating getter would spawn a manager during teardown.
        if (AudioManager.InstanceIfExists != null) AudioManager.InstanceIfExists.StopLoop();
    }

    private void OnEatStarted()
    {
        AudioManager audio = AudioManager.Instance;
        if (audio == null) return;

        if (Random.value <= _eatStartChance) audio.PlayRandomSound(_eatStart);
        audio.PlayLoop(_eatLoop);
    }

    private void OnEatEnded()
    {
        if (AudioManager.InstanceIfExists != null) AudioManager.InstanceIfExists.StopLoop();
    }

    private void OnVictimDrained()
    {
        AudioManager audio = AudioManager.Instance;
        if (audio == null) return;

        audio.PlayRandomSound(_victimDeath);

        // The satisfied line waits, so it lands after the victim's reaction rather than
        // talking over the top of it.
        if (_eatVoice != null && _eatVoice.Length > 0 && Random.value <= _eatVoiceChance)
        {
            StartCoroutine(PlayVoiceAfter(_eatVoiceDelay, _eatVoice));
        }
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
            StartCoroutine(PlayVoiceAfter(_deathVoiceDelay, _deathVoice));
        }
    }

    private System.Collections.IEnumerator PlayVoiceAfter(float delay, AudioClip[] clips)
    {
        // Unscaled, because a sequence that freezes time would otherwise swallow the line.
        yield return new WaitForSecondsRealtime(delay);

        AudioManager audio = AudioManager.Instance;
        if (audio != null) audio.PlayRandomVoice(clips);
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
