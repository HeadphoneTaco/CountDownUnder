using UnityEngine;

namespace Assets._Project.Code.Gameplay.Obstacles
{
    /// <summary>
    /// Drives a single looping locomotion clip from how fast the body is actually moving.
    /// One clip therefore covers standing still, wandering back to the start position and
    /// chasing the player, with no extra animator states or transitions to maintain.
    /// Put this on the same object as the SpriteRenderer and the Animator.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class ObstacleAnimator : MonoBehaviour
    {
        private static readonly int SpeedMultiplierHash = Animator.StringToHash("SpeedMultiplier");

        [Tooltip("Horizontal speed, in units per second, at which the cycle plays at its authored rate. Set this to roughly the chase speed.")]
        [SerializeField] private float _referenceSpeed = 2f;

        [Tooltip("Slowest the cycle is allowed to play. Zero freezes the animal on a frame when it stops moving.")]
        [SerializeField] private float _minimumPlaybackRate = 0f;

        [Tooltip("Fastest the cycle is allowed to play, so a knockback or a lunge cannot spin the legs up.")]
        [SerializeField] private float _maximumPlaybackRate = 2f;

        [Tooltip("Seconds the playback rate takes to catch up to the body speed. Stops the cycle snapping when a chase starts.")]
        [SerializeField] private float _smoothingTime = 0.1f;

        private Animator _animator;
        private Rigidbody2D _rigidbody;
        private float _playbackRate;
        private float _playbackRateVelocity;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _rigidbody = GetComponentInParent<Rigidbody2D>();

            // With no body there is no speed to read, so the multiplier would sit at zero and the
            // animal would ship as a statue. That reads as broken art rather than a missing component,
            // so it is worth saying out loud.
            if (_rigidbody == null)
            {
                Debug.LogWarning($"ObstacleAnimator on '{name}' found no Rigidbody2D on itself or a parent, " +
                                 "so the locomotion cycle will never play.", this);
            }

            // A reference speed of zero divides the whole thing by nothing and pins the cycle at its
            // maximum rate forever.
            if (_referenceSpeed <= 0f)
            {
                Debug.LogWarning($"ObstacleAnimator on '{name}' has a reference speed of {_referenceSpeed}, " +
                                 "which is not a usable divisor. Falling back to 1.", this);
                _referenceSpeed = 1f;
            }

            if (_maximumPlaybackRate < _minimumPlaybackRate)
            {
                Debug.LogWarning($"ObstacleAnimator on '{name}' has a maximum playback rate below its minimum. " +
                                 "Clamping the maximum up to the minimum.", this);
                _maximumPlaybackRate = _minimumPlaybackRate;
            }

            _playbackRate = _minimumPlaybackRate;
        }

        private void Update()
        {
            if (_rigidbody == null) return;

            float target = Mathf.Clamp(Mathf.Abs(_rigidbody.linearVelocityX) / _referenceSpeed,
                                       _minimumPlaybackRate, _maximumPlaybackRate);

            _playbackRate = Mathf.SmoothDamp(_playbackRate, target, ref _playbackRateVelocity, _smoothingTime);
            _animator.SetFloat(SpeedMultiplierHash, _playbackRate);
        }
    }
}
