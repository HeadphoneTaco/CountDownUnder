using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerStateMachine MyStateMachine;
    [HideInInspector] public Vector2 DirectionalInput = Vector2.zero;
    [HideInInspector] public bool BatInputHeld;
    [HideInInspector] public Rigidbody2D RB;
    [HideInInspector] public bool CanTransform = true;

    [Header("Player Stats")]
    [SerializeField] public float WalkSpeed;
    [SerializeField] public float FlySpeed;
    [SerializeField] public float MistSpeed;
    [SerializeField] public float MistTime;
    [SerializeField] private float _maxBatTime;
    [SerializeField] private float _batTimeDrainRate;
    [SerializeField] private float _batTimeFillRate;
    private float _currentBatTime;
    [HideInInspector] public float LastBatBreakTime;
    [SerializeField] public float TimeAfterBreakToTransform;
    [SerializeField] public float TimeBetweenMist;
    private float _lastTransformationTime;
    [SerializeField] public float DefaultGravity;


    [Header("GroundCheck")]
    [SerializeField] private float _groundCheckOffset;
    [SerializeField] private float _groundCheckDistance;
    [SerializeField] private string _groundLayerName;
    private int _groundLayerIndex;

    [Header("EatStats")]
    [SerializeField] private Vector2 BoxCastHalf;

    
    

    private void Awake()
    {
        MyStateMachine = new PlayerStateMachine(this);
        _groundLayerIndex = LayerMask.GetMask(_groundLayerName);
        RB = GetComponent<Rigidbody2D>();
    }
    private void OnEnable()
    {
        MyStateMachine.Initialize(MyStateMachine.StateIdle);
        EventManager.TransformationChanged += ChangeBatInput;
        _currentBatTime = _maxBatTime;
        LastBatBreakTime = Time.time;
    }
    private void OnDisable()
    {
        MyStateMachine.Disable();
        EventManager.TransformationChanged -= ChangeBatInput;
    }

    public void Update()
    {
        MyStateMachine.Execute();
    }

    public bool IsGrounded()
    {
        return Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y) + _groundCheckOffset * Vector2.down, Vector2.down, _groundCheckDistance, _groundLayerIndex);
    }
    public void ChangeDI(Vector2 directionalInput)
    {
        DirectionalInput = directionalInput;
    }
    public void ChangeBatInput(bool batInputHeld)
    {
        BatInputHeld = batInputHeld;
        if (CanTransform && Time.time - _lastTransformationTime > TimeBetweenMist && Time.time - LastBatBreakTime > TimeAfterBreakToTransform)
        {
            MyStateMachine.ChangeState(MyStateMachine.StateMist);
        }
    }
    public bool ReduceBatTime()
    {
        _currentBatTime = Mathf.Clamp(_currentBatTime - _batTimeDrainRate * Time.deltaTime, 0, _maxBatTime);
        if (_currentBatTime == 0)
        {
            LastBatBreakTime = Time.time;
            return true;
        }
        return false;
    }
    public void IncreaseBatTime()
    {
        if (_currentBatTime >= _maxBatTime)
        {
            _currentBatTime = _maxBatTime;
        }
        else
        {
            _currentBatTime = Mathf.Clamp( _currentBatTime + _batTimeFillRate * Time.deltaTime, 0, _maxBatTime );
        }
    }
}
