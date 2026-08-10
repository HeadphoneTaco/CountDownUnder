using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// Volume sliders, saved to PlayerPrefs so the choice survives a restart.
///
/// If an AudioMixer is assigned the values go through exposed parameters, which is
/// the setup you want long term. Without one it falls back to raising a
/// VolumeChanged event and setting AudioListener.volume from the BGM slider, which
/// is enough to make the sliders do something visible today.
///
/// Sliders are linear 0 to 1 because that is what a player expects to see, but mixer
/// volume is in decibels, so the value is converted with a log curve on the way in.
/// A linear slider wired straight to decibels feels dead across most of its travel.
/// </summary>
public class SettingsPanelUI : MonoBehaviour
{
    private const string BgmKey = "CDU_BGMVolume";
    private const string SfxKey = "CDU_SFXVolume";

    [Header("Sliders")]
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _sfxSlider;

    [Header("Mixer (optional)")]
    [Tooltip("Leave empty until an AudioMixer exists. The sliders still work and still save.")]
    [SerializeField] private AudioMixer _mixer;

    [SerializeField] private string _bgmExposedParam = "BGMVolume";
    [SerializeField] private string _sfxExposedParam = "SFXVolume";

    [Header("Defaults")]
    [Range(0f, 1f)][SerializeField] private float _defaultBgm = 0.7f;
    [Range(0f, 1f)][SerializeField] private float _defaultSfx = 0.8f;

    private float _bgm;
    private float _sfx;

    private void Awake()
    {
        _bgm = PlayerPrefs.GetFloat(BgmKey, _defaultBgm);
        _sfx = PlayerPrefs.GetFloat(SfxKey, _defaultSfx);

        if (_bgmSlider != null)
        {
            _bgmSlider.minValue = 0f;
            _bgmSlider.maxValue = 1f;
            _bgmSlider.SetValueWithoutNotify(_bgm);
            _bgmSlider.onValueChanged.AddListener(SetBgmVolume);
        }

        if (_sfxSlider != null)
        {
            _sfxSlider.minValue = 0f;
            _sfxSlider.maxValue = 1f;
            _sfxSlider.SetValueWithoutNotify(_sfx);
            _sfxSlider.onValueChanged.AddListener(SetSfxVolume);
        }

        ApplyAll();
    }

    private void OnDestroy()
    {
        if (_bgmSlider != null) _bgmSlider.onValueChanged.RemoveListener(SetBgmVolume);
        if (_sfxSlider != null) _sfxSlider.onValueChanged.RemoveListener(SetSfxVolume);
    }

    public void SetBgmVolume(float value)
    {
        _bgm = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(BgmKey, _bgm);
        ApplyAll();
    }

    public void SetSfxVolume(float value)
    {
        _sfx = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(SfxKey, _sfx);
        ApplyAll();
    }

    public void ResetToDefaults()
    {
        SetBgmVolume(_defaultBgm);
        SetSfxVolume(_defaultSfx);
        if (_bgmSlider != null) _bgmSlider.SetValueWithoutNotify(_bgm);
        if (_sfxSlider != null) _sfxSlider.SetValueWithoutNotify(_sfx);
    }

    private void ApplyAll()
    {
        if (_mixer != null)
        {
            _mixer.SetFloat(_bgmExposedParam, LinearToDecibels(_bgm));
            _mixer.SetFloat(_sfxExposedParam, LinearToDecibels(_sfx));
        }
        else if (Core.AudioManager.Instance != null)
        {
            // Per channel, so the two sliders actually do different things.
            Core.AudioManager.Instance.SetBgmVolume(_bgm);
            Core.AudioManager.Instance.SetSfxVolume(_sfx);
        }
        else
        {
            // Blunt last resort when there is no AudioManager in the scene at all.
            AudioListener.volume = _bgm;
        }

        PlayerPrefs.Save();
        EventManager.VolumeChanged?.Invoke(_bgm, _sfx);
    }

    /// <summary>0 maps to full mute at -80 dB, 1 maps to 0 dB.</summary>
    private static float LinearToDecibels(float linear)
    {
        if (linear <= 0.0001f) return -80f;
        return Mathf.Log10(linear) * 20f;
    }

    public static float SavedBgm => PlayerPrefs.GetFloat(BgmKey, 0.7f);
    public static float SavedSfx => PlayerPrefs.GetFloat(SfxKey, 0.8f);
}
