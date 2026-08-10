using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Scene routing for the menu screens. Put one of these on the Canvas of any menu
/// scene and point button OnClick events at its public methods.
///
/// Every method is public, returns void, and takes either no arguments or a single
/// string, which is what the Inspector's OnClick dropdown is able to call. A method
/// with two arguments will not appear in that list, which is the usual reason a
/// button "has no script to pick".
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Scene Names")]
    [Tooltip("Loaded by Play. Must match the scene name in File > Build Settings exactly.")]
    [SerializeField] private string _gameSceneName = "Game";

    [SerializeField] private string _settingsSceneName = "SettingsMenu";
    [SerializeField] private string _mainMenuSceneName = "MainMenu";

    [Header("Buttons")]
    [Tooltip("Drag the button objects here and leave their OnClick lists empty. " +
             "A button prefab cannot store a reference to something outside itself, so wiring OnClick " +
             "in Prefab Mode keeps the method name but silently drops the target.")]
    [SerializeField] private Button _playButton;

    [Tooltip("End screens. Does the same thing as Play, named for what it means there.")]
    [SerializeField] private Button _replayButton;

    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _mainMenuButton;
    [SerializeField] private Button _quitButton;

    private void Awake()
    {
        // If the player quit to menu out of a paused game, timeScale is still zero and
        // every animation on this screen would sit frozen. Menus always run at full speed.
        Time.timeScale = 1f;
        AudioListener.pause = false;

        Hook(_playButton, PlayGame);
        Hook(_replayButton, PlayGame);
        Hook(_settingsButton, OpenSettings);
        Hook(_mainMenuButton, BackToMainMenu);
        Hook(_quitButton, QuitGame);
    }

    private void OnDestroy()
    {
        Unhook(_playButton, PlayGame);
        Unhook(_replayButton, PlayGame);
        Unhook(_settingsButton, OpenSettings);
        Unhook(_mainMenuButton, BackToMainMenu);
        Unhook(_quitButton, QuitGame);
    }

    private static void Hook(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null) button.onClick.AddListener(action);
    }

    private static void Unhook(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null) button.onClick.RemoveListener(action);
    }

    // Button hooks.

    public void PlayGame() => Load(_gameSceneName);

    public void OpenSettings() => Load(_settingsSceneName);

    public void BackToMainMenu() => Load(_mainMenuSceneName);

    /// <summary>For buttons that want to name their own destination in the Inspector.</summary>
    public void LoadSceneByName(string sceneName) => Load(sceneName);

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void Load(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("[MainMenuUI] A scene name field is empty, so this button has nowhere to go.", this);
            return;
        }

        // A scene missing from Build Settings throws a raw exception that says very little
        // about which button caused it. Checking first turns that into a readable message.
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"[MainMenuUI] Scene '{sceneName}' is not in Build Settings, or the name is misspelled. " +
                           "Open File > Build Settings and add it. Use the scene name only, no path and no .unity.", this);
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}
