using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Ends the run. Listens for death or victory, holds for a beat so the moment lands and
/// the audio cue can finish, fades the player out, then loads the matching end screen.
///
/// Put one in the game scene. It owns nothing about why the run ended, only what happens
/// afterwards, so the win and lose paths stay identical apart from the scene name.
/// </summary>
public class RunEndController : MonoBehaviour
{
    [Header("Scenes")]
    [Tooltip("Loaded when the player runs out of blood. Must be in Build Settings.")]
    [SerializeField] private string _deathSceneName = "EndScreenA";

    [Tooltip("Loaded when the player reaches the win trigger. EndScreenC is the airport ending.")]
    [SerializeField] private string _winSceneName = "EndScreenC";

    [Header("Timing")]
    [Tooltip("Seconds to hold on the game before the end screen loads.")]
    [SerializeField] private float _deathDelay = 2f;

    [SerializeField] private float _winDelay = 1.5f;

    [Header("Death Visual")]
    [Tooltip("Fade the player's sprites out during the delay. Stands in until there is a death animation.")]
    [SerializeField] private bool _fadePlayerOnDeath = true;

    [Tooltip("Seconds the fade takes. Kept under the death delay or it gets cut off.")]
    [SerializeField] private float _fadeDuration = 1.2f;

    [Tooltip("Leave empty to collect from the PlayerController in the scene, skipping anything under a Camera.")]
    [SerializeField] private SpriteRenderer[] _playerSprites;

    private bool _ending;

    private void OnEnable()
    {
        EventManager.PlayerDied += OnPlayerDied;
        EventManager.PlayerWon += OnPlayerWon;
    }

    private void OnDisable()
    {
        EventManager.PlayerDied -= OnPlayerDied;
        EventManager.PlayerWon -= OnPlayerWon;
    }

    private void OnPlayerDied() => EndRun(_deathSceneName, _deathDelay, fade: _fadePlayerOnDeath);

    private void OnPlayerWon() => EndRun(_winSceneName, _winDelay, fade: false);

    private void EndRun(string sceneName, float delay, bool fade)
    {
        // Whichever ending arrives first wins. A win trigger crossed on the same frame as
        // the last hit would otherwise start two transitions.
        if (_ending) return;
        _ending = true;

        // No pausing your way out of an ending that is already underway.
        if (PauseManager.InstanceIfExists != null) PauseManager.InstanceIfExists.SetPauseAllowed(false);

        StartCoroutine(EndRunRoutine(sceneName, delay, fade));
    }

    private IEnumerator EndRunRoutine(string sceneName, float delay, bool fade)
    {
        if (fade) yield return StartCoroutine(FadePlayer());

        float remaining = delay - (fade ? _fadeDuration : 0f);
        if (remaining > 0f) yield return new WaitForSecondsRealtime(remaining);

        if (string.IsNullOrWhiteSpace(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"[RunEndController] Scene '{sceneName}' is not in Build Settings, so the run cannot end. " +
                           "Add it under File > Build Settings.", this);
            yield break;
        }

        // Reset in case the run ended while paused, otherwise the end screen inherits a
        // frozen timeScale and sits still.
        Time.timeScale = 1f;
        AudioListener.pause = false;

        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FadePlayer()
    {
        SpriteRenderer[] sprites = ResolveSprites();
        if (sprites.Length == 0 || _fadeDuration <= 0f) yield break;

        var startColours = new Color[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            startColours[i] = sprites[i] != null ? sprites[i].color : Color.white;
        }

        float elapsed = 0f;
        while (elapsed < _fadeDuration)
        {
            // Unscaled, so a death that also stops time still fades.
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / _fadeDuration);

            for (int i = 0; i < sprites.Length; i++)
            {
                if (sprites[i] == null) continue;

                Color c = startColours[i];
                c.a = startColours[i].a * (1f - t);
                sprites[i].color = c;
            }

            yield return null;
        }
    }

    private SpriteRenderer[] ResolveSprites()
    {
        if (_playerSprites != null && _playerSprites.Length > 0) return _playerSprites;

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player == null) return new SpriteRenderer[0];

        // Same exclusion as the invincibility flicker: the Main Camera is a child of the
        // player and carries a full screen backdrop that must not be faded with it.
        var kept = new List<SpriteRenderer>();
        foreach (SpriteRenderer r in player.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (!IsUnderCamera(r.transform)) kept.Add(r);
        }

        _playerSprites = kept.ToArray();
        return _playerSprites;
    }

    private static bool IsUnderCamera(Transform t)
    {
        for (Transform p = t; p != null; p = p.parent)
        {
            if (p.GetComponent<Camera>() != null) return true;
        }
        return false;
    }
}
