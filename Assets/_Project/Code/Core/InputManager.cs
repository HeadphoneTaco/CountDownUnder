using UnityEngine;

public class InputManager : MonoBehaviour
{
    private PlayerInputs _inputs;
    private Vector2 _currentDI;
    private Vector2 _lastReadDI;

    private void Awake()
    {
        _inputs = new PlayerInputs();
    }
    private void OnEnable()
    {
        _inputs.Enable();
    }
    private void OnDisable()
    {
        _inputs.Disable();
    }
    public void Update()
    {
        _currentDI = _inputs.Player.Move.ReadValue<Vector2>();
        if (_lastReadDI != _currentDI)
        {
            OnMove(_currentDI);
            _lastReadDI = _currentDI;
        }
        if (_inputs.Player.Mist.WasReleasedThisFrame()) { EventManager.TransformationChanged?.Invoke(false); Debug.Log("Releaced"); }
        if (_inputs.Player.Mist.WasPressedThisFrame()) { EventManager.TransformationChanged?.Invoke(true); Debug.Log("Pressed"); }
        if (_inputs.Player.Jump.WasPressedThisFrame()) EventManager.JumpEvent?.Invoke();
    }
    public void OnMove(Vector2 di)
    {
        EventManager.DIEvent?.Invoke(di);
    }
}
