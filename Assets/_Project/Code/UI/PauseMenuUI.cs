using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// The pause overlay. Owns which panel is showing and nothing else.
///
/// It does not decide whether the game is paused, PauseManager does. This listens
/// to GamePauseChanged and reacts. Keeping it one directional means an unpause
/// triggered from anywhere, including a scene transition, always closes the menu.
///
/// Buttons are assigned to this script rather than this script being assigned to
/// each button's OnClick. That is deliberate. A button prefab cannot hold a
/// reference to something outside itself, so wiring OnClick in Prefab Mode saves
/// the method name and quietly throws the target away, leaving a button that looks
/// wired and does nothing. Pointing this way keeps every reference inside one
/// prefab, where it serialises correctly.
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    private enum Panel { Root, Settings, Controls }

    [Header("Panels")]
    [Tooltip("The panel that gets shown and hidden. Must be a CHILD of this object, not this object itself.")]
    [SerializeField] private GameObject _pauseRoot;

    [Tooltip("The button column: Resume, Settings, Controls, Main Menu.")]
    [SerializeField] private GameObject _buttonsPanel;

    [Tooltip("Leave empty until a real settings panel exists. Do not put a button here.")]
    [SerializeField] private GameObject _settingsPanel;

    [Tooltip("Leave empty until a real controls panel exists. Do not put a button here.")]
    [SerializeField] private GameObject _controlsPanel;

    [Header("Buttons")]
    [Tooltip("Drag the button objects here. Their OnClick lists can stay empty.")]
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _controlsButton;
    [SerializeField] private Button _mainMenuButton;

    [Header("Scenes")]
    [Tooltip("Scene name for the Main Menu button. Must be in Build Settings.")]
    [SerializeField] private string _mainMenuSceneName = "MainMenu";

    [Header("Authoring")]
    [Tooltip("Switch everything under the pause root back on at startup, so you can freely " +
             "disable pieces in the editor to see the game view without breaking the menu at runtime.")]
    [SerializeField] private bool _restoreChildrenOnStart = true;

    private Panel _current = Panel.Root;

    private void Awake()
    {
        if (_pauseRoot == null)
        {
            Debug.LogError("[PauseMenuUI] No pause root assigned.", this);
            enabled = false;
            return;
        }

        // Hiding the pause menu must not hide this script along with it. If the root is
        // this same object, SetActive(false) switches the component off too, Unity skips
        // OnEnable, and the subscription below never happens.
        if (_pauseRoot == gameObject)
        {
            Debug.LogError($"[PauseMenuUI] Pause Root is '{name}', the same object this script is on. " +
                           "Assign the child panel instead, and leave this object active.", this);
            enabled = false;
            return;
        }

        // A panel field pointed at a Button is almost always a mis-drag. The symptom is
        // that the button vanishes, because showing the root panel deactivates it.
        WarnIfButton(_settingsPanel, "Settings Panel");
        WarnIfButton(_controlsPanel, "Controls Panel");

        // Anything switched off in the editor to get it out of the way stays off at
        // runtime otherwise, which reads as a half drawn menu.
        if (_restoreChildrenOnStart) SetChildrenActive(_pauseRoot.transform);

        _pauseRoot.SetActive(false);

        HookButton(_resumeButton, OnResumePressed);
        HookButton(_settingsButton, OnSettingsPressed);
        HookButton(_controlsButton, OnControlsPressed);
        HookButton(_mainMenuButton, OnQuitToMenuPressed);

        // Subscribed in Awake rather than OnEnable so the listener survives this object
        // being deactivated by something else. Released in OnDestroy to match.
        EventManager.GamePauseChanged += OnPauseChanged;
    }

    private void Start()
    {
        // No EventSystem means Unity processes no UI input at all: no clicks, no hover.
        // The menu still draws, so it looks like a dead button rather than a missing object.
        if (EventSystem.current == null)
        {
            Debug.LogError("[PauseMenuUI] There is no EventSystem in this scene, so no UI can be clicked. " +
                           "Add one with GameObject > UI > Event System.", this);
        }
    }

    private void OnDestroy()
    {
        EventManager.GamePauseChanged -= OnPauseChanged;

        UnhookButton(_resumeButton, OnResumePressed);
        UnhookButton(_settingsButton, OnSettingsPressed);
        UnhookButton(_controlsButton, OnControlsPressed);
        UnhookButton(_mainMenuButton, OnQuitToMenuPressed);
    }

    private void OnPauseChanged(bool paused)
    {
        _pauseRoot.SetActive(paused);
        if (paused) ShowPanel(Panel.Root);
    }

    // Public so they still work if someone prefers wiring OnClick by hand on a scene instance.

    public void OnResumePressed()
    {
        if (PauseManager.Instance != null) PauseManager.Instance.SetPaused(false);
    }

    public void OnSettingsPressed() => ShowPanel(Panel.Settings);

    public void OnControlsPressed() => ShowPanel(Panel.Controls);

    /// <summary>Back out of a sub panel to the button column. Also what Cancel does.</summary>
    public void OnBackPressed()
    {
        if (_current == Panel.Root) OnResumePressed();
        else ShowPanel(Panel.Root);
    }

    public void OnQuitToMenuPressed()
    {
        // Resume before loading, otherwise the menu scene inherits timeScale 0 and
        // every animation there sits still.
        if (PauseManager.Instance != null) PauseManager.Instance.SetPaused(false);

        if (!Application.CanStreamedLevelBeLoaded(_mainMenuSceneName))
        {
            Debug.LogError($"[PauseMenuUI] Scene '{_mainMenuSceneName}' is not in Build Settings.", this);
            return;
        }

        SceneManager.LoadScene(_mainMenuSceneName);
    }

    public void OnQuitGamePressed()
    {
        if (PauseManager.Instance != null) PauseManager.Instance.SetPaused(false);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ShowPanel(Panel panel)
    {
        // With no panel to show, swapping away from the buttons would leave a blank
        // screen the player cannot get out of except by unpausing.
        if (panel == Panel.Settings && _settingsPanel == null) return;
        if (panel == Panel.Controls && _controlsPanel == null) return;

        _current = panel;

        if (_buttonsPanel != null) _buttonsPanel.SetActive(panel == Panel.Root);
        if (_settingsPanel != null) _settingsPanel.SetActive(panel == Panel.Settings);
        if (_controlsPanel != null) _controlsPanel.SetActive(panel == Panel.Controls);

        if (panel == Panel.Settings) WarnIfCollapsed(_settingsPanel);
        if (panel == Panel.Controls) WarnIfCollapsed(_controlsPanel);
    }

    /// <summary>
    /// An object that used to carry a Canvas has its RectTransform driven by that Canvas,
    /// so the serialised scale and size are usually zero and simply never used. Delete the
    /// Canvas and those zeros go live, collapsing the panel to an invisible point. The panel
    /// is active and correct in the hierarchy, it just has no area to draw into.
    /// </summary>
    private static void WarnIfCollapsed(GameObject panel)
    {
        if (panel == null) return;

        RectTransform rect = panel.transform as RectTransform;
        if (rect == null)
        {
            Debug.LogWarning($"[PauseMenuUI] '{panel.name}' has a plain Transform, not a RectTransform, " +
                             "so UI layout will not reach anything under it.", panel);
            return;
        }

        if (rect.localScale.x == 0f || rect.localScale.y == 0f)
        {
            Debug.LogWarning($"[PauseMenuUI] '{panel.name}' has a scale of zero, so it is invisible no matter what " +
                             "is inside it. This happens after deleting a Canvas component. Set Scale to 1, 1, 1.", panel);
        }
        else if (rect.rect.width == 0f || rect.rect.height == 0f)
        {
            Debug.LogWarning($"[PauseMenuUI] '{panel.name}' has zero width or height, so nothing inside it can draw. " +
                             "Set its anchors to stretch and the offsets to 0.", panel);
        }
    }

    private static void SetChildrenActive(Transform root)
    {
        foreach (Transform child in root)
        {
            child.gameObject.SetActive(true);
            SetChildrenActive(child);
        }
    }

    private static void HookButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null) button.onClick.AddListener(action);
    }

    private static void UnhookButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null) button.onClick.RemoveListener(action);
    }

    private static void WarnIfButton(GameObject panel, string fieldName)
    {
        if (panel == null || panel.GetComponent<Selectable>() == null) return;

        Debug.LogWarning($"[PauseMenuUI] '{fieldName}' is set to '{panel.name}', which is a button rather than a panel. " +
                         "This gets hidden whenever the button column is shown, so that button will disappear. " +
                         "Leave the field empty until the real panel exists.", panel);
    }
}
