using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerStateMachine MyStateMachine;
    [HideInInspector] public Vector2 DirectionalInput = Vector2.zero;
    [HideInInspector] public bool BatInputHeld;
    [HideInInspector] public Rigidbody2D RB;
    [HideInInspector] public bool CanTransform = true;

    [Header("Player Stats")]
    //[SerializeField] public float WalkSpeed;
    //[SerializeField] public float FlySpeed;
    //[SerializeField] public float MistSpeed;
    //[SerializeField] public float MistTime;
    //[SerializeField] public float MaxBatTime;
    //[SerializeField] public float BatTimeDrainRate;
    //[SerializeField] public float _batTimeFillRate;
    private float _currentBatTime;
    [HideInInspector] public float LastBatBreakTime;
    //[SerializeField] public float TimeAfterBreakToTransform;
    //[SerializeField] public float TimeBetweenMist;
    private float _lastTransformationTime;
    //[SerializeField] public float DefaultGravity;

    public BatInfo BatInfo;
    public CountInfo CountInfo;
    public MistInfo MistInfo;
    public HitInfo HitInfo;


    [Header("GroundCheck")]
    [SerializeField] private float _groundCheckOffset;
    [SerializeField] private float _groundCheckDistance;
    [SerializeField] private string _groundLayerName;
    private int _groundLayerIndex;
    [Header("VictimCheck")]
    [SerializeField] private string _victimLayerName;
    private int _victimLayerIndex;

    //[Header("EatStats")]
    //[SerializeField] public Vector2 BoxCastHalf;

    
    

    private void Awake()
    {
        MyStateMachine = new PlayerStateMachine(this);
        _groundLayerIndex = LayerMask.GetMask(_groundLayerName);
        _victimLayerIndex = LayerMask.GetMask(_victimLayerName);
        RB = GetComponent<Rigidbody2D>();
    }
    private void OnEnable()
    {
        MyStateMachine.Initialize(MyStateMachine.StateIdle);
        EventManager.TransformationChanged += ChangeBatInput;
        _currentBatTime = BatInfo.MaxBatTime;
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
        if (CanTransform && Time.time - _lastTransformationTime > MistInfo.TimeBetweenMist && Time.time - LastBatBreakTime > MistInfo.TimeAfterBreakToTransform)
        {
            MyStateMachine.ChangeState(MyStateMachine.StateMist);
        }
    }
    public bool ReduceBatTime()
    {
        _currentBatTime = Mathf.Clamp(_currentBatTime - BatInfo.BatTimeDrainRate * Time.deltaTime, 0, BatInfo.MaxBatTime);
        if (_currentBatTime == 0)
        {
            LastBatBreakTime = Time.time;
            return true;
        }
        return false;
    }
    public void IncreaseBatTime()
    {
        if (_currentBatTime >= BatInfo.MaxBatTime)
        {
            _currentBatTime = BatInfo.MaxBatTime;
        }
        else
        {
            _currentBatTime = Mathf.Clamp( _currentBatTime + BatInfo.BatTimeFillRate * Time.deltaTime, 0, BatInfo.MaxBatTime );
        }
    }
}
