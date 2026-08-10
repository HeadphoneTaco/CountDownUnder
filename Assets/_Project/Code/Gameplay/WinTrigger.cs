using UnityEngine;

/// <summary>
/// Ends the run successfully when the player reaches it. Drop this on the Win object and
/// make sure its Collider2D has Is Trigger ticked.
///
/// Detects by looking for a PlayerController rather than by tag, so it cannot be fooled
/// by an untagged player or by a stray object wearing the Player tag.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class WinTrigger : MonoBehaviour
{
    [Tooltip("Ignore the trigger if the player is already dead, so a corpse sliding into it cannot win the run.")]
    [SerializeField] private bool _ignoreIfDead = true;

    private bool _fired;

    private void Reset()
    {
        // Convenience when adding the component by hand: a solid collider here would
        // block the player instead of registering them.
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning($"[WinTrigger] The Collider2D on '{name}' is not a trigger, so the player will " +
                             "bump into it rather than pass through and win. Tick Is Trigger.", this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_fired) return;

        // GetComponentInParent, because the collider that enters is often on a child of
        // the player rather than on the object holding PlayerController.
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null) return;
        if (_ignoreIfDead && player.IsDead) return;

        _fired = true;
        EventManager.PlayerWon?.Invoke();
    }
}
