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
    private bool _transformationReady;
    private float _invincibleUntil;

    [Header("Player Stats")]
    private float _currentBatTime;
    [HideInInspector] public float LastBatBreakTime;
    private float _transformationCooldown;

    public BatInfo BatInfo;
    public CountInfo CountInfo;
    public MistInfo MistInfo;
    public HitInfo HitInfo;


    [Header("GroundCheck")]
    [SerializeField] private float _groundCheckOffset;
    [SerializeField] private float _groundCheckDistance;
    [SerializeField] private string _groundLayerName;
    private int _groundLayerIndex;
    [HideInInspector] public bool IsGrounded;
    [Header("VictimCheck")]
    [SerializeField] private string _victimLayerName;
    [HideInInspector] public int VictimLayerIndex;
    [HideInInspector] public Victim Food;

    [Header("Player Animator")]
    [SerializeField] private Animator _animator;
    [SerializeField] private GameObject _bat;
    [HideInInspector] public PlayerAnimator MyAnimator;

    [Header("Player Particles")]
    [SerializeField] public ParticleSystem MistParticlesPerson;
    [SerializeField] public ParticleSystem MistParticlesBat;
    [SerializeField] public ParticleSystem MistParticlesTrail;

    [Tooltip("The BloodSplatter under Particles. One-shot burst fired when a bite lands. " +
             "Leave empty and a child named BloodSplatter is found automatically.")]
    [SerializeField] public ParticleSystem BloodSplatter;
    

    private void Awake()
    {
        MyStateMachine = new PlayerStateMachine(this);
        MyAnimator = new PlayerAnimator(_bat, _animator);
        _groundLayerIndex = LayerMask.GetMask(_groundLayerName);
        VictimLayerIndex = LayerMask.GetMask(_victimLayerName);
        RB = GetComponent<Rigidbody2D>();
        ResolveBloodSplatter();

        // Max blood is the divisor for every normalised health value the HUD reads.
        // At zero that division is 0/0, so the bar gets NaN and the player starts empty.
        if (HitInfo.MaxBloodPoints <= 0f)
        {
            Debug.LogError($"[PlayerController] HitInfo '{HitInfo.name}' has MaxBloodPoints of {HitInfo.MaxBloodPoints}. " +
                           "Falling back to 100 so the game is playable. Set a real value on the asset.", HitInfo);
            HitInfo.MaxBloodPoints = 100f;
        }

        _CurrentBlood = HitInfo.MaxBloodPoints;
        RB.gravityScale = CountInfo.DefaultGravity;
    }
    private void OnEnable()
    {
        MyStateMachine.Initialize(MyStateMachine.StateIdle);
        MyAnimator.Initialize(PlayerAnimationState.IDLE);
        EventManager.TransformationChanged += ChangeBatInput;
        _currentBatTime = BatInfo.MaxBatTime;
        LastBatBreakTime = Time.time;
    }
    private void OnDisable()
    {
        MyStateMachine.Disable();
        EventManager.TransformationChanged -= ChangeBatInput;
    }

    private void Start()
    {
        // PlayerHealthChange only fires on a change, so a HUD that subscribes in OnEnable
        // would sit on whatever fill the prefab was saved with until the first hit lands.
        // Start runs after every OnEnable, so everyone is listening by now.
        EventManager.PlayerHealthChange?.Invoke(_CurrentBlood / HitInfo.MaxBloodPoints);
        BroadcastBatTime();
    }

    public void Update()
    {
        // FixedUpdate already stops on its own at timeScale 0, but Update does not,
        // so the state machine would keep ticking behind the pause menu.
        if (IsDead || PauseManager.IsPaused) return;

        ApplyPassiveDrain();
        if (IsDead) return;

        MyStateMachine.Execute();
    }
    public void FixedUpdate()
    {
        if (IsDead || PauseManager.IsPaused) return;
        if (RB.linearVelocityX != 0) transform.localScale = new Vector3(Mathf.Sign(RB.linearVelocityX), 1, 1);
        IsGrounded = GroundedCheck();
        if (!_transformationReady) ReduceTransformationCooldown();
        MyStateMachine.FixedUpdate();
    }

    /// <summary>
    /// Falls back to a child named BloodSplatter when the field was left empty, so the
    /// effect works without a manual drag. An explicit assignment always wins, and the
    /// fallback says what it picked so a rename does not fail silently.
    /// </summary>
    private void ResolveBloodSplatter()
    {
        if (BloodSplatter != null) return;

        foreach (ParticleSystem ps in GetComponentsInChildren<ParticleSystem>(true))
        {
            if (!ps.name.Contains("BloodSplatter")) continue;

            BloodSplatter = ps;
            Debug.Log($"[PlayerController] Blood Splatter was empty, using the child '{ps.name}'. " +
                      "Assign it in the Inspector to be explicit.", this);
            return;
        }

        Debug.LogWarning("[PlayerController] No Blood Splatter assigned and no child named BloodSplatter found, " +
                         "so eating will not spray.", this);
    }

    private bool GroundedCheck()
    {
        return Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y) + _groundCheckOffset * Vector2.down, Vector2.down, _groundCheckDistance, _groundLayerIndex);
    }
    public void ChangeDI(Vector2 directionalInput)
    {
        DirectionalInput = directionalInput;
    }
    private void ReduceTransformationCooldown()
    {
        if (IsGrounded) _transformationCooldown -= Time.fixedDeltaTime;
        else _transformationCooldown -= Time.fixedDeltaTime / MistInfo.TimeBetweenFallReduce;
        if (_transformationCooldown < 0) _transformationReady = true;
    }
    public void ChangeBatInput(bool batInputHeld)
    {
        BatInputHeld = batInputHeld;
        if ((CanTransform && _transformationReady && Time.time - LastBatBreakTime > MistInfo.TimeAfterBreakToTransform) || RB.gravityScale == 0)
        {
            _transformationCooldown = MistInfo.TimeBetweenMist;
            _transformationReady = false;
            MyStateMachine.ChangeState(MyStateMachine.StateMist);
        }
    }
    public void Jump()
    {
        if (IsGrounded)
        {
            RB.AddForce(Vector2.up * CountInfo.JumpForce, ForceMode2D.Impulse);
        }
    }
    
    public bool ReduceBatTime()
    {
        _currentBatTime = Mathf.Clamp(_currentBatTime - BatInfo.BatTimeDrainRate * Time.deltaTime, 0, BatInfo.MaxBatTime);
        BroadcastBatTime();
        if (_currentBatTime == 0)
        {
            LastBatBreakTime = Time.time;
            return true;
        }
        return false;
    }
    public void IncreaseBatTime(float Reducer)
    {
        if (_currentBatTime >= BatInfo.MaxBatTime)
        {
            _currentBatTime = BatInfo.MaxBatTime;
        }
        else
        {
            _currentBatTime = Mathf.Clamp( _currentBatTime + BatInfo.BatTimeFillRate * Time.deltaTime / Reducer, 0, BatInfo.MaxBatTime );
        }
        BroadcastBatTime();
    }

    /// <summary>
    /// Bat time was tracked but never announced, so the meter had nothing to listen to.
    /// </summary>
    private void BroadcastBatTime()
    {
        if (BatInfo.MaxBatTime <= 0f) return;
        EventManager.BatTimeChanged?.Invoke(_currentBatTime / BatInfo.MaxBatTime);
    }
    /// <summary>
    /// The slow bleed that makes hunting compulsory. Kept separate from ChangeBloodPoints
    /// because that one raises PlayerHurt on any loss, which would fire the hit flash and
    /// the ouch sound every single frame.
    /// </summary>
    private void ApplyPassiveDrain()
    {
        float drain = HitInfo.PassiveBloodDrainRate * Time.deltaTime;
        if (drain <= 0f) return;

        ChangeBloodPoints(-drain, countsAsHit: false);
    }

    public void ChangeBloodPoints(float ChangeBy) => ChangeBloodPoints(ChangeBy, true);

    public void ChangeBloodPoints(float ChangeBy, bool countsAsHit)
    {
        if (ChangeBy == 0) Debug.Log("No Change in Blood");
        else if (ChangeBy < 0)
        {
            // Must be <= rather than <. Landing exactly on zero used to take the else
            // branch, leaving the player alive on an empty bar with Die never called.
            if (_CurrentBlood + ChangeBy <= 0)
            {
                _CurrentBlood = 0;
                Die();
            }
            else _CurrentBlood += ChangeBy;
            if (countsAsHit) EventManager.PlayerHurt?.Invoke();
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
    /// <summary>
    /// Kill the player from outside, for example when the sun comes up. Blood loss goes
    /// through ChangeBloodPoints instead and arrives at the same place.
    /// </summary>
    public void Kill(DeathCause cause)
    {
        Die(cause);
    }

    private void Die(DeathCause cause = DeathCause.BloodLoss)
    {
        // ChangeBloodPoints can be reached more than once in the frame a run ends, and a
        // second death would restart the whole end sequence.
        if (IsDead) return;
        IsDead = true;

        // Stop dead rather than sliding onward under the last input.
        DirectionalInput = Vector2.zero;
        if (RB != null) RB.linearVelocity = Vector2.zero;

        // InstanceIfExists throughout: the creating getter would rebuild a manager during
        // a scene transition and leak it into the closing scene.
        if (PauseManager.InstanceIfExists != null) PauseManager.InstanceIfExists.SetPauseAllowed(false);
        if (InputManager.InstanceIfExists != null) InputManager.InstanceIfExists.SetGameplayInputEnabled(false);

        EventManager.PlayerDied?.Invoke(cause);
    }

    /// <summary>True while the player cannot be hit again. Read by anything that wants to flicker the sprite.</summary>
    public bool IsInvincible => Time.time < _invincibleUntil;

    /// <summary>Set once blood hits zero. The state machine stops ticking from that point.</summary>
    public bool IsDead { get; private set; }

    public void TakeDamage(float Damage, Vector2 KnockbackForce, float StunTime)
    {
        // Without this, an obstacle whose collider overlaps the player applies damage
        // every physics tick. That drains blood in one gulp and makes the hit flash
        // strobe once per frame, which reads as a rendering bug rather than as damage.
        if (IsInvincible) return;

        _invincibleUntil = Time.time + HitInfo.InvincibilityTime;

        // take damage
        ChangeBloodPoints(-Damage);
        // apply knockback
        RB.AddForce(KnockbackForce, ForceMode2D.Impulse);
        // apply stun
        // (stun logic would go here)
    }
}
