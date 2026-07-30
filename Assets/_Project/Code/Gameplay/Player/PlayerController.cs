using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerStateMachine MyStateMachine;
    [HideInInspector] public Vector2 DirectionalInput = Vector2.zero;
    [HideInInspector] public bool BatInputHeld;
    [HideInInspector] public Rigidbody2D RB;
    [HideInInspector] public bool CanTransform = true;
    private float _CurrentBlood;

    [Header("Player Stats")]
    private float _currentBatTime;
    [HideInInspector] public float LastBatBreakTime;
    private float _lastTransformationTime;

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
    [HideInInspector] public int VictimLayerIndex;
    [HideInInspector] public Victim Food;

    
    

    private void Awake()
    {
        MyStateMachine = new PlayerStateMachine(this);
        _groundLayerIndex = LayerMask.GetMask(_groundLayerName);
        VictimLayerIndex = LayerMask.GetMask(_victimLayerName);
        RB = GetComponent<Rigidbody2D>();
        _CurrentBlood = HitInfo.MaxBloodPoints;
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
    public void FixedUpdate()
    {
        MyStateMachine.FixedUpdate();
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
    public void ChangeBloodPoints(float ChangeBy)
    {
        if (ChangeBy == 0) Debug.Log("No Change in Blood");
        else if (ChangeBy < 0)
        {
            if (_CurrentBlood + ChangeBy < 0)
            {
                _CurrentBlood = 0;
                Die();
            }
            else _CurrentBlood += ChangeBy;
            EventManager.PlayerHurt?.Invoke();
        }
        else
        {
            if (_CurrentBlood + ChangeBy > HitInfo.MaxBloodPoints)
            {
                _CurrentBlood = HitInfo.MaxBloodPoints;
            }
            else _CurrentBlood += ChangeBy;
        }
        EventManager.PlayerHealthChange?.Invoke(_CurrentBlood/HitInfo.MaxBloodPoints);
    }
    private void Die()
    {

    }
}
