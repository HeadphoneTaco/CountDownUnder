using UnityEngine;

namespace Assets._Project.Code.Gameplay.Obstacles
{
    public class MovingObstacle : BaseObstacle
    {
        // idle, chase, bite, search, forget
        // do a ray cast to look for the player
        // if close to the player do a bite
        // if not close to the player move towards the player
        [Tooltip("The information about the Bite behavior, either SimpleBiteStratagy or LungeBiteStratagy")] 
        [SerializeField] private BiteInfo _biteInfo;
        [Tooltip("The information about the Search behavior, either SingleRaySearchStratagy or BoxSearchStratagy")]
        [SerializeField] private SearchInfo _searchInfo;
        [Tooltip("The information about the Idle behavior, either IdleStratagy or WanderStrategy")]
        [SerializeField] private IdleInfo _idleInfo;
        [Tooltip("The information about the Chase behavior, just ChaseStratagy for now")]
        [SerializeField] private ChaseInfo _chaseInfo;
        

        private Rigidbody2D RB;
        private Vector2 _startPosition;
        private bool _facingRight;
        private float _timeOfLastBiteStart;
        private bool _reachedStartPosition = true;



        protected override void Awake()
        {
            base.Awake();
            RB = GetComponent<Rigidbody2D>();
            _startPosition = transform.position;
        }

        private void FixedUpdate()
        {
            // if the player is known then chase until bite or lose them
            // if the player is not known then search and idle
            if (_playerController != null)
            {
                if (_timeOfLastBiteStart < _biteInfo.BiteDuration + Time.time)
                {
                    _biteInfo.BitePlayer(transform, _playerLayerIndex, _playerController, _facingRight, RB, Time.time - _timeOfLastBiteStart);
                }
                else if (_biteInfo.IsPlayerInBiteRange(transform.position, _playerController.transform.position) && Time.time - _timeOfLastBiteStart + _biteInfo.BiteDuration > _biteInfo.BiteCooldown)
                {
                    _timeOfLastBiteStart = Time.time;
                }
                else
                {
                    if((_playerController.transform.position - transform.position).magnitude > _chaseInfo.ForgetDistance)
                    {
                        _playerController = null;
                        _reachedStartPosition = false;
                        return;
                    }
                    _facingRight = _chaseInfo.Chase(transform, RB, _playerController.transform.position);
                }
            }
            else
            {
                (_facingRight, _reachedStartPosition) = _idleInfo.Idle(transform, RB, _startPosition, _reachedStartPosition);
                _searchInfo.SearchForPlayer(transform, _playerLayerIndex, out _playerController, _facingRight);
            }
            transform.localScale = new Vector3(_facingRight ? 1 : -1, 1, 1);
        }

    }
}