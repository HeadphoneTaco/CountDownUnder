using System.Collections;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Owns the game's AudioSources and the volume settings applied to them.
    ///
    /// Three channels plus voice:
    ///   sfx      - one shot effects, layered freely
    ///   bg       - music playlist
    ///   ambient  - looping atmosphere playlist
    ///   voice    - character lines, deliberately not layered so two lines never talk over each other
    ///
    /// Volume is stored as a linear 0 to 1 per channel and persisted, so it survives a
    /// scene change and a restart without needing an AudioMixer asset.
    /// </summary>
    public class AudioManager : Singleton<AudioManager>
    {
        private const string BgmKey = "CDU_BGMVolume";
        private const string SfxKey = "CDU_SFXVolume";

        public AudioSource sfxAudioSource;
        public AudioSource bgAudioSource;
        public AudioSource ambientAudioSource;

        [Tooltip("Optional. Character lines play here so a new line cuts the previous one instead of overlapping.")]
        public AudioSource voiceAudioSource;

        [Tooltip("Optional. Sustained effects that run for as long as an action lasts, such as feeding.")]
        public AudioSource loopAudioSource;

        [Tooltip("Trim for character lines relative to effects. The recordings sit quieter than the " +
                 "sourced SFX, so this lifts them without touching the SFX slider.")]
        [Range(0f, 3f)] public float voiceGain = 1.4f;

        public AudioClip[] bgMusic;
        public AudioClip[] ambientAudio;

        [Header("Startup")]
        [Tooltip("Start the music playlist automatically. Since this object survives scene loads, " +
                 "the track carries on from the menu into the game rather than restarting.")]
        [SerializeField] private bool _playMusicOnStart = true;

        [SerializeField] private bool _playAmbientOnStart;

        private int _bgMusicIndex;
        private int _ambientAudioIndex;
        private Coroutine _bgMusicCoroutine;
        private Coroutine _ambientAudioCoroutine;

        private float _bgmVolume = 0.7f;
        private float _sfxVolume = 0.8f;

        public float BgmVolume => _bgmVolume;
        public float SfxVolume => _sfxVolume;

        // Lives in the main menu and carries through every scene, so there is one music
        // playlist and one set of volume settings for the whole session.
        protected override bool PersistAcrossScenes => true;

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this) return;

            EnsureSources();

            _bgmVolume = PlayerPrefs.GetFloat(BgmKey, 0.7f);
            _sfxVolume = PlayerPrefs.GetFloat(SfxKey, 0.8f);
            ApplyVolumes();
        }

        private void Start()
        {
            if (Instance != this) return;
            if (_playMusicOnStart) PlayBgMusic();
            if (_playAmbientOnStart) PlayAmbientAudio();
        }

        /// <summary>
        /// Creates any AudioSource that was not assigned. Means the manager still works
        /// when it gets auto-created by the Singleton, which happens whenever someone
        /// hits play straight into the game scene instead of starting from the menu.
        /// </summary>
        private void EnsureSources()
        {
            sfxAudioSource = sfxAudioSource != null ? sfxAudioSource : CreateSource("SFX");
            bgAudioSource = bgAudioSource != null ? bgAudioSource : CreateSource("Music", loop: false);
            ambientAudioSource = ambientAudioSource != null ? ambientAudioSource : CreateSource("Ambient");
            voiceAudioSource = voiceAudioSource != null ? voiceAudioSource : CreateSource("Voice");
            loopAudioSource = loopAudioSource != null ? loopAudioSource : CreateSource("Loop", loop: true);
        }

        private AudioSource CreateSource(string sourceName, bool loop = false)
        {
            GameObject go = new GameObject(sourceName);
            go.transform.SetParent(transform, false);

            AudioSource source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;

            // 2D. A positioned source would pan menu music around based on where this
            // object happens to sit in the scene.
            source.spatialBlend = 0f;
            return source;
        }

        // Playback

        public void PlaySound(AudioClip clip)
        {
            if (clip == null) return;
            if (sfxAudioSource == null)
            {
                Debug.LogWarning("[AudioManager] No sfxAudioSource assigned, so no effects can play.", this);
                return;
            }

            sfxAudioSource.PlayOneShot(clip, _sfxVolume);
        }

        /// <summary>Plays one clip at random. Used for the v1/v2/v3 line variants so repeats are less obvious.</summary>
        public void PlayRandomSound(AudioClip[] clips)
        {
            AudioClip clip = PickRandom(clips);
            if (clip != null) PlaySound(clip);
        }

        /// <summary>
        /// Character lines go through their own source and interrupt each other. Two
        /// vampire quips talking over one another sounds like a bug, not a chorus.
        /// </summary>
        public void PlayVoice(AudioClip clip, bool interrupt = true)
        {
            if (clip == null) return;

            AudioSource source = voiceAudioSource != null ? voiceAudioSource : sfxAudioSource;
            if (source == null) return;

            if (!interrupt && source.isPlaying) return;

            source.Stop();
            source.clip = clip;
            source.volume = Mathf.Clamp01(_sfxVolume * voiceGain);
            source.Play();
        }

        /// <summary>
        /// Starts a sustained effect that runs until StopLoop. Re-calling with the clip
        /// already playing is ignored, so this is safe to call every frame from a state.
        /// </summary>
        public void PlayLoop(AudioClip clip)
        {
            if (clip == null || loopAudioSource == null) return;
            if (loopAudioSource.clip == clip && loopAudioSource.isPlaying) return;

            loopAudioSource.clip = clip;
            loopAudioSource.loop = true;
            loopAudioSource.volume = _sfxVolume;
            loopAudioSource.Play();
        }

        public void StopLoop()
        {
            if (loopAudioSource != null) loopAudioSource.Stop();
        }

        public void PlayRandomVoice(AudioClip[] clips, bool interrupt = true)
        {
            AudioClip clip = PickRandom(clips);
            if (clip != null) PlayVoice(clip, interrupt);
        }

        private static AudioClip PickRandom(AudioClip[] clips)
        {
            if (clips == null || clips.Length == 0) return null;
            if (clips.Length == 1) return clips[0];
            return clips[Random.Range(0, clips.Length)];
        }

        // Volume

        public void SetBgmVolume(float linear01)
        {
            _bgmVolume = Mathf.Clamp01(linear01);
            PlayerPrefs.SetFloat(BgmKey, _bgmVolume);
            ApplyVolumes();
        }

        public void SetSfxVolume(float linear01)
        {
            _sfxVolume = Mathf.Clamp01(linear01);
            PlayerPrefs.SetFloat(SfxKey, _sfxVolume);
            ApplyVolumes();
        }

        private void ApplyVolumes()
        {
            if (bgAudioSource != null) bgAudioSource.volume = _bgmVolume;
            if (ambientAudioSource != null) ambientAudioSource.volume = _bgmVolume;
            if (voiceAudioSource != null) voiceAudioSource.volume = Mathf.Clamp01(_sfxVolume * voiceGain);
            if (loopAudioSource != null) loopAudioSource.volume = _sfxVolume;

            // sfxAudioSource is intentionally left alone. PlayOneShot takes its own
            // volume scale, so setting the source volume as well would square it.
            PlayerPrefs.Save();
        }

        // Playlists

        public void PlayBgMusic()
        {
            if (!PlaylistUsable(bgAudioSource, bgMusic, "bgMusic")) return;

            if (_bgMusicCoroutine != null) StopCoroutine(_bgMusicCoroutine);
            _bgMusicCoroutine = StartCoroutine(BgMusicPlaylist());
        }

        public void PlayAmbientAudio()
        {
            if (!PlaylistUsable(ambientAudioSource, ambientAudio, "ambientAudio")) return;

            if (_ambientAudioCoroutine != null) StopCoroutine(_ambientAudioCoroutine);
            _ambientAudioCoroutine = StartCoroutine(AmbientAudioPlaylist());
        }

        /// <summary>
        /// An empty clip array or a missing source used to throw inside the coroutine,
        /// where the stack trace points at the playlist rather than at the empty field
        /// that actually caused it.
        /// </summary>
        private bool PlaylistUsable(AudioSource source, AudioClip[] clips, string fieldName)
        {
            if (source == null)
            {
                Debug.LogWarning($"[AudioManager] No AudioSource assigned for {fieldName}.", this);
                return false;
            }

            if (clips == null || clips.Length == 0)
            {
                Debug.LogWarning($"[AudioManager] {fieldName} is empty, so there is nothing to play.", this);
                return false;
            }

            return true;
        }

        private IEnumerator BgMusicPlaylist()
        {
            while (true)
            {
                AudioClip clip = bgMusic[_bgMusicIndex];
                if (clip != null)
                {
                    bgAudioSource.clip = clip;
                    bgAudioSource.Play();

                    // Waiting on isPlaying rather than on the clip length keeps the
                    // playlist in step with the audio itself. PauseManager freezes the
                    // AudioListener, and a timer would keep running through that and
                    // skip to the next track early.
                    yield return null;
                    yield return new WaitWhile(() => bgAudioSource.isPlaying);
                }
                else
                {
                    yield return null;
                }

                _bgMusicIndex = (_bgMusicIndex + 1) % bgMusic.Length;
            }
        }

        private IEnumerator AmbientAudioPlaylist()
        {
            while (true)
            {
                AudioClip clip = ambientAudio[_ambientAudioIndex];
                if (clip != null)
                {
                    ambientAudioSource.clip = clip;
                    ambientAudioSource.Play();
                    yield return null;
                    yield return new WaitWhile(() => ambientAudioSource.isPlaying);
                }
                else
                {
                    yield return null;
                }

                _ambientAudioIndex = (_ambientAudioIndex + 1) % ambientAudio.Length;
            }
        }
    }
}
