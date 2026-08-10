using UnityEngine;

public class Victim : MonoBehaviour
{
    [SerializeField] private float _startBloodPoints = 100f;

    //[Header("Death")]
    //[Tooltip("Animator on this NPC. Its controller is swapped to the death one when drained.")]
    //[SerializeField] private Animator _animator;
    //[Tooltip("Controller holding the death animation. Leave empty to skip straight to despawn.")]
    //[SerializeField] private RuntimeAnimatorController _deathController;
    [Tooltip("Seconds the body stays in the scene after death before it is destroyed.")]
    [SerializeField] private float _despawnDelay = 2f;

    private float _currentBloodPoints;
    private bool _isDead = false;
    private Collider2D _collider;
    [SerializeField] private Animator _anim;
    /// <summary>
    /// put the victim's collider slightly in the positive z direction of the line the player runs along
    /// make sure they are on the "Victims" layer, the player detects them by layer and not by tag
    /// </summary>
    void Start()
    {
        _collider = GetComponent<Collider2D>();
        //if (_animator == null) _animator = GetComponentInChildren<Animator>();
        _currentBloodPoints = _startBloodPoints;

        // A victim with no blood dies on the first bite and pays out nothing, which reads in game
        // as the bite silently doing nothing at all. Worth saying out loud.
        if (_startBloodPoints <= 0f)
        {
            Debug.LogWarning($"Victim on '{name}' has {_startBloodPoints} starting blood, so it dies " +
                             "on the first bite and awards nothing. Set a positive value.", this);
        }
    }
    public bool GetBit()
    {
        return _isDead;
    }
    public (bool, float) DrainBlood(float drainRate)
    {
        // Guard so a second bite on a corpse cannot pay out again or restart the death sequence.
        if (_isDead) return (true, 0f);

        if (_currentBloodPoints - drainRate <= 0)
        {
            float remaining = _currentBloodPoints;
            _currentBloodPoints = 0f;
            Die();
            return (true, remaining);
        }
        else
        {
            _currentBloodPoints -= drainRate;
            return (false, drainRate);
        }
    }

    // Stop being bitable immediately, play the death clip, then clear the body so drained NPCs do
    // not pile up over a run.
    private void Die()
    {
        _isDead = true;
        if (_collider != null) _collider.enabled = false;

        EventManager.VictimDrained?.Invoke();

        //if (_animator != null && _deathController != null)
        //    _animator.runtimeAnimatorController = _deathController;
        _anim.SetTrigger("Death");
        Destroy(gameObject, Mathf.Max(0f, _despawnDelay));
    }
}
