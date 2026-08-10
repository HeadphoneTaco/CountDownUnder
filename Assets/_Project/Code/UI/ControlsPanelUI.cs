using UnityEngine;

/// <summary>
/// Shows and hides the controls diagram. That is the whole job.
///
/// Both key layouts are live at the same time in the input asset, WASD/Z/X and
/// arrows/comma/period, so there is nothing to switch between. The panel just has
/// to display the sheet. Assign the [UI]ControlsA prefab instance to Diagram.
/// </summary>
public class ControlsPanelUI : MonoBehaviour
{
    [Tooltip("Instance of [UI]ControlsA. Toggled on and off by the Controls button.")]
    [SerializeField] private GameObject _diagram;

    [Tooltip("Hide the diagram on start so it does not sit over the menu on first open.")]
    [SerializeField] private bool _hiddenOnStart = true;

    public bool IsVisible => _diagram != null && _diagram.activeSelf;

    private void Awake()
    {
        if (_diagram == null)
        {
            Debug.LogError("[ControlsPanelUI] No diagram assigned, the Controls button will do nothing.", this);
            enabled = false;
            return;
        }

        if (_hiddenOnStart) _diagram.SetActive(false);
    }

    /// <summary>Wire this to the Controls button OnClick.</summary>
    public void Show()
    {
        if (_diagram != null) _diagram.SetActive(true);
    }

    public void Hide()
    {
        if (_diagram != null) _diagram.SetActive(false);
    }

    public void Toggle()
    {
        if (_diagram != null) _diagram.SetActive(!_diagram.activeSelf);
    }
}
