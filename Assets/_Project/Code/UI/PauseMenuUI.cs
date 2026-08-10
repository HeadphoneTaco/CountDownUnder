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
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    private enum Panel { Root, Settings, Controls }

    [Header("Panels")]
    [Tooltip("Parent of the whole pause overlay. Toggled on and off wholesale.")]
    [SerializeField] private GameObject _pauseRoot;

    [Tooltip("The button column: Resume, Settings, Controls, Quit.")]
    [SerializeField] private GameObject _buttonsPanel;

    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private GameObject _controlsPanel;

    [Header("Gamepad Focus")]
    [Tooltip("Selected when the menu opens, so a controller has somewhere to start.")]
    [SerializeField] private Selectable _firstSelected;

    [SerializeField] private Selectable _settingsFirstSelected;
    [SerializeField] private Selectable _controlsFirstSelected;

    [Header("Scenes")]
    [Tooltip("Scene name for the Quit To Menu button. Must be in Build Settings.")]
    [SerializeField] private string _mainMenuSceneName = "MainMenu";

    private Panel _current = Panel.Root;

    private void Awake()
    {
        if (_pauseRoot == null)
        {
            Debug.LogError("[PauseMenuUI] No pause root assigned.", this);
            enabled = false;
            return;
        }

        _pauseRoot.SetActive(false);
    }

    private void OnEnable()
    {
        EventManager.GamePauseChanged += OnPauseChanged;
    }

    private void OnDisable()
    {
        EventManager.GamePauseChanged -= OnPauseChanged;
    }

    private void OnPauseChanged(bool paused)
    {
        _pauseRoot.SetActive(paused);
        if (paused) ShowPanel(Panel.Root);
        else if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }

    // Button hooks. Wire these to the OnClick of the matching prefab button.

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
        _current = panel;

        if (_buttonsPanel != null) _buttonsPanel.SetActive(panel == Panel.Root);
        if (_settingsPanel != null) _settingsPanel.SetActive(panel == Panel.Settings);
        if (_controlsPanel != null) _controlsPanel.SetActive(panel == Panel.Controls);

        Selectable focus = panel switch
        {
            Panel.Settings => _settingsFirstSelected,
            Panel.Controls => _controlsFirstSelected,
            _ => _firstSelected
        };

        if (focus != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(focus.gameObject);
        }
    }
}
